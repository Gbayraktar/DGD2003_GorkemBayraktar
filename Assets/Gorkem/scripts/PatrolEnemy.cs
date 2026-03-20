using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("Devriye Noktaları")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Ayarlar")]
    [SerializeField] private float moveSpeed    = 3f;
    [SerializeField] private float waitTime     = 1f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float stoppingDistance = 0.2f;

    [Header("Fizik")]
    [SerializeField] private float gravity = -20f;

    private CharacterController _controller;
    private Transform _currentTarget;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private Vector3 _velocity;

    private void Start()
    {
        _controller    = GetComponent<CharacterController>();
        _currentTarget = pointA;
    }

    private void Update()
    {
        ApplyGravity();

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
                _isWaiting = false;

            return;
        }

        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        if (_currentTarget == null) return;

        Vector3 targetPos = new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
        Vector3 direction = targetPos - transform.position;
        float   distance  = direction.magnitude;

        if (distance <= stoppingDistance)
        {
            // Hedefe ulaşıldı: bekle ve diğer noktaya geç
            _isWaiting     = true;
            _waitTimer     = waitTime;
            _currentTarget = _currentTarget == pointA ? pointB : pointA;
            return;
        }

        // Hedefe doğru dön
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // İlerle
        _controller.Move(direction.normalized * moveSpeed * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (_controller.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(new Vector3(0f, _velocity.y, 0f) * Time.deltaTime);
    }

    // Editörde noktaları ve yolu görselleştir
    private void OnDrawGizmos()
    {
        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.2f);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.2f);
        }

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
