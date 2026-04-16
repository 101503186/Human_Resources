using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrolling,
    Attacking,
    Observing,
}

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Patrolling Settings")]
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float stopAtDistance = 0.5f;
    [SerializeField] private float followStopDistance = 3f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRange;
    [SerializeField] private float fieldOfView = 90f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float losePlayerTime = 3f;
    [SerializeField] private float spotPlayerTime = 5f;
    private float detectionSpeed = 1f;
    
    private float timeWaited;
    private int currentPatrolIndex;
    private bool isWaiting;
    private EnemyState enemyState = EnemyState.Patrolling;
    private float timeSinceLostPlayer;
    private float timeSeeingPlayer;

    public MonoBehaviour scriptToDisable;
    public Rigidbody rb;
    public Light detectionLight;
    private NavMeshAgent agent;
    private bool isAlive = true;
    private float healthPoints = 2f;

    public Transform barrelOpening;
    private EnemyGunScript enemyGun;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyGun = GetComponentInChildren<EnemyGunScript>();
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

        if (isAlive)
        {
            switch (enemyState)
            {
                case EnemyState.Patrolling:
                    Patrol();
                    if (distanceToPlayer <= detectionRange && CanSeePlayer() && PlayerHealth.playerHealthScript.playerIsAlive)
                    {
                        enemyState = EnemyState.Observing;
                    }
                    break;

                case EnemyState.Attacking:
                    Follow();
                    if (!CanSeePlayer() || !PlayerHealth.playerHealthScript.playerIsAlive)
                    {
                        timeSeeingPlayer = 0;
                        timeSinceLostPlayer += Time.deltaTime;
                        if (timeSinceLostPlayer >= losePlayerTime)
                        {
                            enemyState = EnemyState.Patrolling;
                            GoToClosestPatrolPoint();
                        }
                    }
                    else
                    {
                        timeSinceLostPlayer = 0f;
                        enemyGun.Shoot(barrelOpening);
                    }
                    break;

                case EnemyState.Observing:
                    timeSeeingPlayer += Time.deltaTime * DetectionSpeedMofidier();
                    if (timeSeeingPlayer >= spotPlayerTime)
                    {
                        enemyState = EnemyState.Attacking;
                    }
                    break;
            }
        }
    }

    private void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            WaitAtPatrolPoint();
        }
    }

    private void Follow()
    {
        isWaiting = false;
        agent.isStopped = false;
        agent.stoppingDistance = followStopDistance;

        float singleStep = rotationSpeed * Time.deltaTime;
        Vector3 playerDir = player.position - transform.position;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, playerDir, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDir);

        if (Vector3.Distance(transform.position, player.position) > followStopDistance)
        {
            agent.SetDestination(player.position);
        }
    }

    private void WaitAtPatrolPoint()
    {
        if (!isWaiting)
        {
            isWaiting = true;
            agent.isStopped = true;
        }

        timeWaited += Time.deltaTime;
                                        
        if (timeWaited >= patrolWaitTime)
        {
            agent.isStopped = false;
            GoToNextPatrolPoint();
            isWaiting = false;
            timeWaited = 0f;
        }
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

    private float DetectionSpeedMofidier()
    {
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);

        if (distanceToPlayer < detectionRange)
        {
            return detectionSpeed * (detectionRange / distanceToPlayer);
        }

        return detectionSpeed;
    }

    public void TakeDamage(float damage)
    {
        if (!isAlive) return;

        healthPoints -= damage;

        if(healthPoints <= 0f)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log("Died: " + gameObject.name);
        Debug.Log("Root tag: " + transform.root.tag);

        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child.CompareTag("VIP"))
            {
                GameManager.Instance.vipIsDead = true;
                Debug.Log("VIP Killed");
                break;
            }
        }

        if (scriptToDisable != null) scriptToDisable.enabled = false;
        
        if (agent != null) agent.enabled = false;
        
        if (rb != null) rb.constraints = RigidbodyConstraints.None;
        
        if(detectionLight != null) detectionLight.enabled = false;
    }
}
