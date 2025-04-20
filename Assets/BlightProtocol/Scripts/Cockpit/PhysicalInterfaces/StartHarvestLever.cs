using TMPro;
using UnityEngine;

public class StartHarvestLever : ACLever
{
    private Harvester harvester;
    public TextMeshPro harvestButtonFeedback;
    public float resourcePointDetectionRange = 20f;
    public LayerMask resourcePointLayer;
    ResourcePoint closestResourcePoint;


    void Start()
    {
        harvester = Harvester.Instance;
        harvester.changedState.AddListener(HarvesterChangedState);
    }

    void OnEnable()
    {
        if (harvester != null)
        {
            harvester.changedState.RemoveListener(HarvesterChangedState);
            harvester.changedState.AddListener(HarvesterChangedState);
        }

    }

    void OnDisable()
    {
        harvester.changedState.RemoveListener(HarvesterChangedState);
    }

    protected override void OnPulled(float normalizedValue)
    {
        if (normalizedValue >= 0.9f && !isPulled)
        {
            switch (harvester.GetZoneState())
            {
                case ZoneState.IDLE:
                    closestResourcePoint = GetClosestResourcePoint();
                    if (closestResourcePoint != null)
                    {
                        isPulled = true;
                        SetPositionNormalized(1f);
                        Logger.Log("Starting Harvesting", LogLevel.INFO, LogType.HARVESTER);
                        harvester.activeResourcePoint = closestResourcePoint;
                        harvester.SetState(new StartHarvestingState(harvester));
                    }
                    else
                    {
                        harvestButtonFeedback.text = "CANNOT HARVEST HERE!";
                        ResetLever();
                    }
                    break;

                case ZoneState.MOVING:
                    harvestButtonFeedback.text = "CANNOT HARVEST WHILE MOVING!";
                    ResetLever();
                    break;

                default:
                    harvestButtonFeedback.text = "CANNOT HARVEST HERE!";
                    ResetLever();
                    break;
            }
        }
    }

    private void HarvesterChangedState(ZoneState zoneState)
    {
        switch (zoneState)
        {
            case ZoneState.HARVESTING:
                harvestButtonFeedback.text = "Harvesting...";
                break;
            case ZoneState.START_HARVESTING:
                harvestButtonFeedback.text = "Starting Harvest...";
                break;
            case ZoneState.END_HARVESTING:
                harvestButtonFeedback.text = "Ending Harvest...";
                ResetLever();
                break;
            default:
                harvestButtonFeedback.text = "";
                break;
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
