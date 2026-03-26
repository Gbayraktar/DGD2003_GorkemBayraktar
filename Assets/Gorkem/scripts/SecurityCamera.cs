using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Tespit")]
    [SerializeField] private float detectionRange    = 10f;
    [SerializeField] private float fieldOfView       = 60f;
    [SerializeField] private LayerMask playerLayer   = ~0;
    [SerializeField] private LayerMask obstacleMask  = ~0;
    [Tooltip("Modelin yüzü yanlış yöne bakıyorsa buradan düzelt. Örnek: (0, 90, 0) modeli 90 derece döndürür.")]
    [SerializeField] private Vector3 directionOffset = Vector3.zero;

    [Header("Devriye Dönüşü")]
    [SerializeField] private float patrolSpeed      = 20f;
    [SerializeField] private float patrolAngle      = 60f;

    [Header("Takip")]
    [SerializeField] private float trackingSpeed    = 5f;

    [Header("Görsel")]
    [SerializeField] private Color normalColor      = Color.green;
    [SerializeField] private Color alertColor       = Color.red;
    [SerializeField] private Renderer cameraLight;

    private Transform _player;
    private bool       _playerDetected  = false;
    private float      _patrolTimer     = 0f;
    private Quaternion _initialRotation;
    private Vector3    _facingDirection;

    private void Start()
    {
        _initialRotation = transform.rotation;
        _facingDirection = Quaternion.Euler(directionOffset) * Vector3.forward;

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        // directionOffset değişirse runtime'da da güncel kalsın
        _facingDirection = Quaternion.Euler(directionOffset) * Vector3.forward;

        _playerDetected = CanSeePlayer();

        if (_playerDetected)
            TrackPlayer();
        else
            Patrol();

        UpdateLight();
    }

    private bool CanSeePlayer()
    {
        Vector3 dirToPlayer  = _player.position - transform.position;
        float   distance     = dirToPlayer.magnitude;

        if (distance > detectionRange) return false;

        // Offset uygulanmış gerçek bakış yönü
        Vector3 actualForward = transform.TransformDirection(_facingDirection);
        float   angle         = Vector3.Angle(actualForward, dirToPlayer);

        if (angle > fieldOfView * 0.5f) return false;

        // Araya engel var mı kontrol et
        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, distance, obstacleMask))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    private void TrackPlayer()
    {
        Vector3 actualForward = transform.TransformDirection(_facingDirection);
        Vector3 dirToPlayer   = (_player.position - transform.position).normalized;

        // Offset farkını hesaplayıp dönüşe ekle
        Quaternion offsetCorrection = Quaternion.FromToRotation(actualForward, transform.forward);
        Quaternion targetRotation   = Quaternion.LookRotation(dirToPlayer) * offsetCorrection;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingSpeed * Time.deltaTime);
    }

    private void Patrol()
    {
        _patrolTimer += Time.deltaTime * patrolSpeed;
        float yOffset = Mathf.Sin(_patrolTimer * Mathf.Deg2Rad) * patrolAngle;

        Quaternion swingRotation = _initialRotation * Quaternion.Euler(0f, yOffset, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, swingRotation, Time.deltaTime * 3f);
    }

    private void UpdateLight()
    {
        if (cameraLight == null) return;

        cameraLight.material.color = _playerDetected ? alertColor : normalColor;
    }

    // Editörde görüş alanını görselleştir
    private void OnDrawGizmos()
    {
        Color coneColor = _playerDetected ? Color.red : Color.green;
        coneColor.a     = 0.15f;

        DrawFOVCone(coneColor);

        // Tespit mesafesi çemberi
        Gizmos.color = _playerDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void DrawFOVCone(Color color)
    {
        float   halfFOV      = fieldOfView * 0.5f;
        int     segments     = 20;
        float   angleStep    = fieldOfView / segments;

        // Offset uygulanmış gerçek bakış yönünü kullan
        Vector3 facing = Application.isPlaying
            ? transform.TransformDirection(_facingDirection)
            : transform.TransformDirection(Quaternion.Euler(directionOffset) * Vector3.forward);

        Gizmos.color = color;

        Vector3 prevPoint = transform.position +
            Quaternion.AngleAxis(-halfFOV, transform.up) * facing * detectionRange;

        for (int i = 1; i <= segments; i++)
        {
            float   angle     = -halfFOV + angleStep * i;
            Vector3 nextPoint = transform.position +
                Quaternion.AngleAxis(angle, transform.up) * facing * detectionRange;

            Gizmos.DrawLine(transform.position, nextPoint);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Vector3 leftEdge  = Quaternion.AngleAxis(-halfFOV, transform.up) * facing * detectionRange;
        Vector3 rightEdge = Quaternion.AngleAxis( halfFOV, transform.up) * facing * detectionRange;
        Gizmos.DrawLine(transform.position, transform.position + leftEdge);
        Gizmos.DrawLine(transform.position, transform.position + rightEdge);
    }
}
