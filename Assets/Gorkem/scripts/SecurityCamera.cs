using UnityEngine;

public class SecurityCamera : MonoBehaviour
{
    [Header("Tespit")]
    [SerializeField] private float      detectionRange   = 10f;
    [SerializeField] private float      fieldOfView      = 60f;
    [SerializeField] private LayerMask  obstacleMask     = ~0;
    [Tooltip("Modelin yüzü yanlış yöne bakıyorsa buradan düzelt.")]
    [SerializeField] private Vector3    directionOffset  = Vector3.zero;
    [Tooltip("Oyuncunun hangi noktasına kitlensin? Y değerini düşürünce daha aşağıya bakar.")]
    [SerializeField] private Vector3    playerAimOffset  = new Vector3(0f, 0f, 0f);

    [Header("Kilit Sistemi")]
    [Tooltip("Oyuncu kaybolduktan kaç saniye sonra devriyeye dönülsün")]
    [SerializeField] private float lostDelay = 1.5f;

    [Header("Devriye")]
    [SerializeField] private float patrolSpeed = 20f;
    [SerializeField] private float patrolAngle = 60f;

    [Header("Takip")]
    [SerializeField] private float trackingSpeed = 8f;

    [Header("Görsel")]
    [SerializeField] private Color    normalColor  = Color.green;
    [SerializeField] private Color    alertColor   = Color.red;
    [SerializeField] private Renderer cameraLight;

    private Transform  _player;
    private bool       _playerDetected = false;
    private bool       _isLocked       = false; // Kilitli mod — flickering önler
    private float      _lostTimer      = 0f;
    private float      _patrolTimer    = 0f;
    private Quaternion _initialRotation;
    private Vector3    _facingDirection;

    public bool    PlayerDetected  => _playerDetected;
    public float   DetectionRange  => detectionRange;
    public float   FieldOfView     => fieldOfView;
    public Vector3 FacingDirection => transform.TransformDirection(Quaternion.Euler(directionOffset) * Vector3.forward);

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

        _facingDirection = Quaternion.Euler(directionOffset) * Vector3.forward;

        bool canSee = CanSeePlayer();

        if (canSee)
        {
            // Oyuncu görüldü — kilitle ve zamanlayıcıyı sıfırla
            _isLocked       = true;
            _lostTimer      = lostDelay;
            _playerDetected = true;
        }
        else if (_isLocked)
        {
            // Kilitli ama artık göremiyoruz — zamanlayıcı bitene kadar takip et
            _lostTimer -= Time.deltaTime;

            if (_lostTimer <= 0f)
            {
                _isLocked       = false;
                _playerDetected = false;
            }
        }

        if (_isLocked)
            TrackPlayer();
        else
            Patrol();

        UpdateLight();
    }

    private bool CanSeePlayer()
    {
        Vector3 aimPoint    = _player.position + playerAimOffset;
        Vector3 dirToPlayer = aimPoint - transform.position;
        float   distance    = dirToPlayer.magnitude;

        if (distance > detectionRange) return false;

        Vector3 actualForward = transform.TransformDirection(_facingDirection);
        float   angle         = Vector3.Angle(actualForward, dirToPlayer);

        if (angle > fieldOfView * 0.5f) return false;

        if (Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, distance, obstacleMask))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }

        return true;
    }

    private void TrackPlayer()
    {
        Vector3 aimPoint    = _player.position + playerAimOffset;
        Vector3 dirToPlayer = (aimPoint - transform.position).normalized;

        // Sabit offset: model yüzü Z+ değilse düzelt (her frame yeniden hesaplanmıyor)
        Quaternion facingOffset   = Quaternion.Euler(directionOffset);
        Quaternion targetRotation = Quaternion.LookRotation(dirToPlayer) * Quaternion.Inverse(facingOffset);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, trackingSpeed * Time.deltaTime);
    }

    private void Patrol()
    {
        _patrolTimer += Time.deltaTime * patrolSpeed;
        float      yOffset       = Mathf.Sin(_patrolTimer * Mathf.Deg2Rad) * patrolAngle;
        Quaternion swingRotation = _initialRotation * Quaternion.Euler(0f, yOffset, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, swingRotation, Time.deltaTime * 3f);
    }

    private void UpdateLight()
    {
        if (cameraLight == null) return;
        cameraLight.material.color = _playerDetected ? alertColor : normalColor;
    }

    private void OnDrawGizmos()
    {
        Color coneColor = _playerDetected ? Color.red : Color.green;
        coneColor.a = 0.15f;
        DrawFOVCone(coneColor);

        Gizmos.color = _playerDetected ? Color.red : Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void DrawFOVCone(Color color)
    {
        float   halfFOV   = fieldOfView * 0.5f;
        int     segments  = 20;
        float   angleStep = fieldOfView / segments;

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
