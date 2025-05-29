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
        SetDestination(Vector3.zero);
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

    public void Reset()
    {
        navMeshAgent.ResetPath();
        isMoving = false;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = moveSpeed;
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
        if (harvester.GetZoneState() == HarvesterState.DIED || harvester.GetZoneState() == HarvesterState.END_HARVESTING) return;

        moveSpeed = newSpeed;
        navMeshAgent.speed = moveSpeed;
        if (moveSpeed > 0.1f)
        {
            if (harvester.GetZoneState() == HarvesterState.HARVESTING)
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

    public void SetMoveSpeedWithoutStateChange(float newSpeed)
    {
        if (harvester.GetZoneState() == HarvesterState.DIED || harvester.GetZoneState() == HarvesterState.END_HARVESTING) return;
        moveSpeed = newSpeed;
        navMeshAgent.speed = moveSpeed;
    }

    void DrawLineToTarget()
    {
        if (lineRenderer == null || !navMeshAgent.hasPath) return;

        NavMeshPath path = navMeshAgent.path;
        lineRenderer.positionCount = path.corners.Length;

        for (int i = 0; i < path.corners.Length; i++)
        {
            lineRenderer.SetPosition(i, path.corners[i]);
        }
    }


    public Vector3 GetMovementDirection()
    {
        return (targetPosition - transform.position).normalized;
    }
}
