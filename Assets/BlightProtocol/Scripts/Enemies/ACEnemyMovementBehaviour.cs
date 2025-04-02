using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class ACEnemyMovementBehaviour : MonoBehaviour
{
    protected Harvester harvester;
    protected NavMeshAgent navMeshAgent;
    protected bool isMoving = true;
    public AnimationCurve knockbackCurve;
    public float moveSpeed = 4f;

    protected abstract void Update(); 

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

    protected void SetDestination(Vector3 targetPosition)
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.SetDestination(targetPosition);
        }
    }

    protected void StopMovement()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = true;
            isMoving = false;
        }
    }

    protected void ResumeMovement()
    {
        if (navMeshAgent != null && navMeshAgent.isActiveAndEnabled)
        {
            navMeshAgent.isStopped = false;
            isMoving = true;
        }
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
