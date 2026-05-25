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
/// Sahneye elle eklemen gerekmez — SecurityCamera sahnede olduğunda otomatik oluşur.
/// </summary>
public class CameraCaughtHandler : MonoBehaviour
{
    public static CameraCaughtHandler Instance { get; private set; }

    [Header("Yakalanma")]
    [Tooltip("Kırmızı görüş kaç saniye kesintisiz sürerse oyun yeniden başlar.")]
    [SerializeField] private float catchDuration = 2f;

    [Header("Kırmızı Kaplama")]
    [Tooltip("Boş bırakılırsa otomatik olarak kendi Canvas + Image üretilir.")]
    [SerializeField] private Image overlayImage;
    [SerializeField] private Color overlayColor   = new Color(1f, 0f, 0f, 0.55f);
    [SerializeField] private float fadeInSpeed    = 6f;
    [SerializeField] private float fadeOutSpeed   = 3f;

    [Header("Yeniden Başlatma")]
    [Tooltip("Boş bırakılırsa aktif sahne yeniden yüklenir.")]
    [SerializeField] private string restartSceneName = "";

    private readonly HashSet<SecurityCamera> _cameras = new HashSet<SecurityCamera>();
    private float _caughtTimer;
    private bool  _isRestarting;
    private float _currentAlpha;

    /// <summary>
    /// Sahnedeki SecurityCamera'lar bu metodu çağırır; handler yoksa oluşturulur.
    /// </summary>
    public static void Register(SecurityCamera camera)
    {
        if (camera == null) return;
        EnsureExists();
        Instance._cameras.Add(camera);
    }

    public static void Unregister(SecurityCamera camera)
    {
        if (camera == null || Instance == null) return;
        Instance._cameras.Remove(camera);
    }

    public static void EnsureExists()
    {
        if (Instance != null) return;

#if UNITY_2023_1_OR_NEWER
        Instance = Object.FindAnyObjectByType<CameraCaughtHandler>();
#else
        Instance = Object.FindObjectOfType<CameraCaughtHandler>();
#endif
        if (Instance != null) return;

        var go = new GameObject("CameraCaughtHandler (Auto)");
        Instance = go.AddComponent<CameraCaughtHandler>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (overlayImage == null)
            overlayImage = CreateOverlayImage();

        SetAlpha(0f);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        RefreshCameras();
    }

    private void Update()
    {
        if (_isRestarting) return;

        PruneNullCameras();

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

    private void PruneNullCameras()
    {
        _cameras.RemoveWhere(c => c == null);
    }

    private bool IsAnyCameraSeeingPlayer()
    {
        foreach (SecurityCamera cam in _cameras)
        {
            if (cam != null && cam.PlayerDetected)
                return true;
        }
        return false;
    }

    private void RefreshCameras()
    {
#if UNITY_2023_1_OR_NEWER
        SecurityCamera[] found = Object.FindObjectsByType<SecurityCamera>(FindObjectsSortMode.None);
#else
        SecurityCamera[] found = Object.FindObjectsOfType<SecurityCamera>();
#endif
        foreach (SecurityCamera cam in found)
            _cameras.Add(cam);
    }

    private IEnumerator RestartRoutine()
    {
        _isRestarting = true;
        Debug.Log("[CameraCaught] Oyuncu yakalandı — sahne yeniden yükleniyor.", this);

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
