using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrolling,
    Following
}

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;
    [SerializeField] private float followStopDistance = 3f;

    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private float losePlayerTime = 3f;

    private NavMeshAgent agent;
    private int currentPatrolIndex;
    private bool isWaiting;
    private EnemyState enemyState = EnemyState.Patrolling;
    private float timeSinceLostPlayer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GoToNextPatrolPoint();
    }

    private void Update()
    {
        Vector3 leftAngle = Quaternion.Euler(0, -fieldOfView/2, 0) * transform.forward;
        Vector3 rightAngle = Quaternion.Euler(0, fieldOfView/2, 0) * transform.forward;

        Debug.DrawRay(transform.position, leftAngle * detectionRange, Color.yellow);
        Debug.DrawRay(transform.position, rightAngle * detectionRange, Color.yellow);

        var distanceToPlayer = Vector3.Distance(player.position, transform.position);

        switch (enemyState)
        {
            case EnemyState.Patrolling:
                Patrol();
                if(distanceToPlayer <= detectionRange && CanSeePlayer())
                {
                    enemyState = EnemyState.Following;
                }
                break;

            case EnemyState.Following:
                Follow();
                if (!CanSeePlayer())
                {
                    timeSinceLostPlayer += Time.deltaTime;
                    if(timeSinceLostPlayer >= losePlayerTime)
                    {
                        enemyState = EnemyState.Patrolling;
                        GoToClosestPatrolPoint();
                    }
                }
                else
                {
                    timeSinceLostPlayer = 0f;
                }

                break;
        }
    }

    private void Patrol()
    {
        if (isWaiting) return;
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartCoroutine(WaitAtPatrolPoint());
        }
    }

    private void Follow()
    {
        agent.stoppingDistance = followStopDistance;
        if (Vector3.Distance(transform.position, player.position) > followStopDistance)
        {
            agent.SetDestination(player.position);
        }
    }

    private IEnumerator WaitAtPatrolPoint()
    {
        isWaiting = true;
        agent.isStopped = true;

        yield return new WaitForSeconds(patrolWaitTime);

        agent.isStopped = false;
        GoToNextPatrolPoint();
        isWaiting = false;
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.stoppingDistance = stopAtDistance;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void GoToClosestPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.stoppingDistance = stopAtDistance;

        var closestIndex = 0;
        var closestDistance = float.MaxValue;

        for(var i = 0; i < patrolPoints.Length; i++)
        {
            var distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    private bool CanSeePlayer()
    {
        return IsFacingPlayer() && HasClearPathToPlayer();
    }

    private bool IsFacingPlayer()
    {
        var dirToPlayer = (player.position - transform.position).normalized;
        var angle = Vector3.Angle(transform.forward, dirToPlayer);
        return angle <= fieldOfView / 2f;
    }

    private bool HasClearPathToPlayer()
    {
        var dirToPlayer = player.position - transform.position;
        if(Physics.Raycast(transform.position, dirToPlayer.normalized, out RaycastHit hit, dirToPlayer.magnitude))
        {
            return hit.transform == player;
        }

        return true;
    }
}
