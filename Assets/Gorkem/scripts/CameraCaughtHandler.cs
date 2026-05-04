using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Herhangi bir SecurityCamera oyuncuyu gördüğünde ekrana kırmızı bir kaplama
/// bindirir. Bu kaplama kesintisiz olarak <see cref="catchDuration"/> saniye
/// boyunca aktif kalırsa sahne yeniden yüklenir (oyun yeniden başlar).
/// Oyuncu kameralardan kaçtığında kaplama yumuşak şekilde söner ve sayaç
/// sıfırlanır.
/// </summary>
public class CameraCaughtHandler : MonoBehaviour
{
    [Header("Yakalanma")]
    [Tooltip("Kırmızı görüş kaç saniye kesintisiz sürerse oyun yeniden başlar.")]
    [SerializeField] private float catchDuration = 3f;

    [Header("Kırmızı Kaplama")]
    [Tooltip("Boş bırakılırsa otomatik olarak kendi Canvas + Image üretilir.")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private Color overlayColor   = new Color(1f, 0f, 0f, 0.55f);
    [SerializeField] private float fadeInSpeed    = 6f;
    [SerializeField] private float fadeOutSpeed   = 3f;

    [Header("Yeniden Başlatma")]
    [Tooltip("Boş bırakılırsa aktif sahne yeniden yüklenir.")]
    [SerializeField] private string restartSceneName = "";

    private readonly List<SecurityCamera> _cameras = new List<SecurityCamera>();
    private float _caughtTimer;
    private bool  _isRestarting;
    private float _currentAlpha;

    private void Awake()
    {
        if (overlayImage == null)
            overlayImage = CreateOverlayImage();

        SetAlpha(0f);
    }

    private void Start()
    {
        RefreshCameras();
    }

    private void Update()
    {
        if (_isRestarting) return;

        bool seen = IsAnyCameraSeeingPlayer();

        float targetAlpha = seen ? overlayColor.a : 0f;
        float speed       = seen ? fadeInSpeed   : fadeOutSpeed;
        _currentAlpha     = Mathf.MoveTowards(_currentAlpha, targetAlpha, speed * Time.deltaTime);
        SetAlpha(_currentAlpha);

        if (seen)
        {
            _caughtTimer += Time.deltaTime;
            if (_caughtTimer >= catchDuration)
                StartCoroutine(RestartRoutine());
        }
        else
        {
            _caughtTimer = 0f;
        }
    }

    private bool IsAnyCameraSeeingPlayer()
    {
        for (int i = 0; i < _cameras.Count; i++)
        {
            SecurityCamera cam = _cameras[i];
            if (cam == null) continue;
            if (cam.PlayerDetected) return true;
        }
        return false;
    }

    private void RefreshCameras()
    {
        _cameras.Clear();
#if UNITY_2023_1_OR_NEWER
        SecurityCamera[] found = Object.FindObjectsByType<SecurityCamera>(FindObjectsSortMode.None);
#else
        SecurityCamera[] found = Object.FindObjectsOfType<SecurityCamera>();
#endif
        _cameras.AddRange(found);
    }

    private IEnumerator RestartRoutine()
    {
        _isRestarting = true;

        while (_currentAlpha < 1f)
        {
            _currentAlpha = Mathf.MoveTowards(_currentAlpha, 1f, fadeInSpeed * 2f * Time.unscaledDeltaTime);
            SetAlpha(_currentAlpha);
            yield return null;
        }

        if (!string.IsNullOrEmpty(restartSceneName))
            SceneManager.LoadScene(restartSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SetAlpha(float a)
    {
        if (overlayImage == null) return;
        Color c = overlayColor;
        c.a = Mathf.Clamp01(a);
        overlayImage.color = c;
        overlayImage.enabled = a > 0.001f;
    }

    private Image CreateOverlayImage()
    {
        GameObject canvasGo = new GameObject("CameraCaughtCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject imgGo = new GameObject("RedOverlay");
        imgGo.transform.SetParent(canvasGo.transform, false);

        Image img = imgGo.AddComponent<Image>();
        img.raycastTarget = false;
        img.color = new Color(overlayColor.r, overlayColor.g, overlayColor.b, 0f);

        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        return img;
    }
}
