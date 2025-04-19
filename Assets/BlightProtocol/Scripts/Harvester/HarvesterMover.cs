using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class HarvesterMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float maxAngularRotationSpeed;
    Vector3 targetPosition;

    public TargetPosition targetPosObject;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private NavMeshAgent navMeshAgent;
    private Harvester harvester;
    private bool isMoving = false;

    private void Awake()
    {
        harvester = Harvester.Instance;
        navMeshAgent.speed = moveSpeed;
    }

    void Update()
    {
        if (isMoving)
        {
            navMeshAgent.angularSpeed = Mathf.Lerp(0, maxAngularRotationSpeed, navMeshAgent.velocity.magnitude / moveSpeed);
            DrawLineToTarget();
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                navMeshAgent.isStopped = true;
                harvester.SetState(new IdleState(harvester));
            }
        }
    }

    public void StartMoving()
    {
        isMoving = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = moveSpeed;
    }

    public void SetDestination(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        navMeshAgent.SetDestination(targetPosition);

        targetPosObject.transform.position = targetPosition;
    }


    public void SetMoveSpeed(float newSpeed)
    {
        if (harvester.GetZoneState() == ZoneState.DIED || harvester.GetZoneState() == ZoneState.END_HARVESTING) return;

        moveSpeed = newSpeed;
        navMeshAgent.speed = moveSpeed;
        if (moveSpeed > 0.1f)
        {
            if (harvester.GetZoneState() == ZoneState.HARVESTING)
            {
                harvester.SetState(new EndHarvestingState(harvester));
            }
            else
            {
                harvester.SetState(new MovingState(harvester));
            }
        }
        else
        {
            harvester.SetState(new IdleState(harvester));
            isMoving = false;
        }
    }

    void DrawLineToTarget()
    {
        if (targetPosition != Vector3.zero && lineRenderer != null)
        {
            lineRenderer.SetPosition(0, targetPosition);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    public Vector3 GetMovementDirection()
    {
        return (targetPosition - transform.position).normalized;
    }
}
