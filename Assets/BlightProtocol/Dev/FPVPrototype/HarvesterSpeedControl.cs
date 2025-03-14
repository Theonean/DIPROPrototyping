using System.Collections;
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
}

public class HarvesterSpeedControl : MonoBehaviour
{
    public static HarvesterSpeedControl Instance { get; private set; }

    [SerializeField] private List<HarvesterSpeedStep> speedSteps;
    public float maxSpeed = 50f;
    public Slider speedIndicator;
    public Image speedIndicatorFill;
    public ResourceData fuelResource;

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
        // Initialize display speed to match the first step
        displaySpeed = speedSteps[currentSpeedStepIndex].speed;
        SetSpeed();
    }

    private void SetSpeed()
    {
        ControlZoneManager.Instance.SetMoveSpeed(speedSteps[currentSpeedStepIndex].speed);
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
            else
            {
                currentSpeedStepIndex = speedSteps.FindIndex(step => step.isBaseSpeed);
                SetSpeed();
            }
        }

        UpdateSpeedIndicator();
    }

    public void OverrideSpeedStep(int index)
    {
        if (index >= 0 && index < speedSteps.Count)
        {
            currentSpeedStepIndex = index;
            UpdateSpeedIndicator();
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
