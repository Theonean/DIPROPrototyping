using System.Collections;
using TMPro;
using UnityEngine;

public class StartHarvestLever : MonoBehaviour, IFPVInteractable
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
    public Transform leverMax;
    public Transform leverMin;
    private float leverProgress;
    public AnimationCurve adjustCurve;
    private Vector2[] screenSpaceBounds;

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
        screenSpaceBounds = this.GetScreenSpaceBounds(leverMin.position, leverMax.position, FPVPlayerCam.Instance.GetComponent<Camera>());
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
        leverProgress = this.GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        float angle = GetLeverAngle(leverProgress);

        leverPivot.localRotation = Quaternion.AngleAxis(angle, Vector3.right);

        if (leverProgress >= 0.9f && !isPulled)
        {
            StartHarvest();
            StartCoroutine(LerpLeverPosition(1, 0.2f));
        }
    }

    private float GetLeverAngle(float progress)
    {
        return Mathf.Acos((1 - progress) * 2 - 1) * Mathf.Rad2Deg;
    }

    private void ResetLever()
    {
        StartCoroutine(LerpLeverPosition(0, 0.5f));
        isPulled = false;
    }

    private IEnumerator LerpLeverPosition(float targetProgress, float duration)
    {
        IsCurrentlyInteractable = false;
        float elapsedTime = 0f;
        float startProgress = leverProgress;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = adjustCurve.Evaluate(t);

            float newProgress = startProgress + curveValue * (targetProgress - startProgress);
            float angle = GetLeverAngle(newProgress);
            leverPivot.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
            leverProgress = newProgress;


            elapsedTime += Time.deltaTime;
            yield return null;
        }
        leverPivot.localRotation = Quaternion.AngleAxis(GetLeverAngle(targetProgress), Vector3.right);
        leverProgress = targetProgress;
        IsCurrentlyInteractable = true;
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
