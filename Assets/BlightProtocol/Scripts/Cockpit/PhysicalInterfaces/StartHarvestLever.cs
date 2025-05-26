using TMPro;
using UnityEngine;

public class StartHarvestLever : ACLever
{
    private Harvester harvester;
    public float resourcePointDetectionRange = 20f;
    public LayerMask resourcePointLayer;
    ResourcePoint closestResourcePoint;


    protected void Awake()
    {
        harvester = Harvester.Instance;
    }

    protected void OnEnable()
    {
        harvester.changedState.AddListener(OnHarvesterChangedState);
    }
    protected void OnDisable()
    {
        harvester.changedState.RemoveListener(OnHarvesterChangedState);
    }

    protected void OnHarvesterChangedState(HarvesterState state)
    {
        if (state == HarvesterState.END_HARVESTING)
        {
            ResetLever();
        }
    }

    protected override void OnPulled(float normalizedValue)
    {
        if (normalizedValue >= 0.9f && !isPulled)
        {
            switch (harvester.GetZoneState())
            {
                case HarvesterState.MOVING:
                case HarvesterState.IDLE:
                    closestResourcePoint = GetClosestResourcePoint();
                    if (closestResourcePoint != null)
                    {
                        isPulled = true;
                        SetPositionNormalized(1f);
                        Logger.Log("Starting Harvesting", LogLevel.INFO, LogType.HARVESTER);
                        harvester.activeResourcePoint = closestResourcePoint;
                        harvester.mover.SetMoveSpeed(0);
                        harvester.SetState(new StartHarvestingState(harvester));
                    }
                    break;
                default:
                    ResetLever();
                    break;
            }
        }
    }

    ResourcePoint GetClosestResourcePoint()
    {
        ResourcePoint closestResourcePoint = null;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, resourcePointDetectionRange, resourcePointLayer);

        if (hitColliders.Length > 0)
        {
            foreach (Collider collider in hitColliders)
            {
                if (collider.gameObject.CompareTag("ResourcePoint"))
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (closestResourcePoint == null || distance < Vector3.Distance(transform.position, closestResourcePoint.transform.position))
                    {
                        closestResourcePoint = collider.GetComponentInParent<ResourcePoint>();
                    }
                }
            }
            return closestResourcePoint;
        }
        else
        {
            return null;
        }
    }
}
