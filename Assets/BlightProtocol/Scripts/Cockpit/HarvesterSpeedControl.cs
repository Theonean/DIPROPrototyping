using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class HarvesterSpeedStep
{
    public float speed;
    public float fuelCost;
    public Color displayColor;
    public bool isBaseSpeed = false;
    public float seismoEmission = 0f;
}

public class HarvesterSpeedControl : MonoBehaviour
{
    public static HarvesterSpeedControl Instance { get; private set; }

    [SerializeField] private List<HarvesterSpeedStep> speedSteps;
    public float maxSpeed = 50f;
    public Slider speedIndicator;
    public Image speedIndicatorFill;
    private ResourceData fuelResource;

    private int currentSpeedStepIndex = 0;
    private float displaySpeed = 0f; // Smoothed speed for UI

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
    }

    void Start()
    {
        fuelResource = ResourceHandler.Instance.fuelResource;
        
        // Initialize display speed to match the first step
        displaySpeed = speedSteps[currentSpeedStepIndex].speed;
        SetSpeed();

        Harvester.Instance.changedState.AddListener(OnHarvesterStateChanged);
    }

    private void SetSpeed()
    {
        Harvester.Instance.mover.SetMoveSpeed(speedSteps[currentSpeedStepIndex].speed);
        Seismograph.Instance.SetOtherEmission("Overspeed", speedSteps[currentSpeedStepIndex].seismoEmission);
    }

    private void UpdateSpeedIndicator()
    {
        displaySpeed = Mathf.Lerp(displaySpeed, speedSteps[currentSpeedStepIndex].speed, Time.deltaTime * 5f);

        speedIndicator.value = displaySpeed / maxSpeed;
        speedIndicatorFill.color = speedSteps[currentSpeedStepIndex].displayColor;
    }

    void Update()
    {
        // Adjust fuel based on current step
        if (speedSteps.Count > 0)
        {
            HarvesterSpeedStep currentStep = speedSteps[currentSpeedStepIndex];

            if (ResourceHandler.Instance.CheckResource(fuelResource) >= currentStep.fuelCost * Time.deltaTime)
            {
                ResourceHandler.Instance.ConsumeResource(fuelResource, currentStep.fuelCost * Time.deltaTime, true);
            }
            else if(currentSpeedStepIndex > 0)
            {
                currentSpeedStepIndex = speedSteps.FindIndex(step => step.isBaseSpeed);
                SetSpeed();
            }
        }

        UpdateSpeedIndicator();
    }

    private void OnHarvesterStateChanged(ZoneState state) {
        switch(state) {
            case ZoneState.IDLE:
            case ZoneState.HARVESTING:
            case ZoneState.START_HARVESTING:
            case ZoneState.END_HARVESTING:
            case ZoneState.DIED:
                OverrideSpeedStep(0);
            break;
        }
    }
    public void OverrideSpeedStep(int index)
    {
        if (index >= 0 && index < speedSteps.Count)
        {
            currentSpeedStepIndex = index;
            UpdateSpeedIndicator();
            Seismograph.Instance.SetOtherEmission("Overspeed", speedSteps[currentSpeedStepIndex].seismoEmission);
        }
    }

    public void AdjustSpeed(bool increase)
    {
        if (increase)
        {
            if (currentSpeedStepIndex < speedSteps.Count - 1)
                currentSpeedStepIndex++;
        }
        else
        {
            if (currentSpeedStepIndex > 0)
                currentSpeedStepIndex--;
        }

        SetSpeed();
    }
}
