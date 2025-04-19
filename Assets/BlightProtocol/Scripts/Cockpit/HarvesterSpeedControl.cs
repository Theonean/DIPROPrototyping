using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

    [SerializeField] public List<HarvesterSpeedStep> speedSteps;
    public float maxSpeed = 50f;
    private ResourceData fuelResource;

    private int currentSpeedStepIndex = 0;
    private float displaySpeed = 0f; // Smoothed speed for UI
    private SpeedSlider speedSlider;
    public UnityEvent inputDenied;

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
        speedSlider = GetComponent<SpeedSlider>();

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

    void Update()
    {
        // reset speed to base speed if fuel is empty
        if (speedSteps.Count > 0)
        {
            HarvesterSpeedStep currentStep = speedSteps[currentSpeedStepIndex];
            
            if (currentStep.fuelCost > 0) {
                return;
            }

            if (ResourceHandler.Instance.GetAmount(fuelResource) >= currentStep.fuelCost * Time.deltaTime)
            {
                ResourceHandler.Instance.Consume(fuelResource, currentStep.fuelCost * Time.deltaTime);
            }
            else if (currentSpeedStepIndex > 0)
            {
                currentSpeedStepIndex = speedSteps.FindIndex(step => step.isBaseSpeed);
                SetSpeed();
                OverrideSpeedStep(currentSpeedStepIndex);
                inputDenied.Invoke();
            }
        }
    }

    private void OnHarvesterStateChanged(ZoneState state)
    {
        switch (state)
        {
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
        // Set slider in case of harvester state change
        if (index >= 0 && index < speedSteps.Count)
        {
            currentSpeedStepIndex = index;
            speedSlider.SetPositionByIndex(index);
            Seismograph.Instance.SetOtherEmission("Overspeed", speedSteps[currentSpeedStepIndex].seismoEmission);
        }
    }

    public void SetSpeedStepIndex(int index)
    {
        if (index >= 0 && index < speedSteps.Count && index != currentSpeedStepIndex)
        {
            if (Harvester.Instance.HasArrivedAtTarget()) {
                OverrideSpeedStep(0);
                inputDenied.Invoke();
            }
            currentSpeedStepIndex = index;
            SetSpeed();
        }
    }

    public int GetSpeedStepCount()
    {
        return speedSteps.Count;
    }
}
