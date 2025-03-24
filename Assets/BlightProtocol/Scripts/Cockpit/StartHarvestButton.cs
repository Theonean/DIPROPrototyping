using TMPro;
using UnityEngine;

public class StartHarvestButton : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E"; // Backing field for Inspector
    private Harvester harvester;
    public TextMeshPro harvestButtonFeedback;

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    void Start()
    {
        harvester = Harvester.Instance;
        harvester.changedState.AddListener(HarvesterChangedState);
    }

    public void OnInteract()
    {
        if (harvester.HasArrivedAtTarget()
        && harvester.IsTargetingResourcePoint())
        {
            if (harvester.GetZoneState() != ZoneState.HARVESTING
            && harvester.GetZoneState() != ZoneState.START_HARVESTING
            && harvester.GetZoneState() != ZoneState.END_HARVESTING)
            {
                Logger.Log("Starting Harvesting", LogLevel.INFO, LogType.HARVESTER);
                harvester.SetState(new StartHarvestingState(harvester));
            }

        }
        else
        {
            harvestButtonFeedback.text = "CANNOT HARVEST HERE!";
        }
    }

    private void HarvesterChangedState(ZoneState zoneState)
    {
        switch(zoneState) {
            case ZoneState.HARVESTING:
            harvestButtonFeedback.text = "Harvesting...";
            break;

            case ZoneState.START_HARVESTING:
            harvestButtonFeedback.text = "Starting Harvest...";
            break;

            case ZoneState.END_HARVESTING:
            harvestButtonFeedback.text = "Ending Harvest...";
            break;

            default:
            harvestButtonFeedback.text = "";
            break;

        }
    }

    public void OnHover()
    {
        this.DefaultOnHover();
    }
}
