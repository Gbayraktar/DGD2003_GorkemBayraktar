using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("NavMesh Devriye")]
    [SerializeField] private float patrolRadius = 6f;
    [SerializeField] private float waitAtPoint = 1.2f;
    [SerializeField] private float arriveDistance = 0.35f;

    private NavMeshAgent _agent;
    private Vector3 _centerPoint;
    private float _waitTimer;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _centerPoint = transform.position;

        _agent.stoppingDistance = arriveDistance;
        PickNextPatrolPoint();
    }

    private void Update()
    {
        if (_agent.pathPending) return;

        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= waitAtPoint)
            {
                _waitTimer = 0f;
                PickNextPatrolPoint();
            }
        }
    }

    private void PickNextPatrolPoint()
    {
        for (int i = 0; i < 15; i++)
        {
            Vector2 rnd2 = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = _centerPoint + new Vector3(rnd2.x, 0f, rnd2.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
                return;
            }
        }

        // Yakın çevrede nokta bulunamazsa merkezde bekle.
        _agent.SetDestination(_centerPoint);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? _centerPoint : transform.position, patrolRadius);
    }
}
