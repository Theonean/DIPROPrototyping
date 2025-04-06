using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HarvesterMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    Vector3 targetPosition;

    public TargetPosition targetPosObject;
    public float travelTimeLeft;
    [SerializeField] private LineRenderer lineRenderer;
    private Harvester harvester;
    private Coroutine moveCoroutine;

    private void Awake()
    {
        harvester = Harvester.Instance;
    }

    public void StartMoving()
    {
        if(moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveHarvesterToDestination());
    }

    private IEnumerator MoveHarvesterToDestination()
    {
        Debug.Log(Vector3.Distance(transform.position, targetPosition));
        while (Vector3.Distance(transform.position, targetPosition) > 0.5f)
        {
            travelTimeLeft -= Time.deltaTime;
            if (travelTimeLeft <= 0)
                Debug.Log("Harvester should have arrived?");


            DrawLineToTarget();

            // Calculate the sinus wave offset
            float sinOffset = Mathf.Sin(Time.time * 2f) * 0.5f;
            Vector3 offsetPosition = targetPosition + transform.right * sinOffset;

            // Move towards the target position with sinus wave
            Vector3 newPosition = Vector3.MoveTowards(transform.position, offsetPosition, moveSpeed * Time.deltaTime);

            // Calculate the movement direction
            Vector3 moveDirection = (newPosition - transform.position).normalized;

            // Update position
            transform.position = newPosition;

            // Rotate to face the movement direction
            if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }

            yield return null;
        }
    }

    public void SetDestination(Vector3 targetPosition)
    { 
        this.targetPosition = targetPosition;

        targetPosObject.transform.position = targetPosition;
        travelTimeLeft = Vector3.Distance(transform.position, targetPosition) / moveSpeed;
    }


    public void SetMoveSpeed(float newSpeed)
    {
        if (harvester.GetZoneState() == ZoneState.DIED || harvester.GetZoneState() == ZoneState.END_HARVESTING) return;

        moveSpeed = newSpeed;
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
        return(targetPosition - transform.position).normalized;
    }
}
