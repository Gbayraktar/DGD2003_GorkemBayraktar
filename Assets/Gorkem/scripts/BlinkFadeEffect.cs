using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlinkFadeEffect : MonoBehaviour
{
    [Header("Göz Kapakları (UI Image RectTransform)")]
    [Tooltip("Ekranın üstünden aşağı inen siyah panel. Pivot Y = 0 (alt), anchor = üst tam genişlik önerilir.")]
    [SerializeField] private RectTransform topLid;

    [Tooltip("Ekranın altından yukarı çıkan siyah panel. Pivot Y = 1 (üst), anchor = alt tam genişlik önerilir.")]
    [SerializeField] private RectTransform bottomLid;

    [Header("Süreler (saniye)")]
    [SerializeField] private float halfCloseDuration = 0.25f;
    [SerializeField] private float reopenDuration    = 0.15f;
    [SerializeField] private float pauseBetween      = 0.10f;
    [SerializeField] private float fullCloseDuration = 0.40f;
    [SerializeField] private float blackHoldDuration = 0.30f;

    [Header("Kapanma Miktarları (0 = açık, 1 = tam kapalı)")]
    [Range(0f, 1f)] [SerializeField] private float firstBlinkAmount = 0.60f;
    [Range(0f, 1f)] [SerializeField] private float reopenAmount     = 0.30f;

    [Header("Animasyon Eğrisi")]
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Yeniden Başlatma")]
    [Tooltip("Boş bırakılırsa aktif sahne yeniden yüklenir.")]
    [SerializeField] private string restartSceneName = "";

    private float _topLidHeight;
    private float _bottomLidHeight;
    private bool  _isPlaying;

    private void Awake()
    {
        if (topLid != null)
        {
            _topLidHeight = topLid.rect.height;
            SetLidProgress(topLid, 0f, true);
        }

        if (bottomLid != null)
        {
            _bottomLidHeight = bottomLid.rect.height;
            SetLidProgress(bottomLid, 0f, false);
        }
    }

    private void OnEnable()
    {
        GameTimer.OnTimeUp += HandleTimeUp;
    }

    private void OnDisable()
    {
        GameTimer.OnTimeUp -= HandleTimeUp;
    }

    private void HandleTimeUp()
    {
        if (_isPlaying) return;
        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        _isPlaying = true;

        yield return AnimateLids(0f, firstBlinkAmount, halfCloseDuration);
        yield return AnimateLids(firstBlinkAmount, reopenAmount, reopenDuration);

        if (pauseBetween > 0f)
            yield return new WaitForSeconds(pauseBetween);

        yield return AnimateLids(reopenAmount, 1f, fullCloseDuration);

        if (blackHoldDuration > 0f)
            yield return new WaitForSeconds(blackHoldDuration);

        RestartGame();
    }

    private IEnumerator AnimateLids(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            ApplyProgress(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = easeCurve.Evaluate(Mathf.Clamp01(t / duration));
            ApplyProgress(Mathf.LerpUnclamped(from, to, k));
            yield return null;
        }

        ApplyProgress(to);
    }

    private void ApplyProgress(float progress)
    {
        if (topLid    != null) SetLidProgress(topLid,    progress, true);
        if (bottomLid != null) SetLidProgress(bottomLid, progress, false);
    }

    // progress: 0 = tamamen açık (ekran dışı), 1 = ekranı yarıya kadar kapatmış (tam kapalı)
    private void SetLidProgress(RectTransform lid, float progress, bool isTop)
    {
        float height = isTop ? _topLidHeight : _bottomLidHeight;
        if (height <= 0f) height = lid.rect.height;

        // Üst kapak için: progress 0 → y = +height/2 (ekran dışı), progress 1 → y = -height/2 (aşağı inmiş)
        // Alt kapak için: progress 0 → y = -height/2 (ekran dışı), progress 1 → y = +height/2 (yukarı çıkmış)
        float targetY = isTop
            ? Mathf.Lerp(height,  0f, progress)
            : Mathf.Lerp(-height, 0f, progress);

        Vector2 pos = lid.anchoredPosition;
        pos.y = targetY;
        lid.anchoredPosition = pos;
    }

    private void RestartGame()
    {
        if (!string.IsNullOrEmpty(restartSceneName))
            SceneManager.LoadScene(restartSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
