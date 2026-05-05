using System.Collections;
using UnityEngine;

/// <summary>
/// Bu objenin trigger'ına oyuncu girdiğinde:
///   - <see cref="targetToRotate"/> objesi belirtilen süre boyunca kendi
///     ekseni etrafında sürekli 360° döner.
///   - Atanmış SFX bir kez çalar.
///   - Trigger sadece bir kez tetiklenir, sonraki temaslar yok sayılır.
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerSpinAndSfx : MonoBehaviour
{
    [Header("Döndürülecek Hedef")]
    [Tooltip("Etrafında dönecek olan obje (kendi ekseni etrafında).")]
    [SerializeField] private Transform targetToRotate;

    [Header("Dönüş Ayarları")]
    [Tooltip("Toplam dönüş süresi (saniye).")]
    [SerializeField] private float duration = 5f;
    [Tooltip("Saniyede kaç derece dönsün. 1440 = saniyede 4 tur (hızlı).")]
    [SerializeField] private float degreesPerSecond = 1440f;
    [Tooltip("Dünya eksenine göre dönüş yönü. (0,1,0) = Y ekseni etrafında yatay dönüş.")]
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [Tooltip("Açıksa dönüş local space'te yapılır (objenin kendi ekseni).")]
    [SerializeField] private bool rotateInLocalSpace = true;

    [Header("Yükselme")]
    [Tooltip("Süre içinde toplam ne kadar yükselsin (metre). 0.05 = 5 cm.")]
    [SerializeField] private float riseDistance = 0.05f;
    [Tooltip("Yükselme eğrisi. Varsayılan EaseInOut yumuşak bir kalkış sağlar.")]
    [SerializeField] private AnimationCurve riseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Açıksa süre bitince obje başlangıç yüksekliğine geri inmez, yukarıda kalır.")]
    [SerializeField] private bool stayLifted = true;

    [Header("Ses")]
    [SerializeField] private AudioClip clip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("Filtre")]
    [Tooltip("Bu tag'e sahip obje tetikler. Boş bırakılırsa her şey tetikler.")]
    [SerializeField] private string requiredTag = "Player";

    [Header("Davranış")]
    [Tooltip("Açıksa tetiklendikten sonra component devre dışı kalır.")]
    [SerializeField] private bool disableAfterTrigger = true;

    private bool _triggered;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        _triggered = true;

        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);

        if (targetToRotate != null)
            StartCoroutine(SpinRoutine());
        else
            Debug.LogWarning($"{name}: TargetToRotate atanmamış, dönüş yapılmayacak.");

        if (disableAfterTrigger)
            enabled = false;
    }

    private IEnumerator SpinRoutine()
    {
        float   t         = 0f;
        Vector3 axis      = rotationAxis.sqrMagnitude > 0.0001f ? rotationAxis.normalized : Vector3.up;
        Space   space     = rotateInLocalSpace ? Space.Self : Space.World;
        Vector3 startPos  = targetToRotate.position;

        while (t < duration && targetToRotate != null)
        {
            float dt = Time.deltaTime;

            float step = degreesPerSecond * dt;
            targetToRotate.Rotate(axis, step, space);

            float k = riseCurve.Evaluate(Mathf.Clamp01(t / duration));
            Vector3 pos = targetToRotate.position;
            pos.y = startPos.y + riseDistance * k;
            targetToRotate.position = pos;

            t += dt;
            yield return null;
        }

        if (targetToRotate != null)
        {
            Vector3 finalPos = targetToRotate.position;
            finalPos.y = startPos.y + (stayLifted ? riseDistance : 0f);
            targetToRotate.position = finalPos;
        }
    }
}
