using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("Devriye Noktaları")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Ayarlar")]
    [SerializeField] private float waitAtPoint = 1f;
    [SerializeField] private float arriveDistance = 0.3f;

    private NavMeshAgent _agent;
    private Transform _target;
    private float _waitTimer;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.stoppingDistance = arriveDistance;

        TryAutoFindPoints();

        if (pointA == null || pointB == null)
        {
            Debug.LogWarning("[PatrolEnemy] Point A ve Point B atanmamış! Boş obje oluşturup sürükle.", this);
            return;
        }

        _target = pointA;
        GoTo(_target);
    }

    private void Update()
    {
        if (_target == null || _agent.pathPending) return;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitAtPoint)
            {
                _waitTimer = 0f;
                _target = _target == pointA ? pointB : pointA;
                GoTo(_target);
            }
        }
    }

    private void GoTo(Transform target)
    {
        if (target == null) return;

        if (NavMesh.SamplePosition(target.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
        else
            _agent.SetDestination(target.position);
    }

    private void TryAutoFindPoints()
    {
        if (pointA == null)
        {
            GameObject a = GameObject.Find("PointA");
            if (a != null) pointA = a.transform;
        }

        if (pointB == null)
        {
            GameObject b = GameObject.Find("PointB");
            if (b != null) pointB = b.transform;
        }
    }

    private void OnDrawGizmos()
    {
        if (pointA != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.25f);
        }

        if (pointB != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pointB.position, 0.25f);
        }

        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}
