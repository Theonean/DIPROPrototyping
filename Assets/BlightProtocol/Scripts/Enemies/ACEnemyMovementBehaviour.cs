using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyMovementType
{
    CUSTOM,
    REMOTECONTROLLED
}

[RequireComponent(typeof(NavMeshAgent))]
public abstract class ACEnemyMovementBehaviour : MonoBehaviour
{
    protected Harvester harvester;
    public NavMeshAgent navMeshAgent { get; protected set; }
    public LayerMask environmentLayer;
    protected bool isMoving = true;

    [Header("Knockback settings")]
    public AnimationCurve knockbackCurve;
    public float knockBackTime = 0.5f;
    public float knockBackStrength = 10f;

    [Header("Speed and Type settings")]
    public float baseSpeed = 4f;
    public float outOfScreenSpeed = 12f;
    public float moveSpeed = 4f;
    private bool isSpeeding = false;
    public EnemyMovementType movementType = EnemyMovementType.CUSTOM;
    public EnemyType type;
    public float outOfScreenThreshold = 125f;
    public float tooFarAwayFromTargetSelfDestruct = 300f;
    

    protected GameObject target;

    void Awake()
    { 
        harvester = Harvester.Instance;
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetSpeed(moveSpeed);

        Vector3 harvesterPosition = harvester.transform.position;
        Vector3 playerPosition = PlayerCore.Instance.transform.position;

        float distanceToHarvester = Vector3.Distance(transform.position, harvesterPosition);
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
        target = distanceToHarvester > distanceToPlayer ? PlayerCore.Instance.gameObject : harvester.gameObject;

        transform.LookAt(target.transform);
    }

    private void Start()
    {
        // Check if a complete path exists to that destination
        Vector3 destination = harvester.transform.position;
        NavMeshPath path = new NavMeshPath();
        bool pathFound = navMeshAgent.CalculatePath(destination, path);
        if (!pathFound || path.status != NavMeshPathStatus.PathComplete)
        {
            Destroy(gameObject);
            return;
        }
    }

        protected void Update()
    {
        Vector3 harvesterPosition = harvester.transform.position;
        Vector3 playerPosition = PlayerCore.Instance.transform.position;

        float distanceToHarvester = Vector3.Distance(transform.position, harvesterPosition); 
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

        target = distanceToHarvester > distanceToPlayer ? PlayerCore.Instance.gameObject : harvester.gameObject;
        
        if(movementType == EnemyMovementType.CUSTOM && 
            (distanceToHarvester > tooFarAwayFromTargetSelfDestruct && distanceToPlayer > tooFarAwayFromTargetSelfDestruct))
        {
            Destroy(gameObject);
            return;
        }

        if (!isSpeeding && movementType == EnemyMovementType.CUSTOM) {
            if (Vector3.Distance(transform.position, harvesterPosition) > outOfScreenThreshold && Vector3.Distance(transform.position, playerPosition) > outOfScreenThreshold)
            {
                moveSpeed = outOfScreenSpeed;
                isSpeeding = true;
                SetSpeed(moveSpeed);
            }
        }
        else {
            if (Vector3.Distance(transform.position, harvesterPosition) < outOfScreenThreshold && PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.DRONE)
            {
                moveSpeed = baseSpeed;
                isSpeeding = false;
                SetSpeed(moveSpeed);
            }
        }
        
        SetSpeed(moveSpeed);
        switch (movementType)
        {
            case EnemyMovementType.REMOTECONTROLLED:
                break; //don't have any movement behaviour when remote controlled
            case EnemyMovementType.CUSTOM:
                CustomMovementUpdate();
                break;
        }
    }

    protected abstract void CustomMovementUpdate();

    public bool CanMove()
    {
        return isMoving && harvester != null && harvester.GetZoneState() != HarvesterState.DIED;
    }

    public void SetDestination(Vector3 targetPosition)
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(targetPosition);
        }
    }

    public void StopMovement()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = true;
            isMoving = false;
        }
    }

    public void ResumeMovement()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = false;
            isMoving = true;
        }
    }

    public void SetMovementType(EnemyMovementType movementType)
    {
        this.movementType = movementType;
    }

    protected void SetSpeed(float speed)
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.speed = speed;
        }
    }


    public IEnumerator ApplyKnockback(Vector3 direction)
    {
        StopMovement();
        float timer = 0f;
        while (timer < knockBackTime)
        {
            transform.position += direction * knockBackStrength * Time.deltaTime * knockbackCurve.Evaluate(timer / knockBackTime);
            timer += Time.deltaTime;
            yield return null;
        }
        ResumeMovement();
    }
}
