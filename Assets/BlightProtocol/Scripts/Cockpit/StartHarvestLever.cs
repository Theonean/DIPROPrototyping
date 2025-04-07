using TMPro;
using UnityEngine;

public class StartHarvestLever : ACLever
{
    private Harvester harvester;
    public TextMeshPro harvestButtonFeedback;

    void Start()
    {
        harvester = Harvester.Instance;
        harvester.changedState.AddListener(HarvesterChangedState);
    }

    protected override void OnPulled(float normalizedValue)
    {
        if (normalizedValue >= 0.9f && !isPulled)
        {
            if (harvester.GetZoneState() == ZoneState.MOVING) {
                harvestButtonFeedback.text = "CANNOT HARVEST WHILE MOVING!";
                ResetLever();
            }
            else if (harvester.HasArrivedAtTarget()
            && harvester.IsTargetingResourcePoint()
            && harvester.GetZoneState() is not (ZoneState.HARVESTING or ZoneState.START_HARVESTING or ZoneState.END_HARVESTING)
            && harvester.resourcePointDetector.activeResourcePoints.Count > 0)
            {
                isPulled = true;
                SetPositionNormalized(1f);
                Logger.Log("Starting Harvesting", LogLevel.INFO, LogType.HARVESTER);
                harvester.SetState(new StartHarvestingState(harvester));
            }
            else
            {
                harvestButtonFeedback.text = "CANNOT HARVEST HERE!";
                ResetLever();
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
}
