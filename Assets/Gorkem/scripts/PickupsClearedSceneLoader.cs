using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Sahnedeki toplanabilir/satılabilir <see cref="PickupObject"/> sayısı sıfıra
/// düştükten <see cref="delayBeforeLoad"/> saniye sonra belirlenen sahneye geçiş
/// yapar. Oyun başlarken obje sayısı 0 ise yanlışlıkla tetiklenmemesi için en
/// az bir kez "obje vardı" durumunu beklemesi sağlanmıştır.
/// </summary>
public class PickupsClearedSceneLoader : MonoBehaviour
{
    [Header("Sahne Geçişi")]
    [Tooltip("Yüklenecek sahnenin adı. Build Settings içinde ekli olmalı.")]
    [SerializeField] private string nextSceneName = "";
    [Tooltip("Sahne adını boş bırakırsan bu indeks kullanılır. -1 = kullanma.")]
    [SerializeField] private int nextSceneBuildIndex = -1;
    [Tooltip("Tüm objeler bittikten kaç saniye sonra geçiş yapılsın.")]
    [SerializeField] private float delayBeforeLoad = 5f;

    [Header("Sayım Modu")]
    [Tooltip("Açıksa sadece 'sellable' (satılabilir) işaretli PickupObject'ler sayılır.\nKapalıysa sahnedeki tüm aktif PickupObject'ler sayılır.")]
    [SerializeField] private bool countOnlySellable = true;

    private bool _hasSeenAny;
    private bool _scheduled;

    private void OnEnable()
    {
        if (countOnlySellable)
            PickupObject.OnActiveSellableCountChanged += HandleCountChanged;
        else
            PickupObject.OnActiveCountChanged += HandleCountChanged;
    }

    private void OnDisable()
    {
        if (countOnlySellable)
            PickupObject.OnActiveSellableCountChanged -= HandleCountChanged;
        else
            PickupObject.OnActiveCountChanged -= HandleCountChanged;
    }

    private void Start()
    {
        int initial = countOnlySellable
            ? PickupObject.ActiveSellableCount
            : PickupObject.ActiveCount;

        if (initial > 0) _hasSeenAny = true;
    }

    private void HandleCountChanged(int newCount)
    {
        if (newCount > 0)
        {
            _hasSeenAny = true;
            return;
        }

        if (!_hasSeenAny || _scheduled) return;

        _scheduled = true;
        StartCoroutine(LoadAfterDelay());
    }

    private IEnumerator LoadAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        int currentCount = countOnlySellable
            ? PickupObject.ActiveSellableCount
            : PickupObject.ActiveCount;

        if (currentCount > 0)
        {
            _scheduled = false;
            yield break;
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else if (nextSceneBuildIndex >= 0)
            SceneManager.LoadScene(nextSceneBuildIndex);
        else
            Debug.LogWarning($"{name}: Sonraki sahne adı/indeksi atanmamış!");
    }
}
