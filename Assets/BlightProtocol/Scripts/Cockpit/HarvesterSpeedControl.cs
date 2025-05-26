using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class HarvesterSpeedStep
{
    public float speed;
    public Color textColor;
    public Color activeBackgroundColor;
    public bool isBaseSpeed = false;
    public int seismoEmission = 0;
    public int crystalCost;
}

public class HarvesterSpeedControl : MonoBehaviour
{
    public static HarvesterSpeedControl Instance { get; private set; }
    [SerializeField] public List<HarvesterSpeedStep> speedSteps;
    private int currentSpeedStepIndex = 0;
    private int baseSpeedIndex;
    [SerializeField] private float crystalConsumptionInterval = 1f;
    private float timeSinceLastConsumption = 0f;


    [SerializeField] private SpeedSlider speedSlider;
    [SerializeField] private SpeedScreen speedDisplayer;
    private float[] stepPositions, boundaries;


    public UnityEvent overrodePosition;


    // Singleton References
    private ItemManager itemManager;
    private Harvester harvester;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        InitializeStepPositions();
    }

    private void InitializeStepPositions()
    {
        stepPositions = new float[speedSteps.Count];
        boundaries = new float[speedSteps.Count];

        float stepHeightNormalized = 1f / speedSteps.Count;

        for (int i = 0; i < speedSteps.Count; i++)
        {
            stepPositions[i] = stepHeightNormalized * (i + 1) - stepHeightNormalized / 2;
            boundaries[i] = stepHeightNormalized * (i + 1) - 0.05f;
        }
        speedSlider.boundary = boundaries[^1];
    }

    void Start()
    {
        itemManager = ItemManager.Instance;

        harvester = Harvester.Instance;

        baseSpeedIndex = speedSteps.FindIndex(step => step.isBaseSpeed);
        SetSpeed(0);
        OverrideSpeedSliderPos(0);
    }

    void OnEnable()
    {
        Harvester.Instance.changedState.AddListener(OnHarvesterStateChanged);
        speedSlider.valueChanged.AddListener(OnSliderValueChanged);
        speedSlider.startDrag.AddListener(OnSliderStartDrag);
        speedSlider.endDrag.AddListener(OnSliderEndDrag);
    }

    void OnDisable()
    {
        Harvester.Instance.changedState.RemoveListener(OnHarvesterStateChanged);
        speedSlider.valueChanged.RemoveListener(OnSliderValueChanged);
        speedSlider.startDrag.RemoveListener(OnSliderStartDrag);
        speedSlider.endDrag.RemoveListener(OnSliderEndDrag);
    }

    private void OnSliderStartDrag()
    {
        UpdateSliderBoundary();
    }

    private void OnSliderEndDrag(float value)
    {
        int index = GetSliderIndex(value);
        OverrideSpeedSliderPos(index, false);
    }

    private void OnSliderValueChanged(float value)
    {
        int index = GetSliderIndex(value);


        if (index != currentSpeedStepIndex)
        {
            SetSpeed(index);
        }

        currentSpeedStepIndex = index;
        UpdateSliderBoundary();
    }

    private int GetSliderIndex(float value)
    {
        int index = Mathf.FloorToInt(value * speedSteps.Count);
        return Mathf.Clamp(index, 0, speedSteps.Count - 1);
    }

    public void SetSpeed(int index)
    {
        TutorialManager tM = TutorialManager.Instance;
        if (tM.IsTutorialOngoing() && tM.progressState is not TutorialProgress.SETSPEED and not TutorialProgress.SETSPEEDRESOURCEPOINT and not TutorialProgress.DRIVETOCHECKPOINT) return;

        if (index >= 0 && index < speedSteps.Count)
        {

            Harvester.Instance.mover.SetMoveSpeed(speedSteps[index].speed);

            Seismograph.Instance.SetOtherEmission(
                "Overspeed",
                speedSteps[index].seismoEmission
            );

            speedDisplayer.SetActiveStep(index);
        }
        else
        {
            Logger.Log("speed step index out of bounds", LogLevel.ERROR, LogType.COCKPIT);
        }

    }

    void Update()
    {
        // reset speed to base speed if crystals are empty
        HarvesterSpeedStep currentStep = speedSteps[currentSpeedStepIndex];

        if (currentSpeedStepIndex > 0)
        {
            if (harvester.HasArrivedAtTarget())
            {
                SetSpeed(0);
                OverrideSpeedSliderPos(0);
                currentSpeedStepIndex = 0;
                return;
            }

            if (currentStep.crystalCost <= 0)
            {
                return;
            }

            if (timeSinceLastConsumption >= crystalConsumptionInterval)
            {
                if (itemManager.RemoveCrystal(Mathf.FloorToInt(currentStep.crystalCost)))
                {
                    timeSinceLastConsumption = 0f;
                }
                else
                {
                    currentSpeedStepIndex = baseSpeedIndex;
                    SetSpeed(baseSpeedIndex);
                    OverrideSpeedSliderPos(currentSpeedStepIndex);
                }
            }
            else
            {
                timeSinceLastConsumption += Time.deltaTime;
            }
        }

    }

    private void OnHarvesterStateChanged(HarvesterState state)
    {
        switch (state)
        {
            case HarvesterState.HARVESTING:
            case HarvesterState.START_HARVESTING:
            case HarvesterState.END_HARVESTING:
            case HarvesterState.DIED:
                SetSpeed(0);
                OverrideSpeedSliderPos(0);
                break;
        }
    }

    // Only use if speed is not set by slider
    public void OverrideSpeedSliderPos(int index, bool showFeedback = true)
    {
        float normalizedPos = stepPositions[index];
        speedSlider.SetPositionNormalized(normalizedPos);
        if (showFeedback) overrodePosition.Invoke();
    }

    public int GetSpeedStepCount()
    {
        return speedSteps.Count;
    }

    private void UpdateSliderBoundary()
    {
        int index;
        if (harvester.HasArrivedAtTarget())
        {
            index = 0;
        }
        else if (currentSpeedStepIndex + 1 < speedSteps.Count)
        {
            if (itemManager.GetCrystal() < speedSteps[currentSpeedStepIndex + 1].crystalCost)
            {
                index = currentSpeedStepIndex;
            }
            else
            {
                index = speedSteps.Count - 1;
            }
        }
        else
        {
            index = speedSteps.Count - 1;
        }

        speedSlider.boundary = boundaries[index];
    }
}
