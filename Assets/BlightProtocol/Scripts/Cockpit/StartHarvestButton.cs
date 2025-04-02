using TMPro;
using UnityEngine;

public class StartHarvestButton : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E";
    public string interactText = "";
    public bool UpdateHover { get; set; } = false;
    public bool UpdateInteract { get; set; } = true;
    [SerializeField] private Transform _touchPoint;
    public Transform leverPivot;
    public float leverMinAngle = 0f;
    public float leverMaxAngle = 180f;
    private float rotDelta = 0f;
    private bool isPulled = false;

    public Transform TouchPoint
    {
        get => _touchPoint;
        set => _touchPoint = value;
    }

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public string InteractText
    {
        get => interactText;
        set => interactText = value;
    }
    private Harvester harvester;
    public TextMeshPro harvestButtonFeedback;

    void Start()
    {
        harvester = Harvester.Instance;
        harvester.changedState.AddListener(HarvesterChangedState);
    }

    public void OnStartInteract()
    {
        Cursor.visible = false;
        rotDelta = 0f;
    }
    public void OnUpdateInteract() { if (!isPulled) PullLever(); }
    public void OnEndInteract()
    {
        Cursor.visible = true;
        if (!isPulled)
        {
            ResetLever();
        }
    }

    private void PullLever()
    {

        float mouseY = -Input.GetAxis("Mouse Y") * 15f;

        leverPivot.Rotate(mouseY, 0f, 0f);
        rotDelta += mouseY;

        float currentAngle = leverPivot.localRotation.eulerAngles.x;

        if (currentAngle > 180f)
        {
            currentAngle -= 360f;
        }

        float clampedAngle = Mathf.Clamp(currentAngle, leverMinAngle, leverMaxAngle);

        leverPivot.localRotation = Quaternion.Euler(clampedAngle, leverPivot.localRotation.eulerAngles.y, leverPivot.localRotation.eulerAngles.z);

        if (rotDelta >= leverMaxAngle - 10f && !isPulled)
        {
            StartHarvest();
        }
    }


    private void ResetLever()
    {
        leverPivot.localRotation = Quaternion.Euler(leverMinAngle, 0, 0);
        isPulled = false;
    }

    private void StartHarvest()
    {
        if (harvester.HasArrivedAtTarget()
        && harvester.IsTargetingResourcePoint())
        {
            if (harvester.GetZoneState() != ZoneState.HARVESTING
            && harvester.GetZoneState() != ZoneState.START_HARVESTING
            && harvester.GetZoneState() != ZoneState.END_HARVESTING)
            {
                isPulled = true;
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

    public void OnHover()
    {
        this.DefaultOnHover();
    }
}
