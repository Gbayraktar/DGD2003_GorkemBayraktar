using UnityEngine;

/// <summary>
/// Bu objenin trigger collider'ına oyuncu girdiğinde verilen sesi yalnızca
/// bir kez çalar. Sonraki girişlerde tekrar çalmaz.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerSfxOnce : MonoBehaviour
{
    [Header("Ses")]
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Filtre")]
    [Tooltip("Bu tag'e sahip objeyle temasta tetiklenir. Boş bırakılırsa her şeyi tetikler.")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Davranış")]
    [Tooltip("Açıksa ses çaldıktan sonra bu component devre dışı kalır.")]
    [SerializeField] private bool disableAfterPlay = true;
    [Tooltip("Açıksa ses çaldıktan sonra GameObject yok edilir.")]
    [SerializeField] private bool destroyAfterPlay = false;

    private bool _played;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_played) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        _played = true;

        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);

        if (destroyAfterPlay)
            Destroy(gameObject);
        else if (disableAfterPlay)
            enabled = false;
    }
}
