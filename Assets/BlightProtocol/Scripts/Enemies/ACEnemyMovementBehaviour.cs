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
    protected bool isMoving = true;
    public AnimationCurve knockbackCurve;
    public float moveSpeed = 4f;
    public EnemyMovementType movementType = EnemyMovementType.CUSTOM;

    protected void Update()
    {
        switch(movementType)
        {
            case EnemyMovementType.REMOTECONTROLLED:
                break; //don't have any movement behaviour when remote controlled
                case EnemyMovementType.CUSTOM:
                CustomMovementUpdate();
                break;
        }
    }

    protected abstract void CustomMovementUpdate();

    //NOTE: navMeshAgent.SetDestination forces a path recalculation, maybe in future add only do it every x frames

    void Awake()
    {
        harvester = Harvester.Instance;
        navMeshAgent = GetComponent<NavMeshAgent>();
        SetSpeed(moveSpeed);
    }
    public bool CanMove()
    {
        return isMoving && harvester != null && harvester.GetZoneState() != ZoneState.DIED;
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


    public IEnumerator ApplyKnockback(Vector3 direction, float knockback)
    {
        StopMovement();
        float timer = 0f;
        float knockbackTime = 0.5f;
        while (timer < knockbackTime)
        {
            transform.position += direction * knockback * Time.deltaTime * knockbackCurve.Evaluate(timer / knockbackTime);
            timer += Time.deltaTime;
            yield return null;
        }
        ResumeMovement();
    }
}
