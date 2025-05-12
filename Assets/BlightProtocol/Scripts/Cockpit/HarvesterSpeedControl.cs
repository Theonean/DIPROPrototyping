using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class HarvesterSpeedStep
{
    public float speed;
    public Color displayColor;
    public bool isBaseSpeed = false;
    public int seismoEmission = 0;
    public int crystalCost;
}

public class HarvesterSpeedControl : MonoBehaviour
{
    public static HarvesterSpeedControl Instance { get; private set; }
    [SerializeField] public List<HarvesterSpeedStep> speedSteps;
    [SerializeField] private float crystalConsumptionInterval = 1f;
    private float timeSinceLastConsumption = 0f;
    private ItemManager itemManager;
    private int currentSpeedStepIndex = 0;
    [SerializeField] private SpeedSlider speedSlider;
    public UnityEvent overrodePosition;

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
        SetSpeed();
        Harvester.Instance.changedState.AddListener(OnHarvesterStateChanged);

        itemManager = ItemManager.Instance;
    }

    void OnEnable()
    {
        Harvester.Instance.changedState.AddListener(OnHarvesterStateChanged);
    }

    void OnDisable()
    {
        Harvester.Instance.changedState.RemoveListener(OnHarvesterStateChanged);
    }

    private void SetSpeed()
    {
        Harvester.Instance.mover.SetMoveSpeed(speedSteps[currentSpeedStepIndex].speed);
        Seismograph.Instance.SetOtherEmission(
            "Overspeed",
            speedSteps[currentSpeedStepIndex].seismoEmission
        );
    }

    void Update()
    {
        // reset speed to base speed if crystals are empty
        if (speedSteps.Count > 0)
        {
            HarvesterSpeedStep currentStep = speedSteps[currentSpeedStepIndex];

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
                else if (currentSpeedStepIndex > 0)
                {
                    currentSpeedStepIndex = speedSteps.FindIndex(step => step.isBaseSpeed);
                    SetSpeed();
                    OverrideSpeedStep(currentSpeedStepIndex);
                }
            }
            else
            {
                timeSinceLastConsumption += Time.deltaTime;
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
            Seismograph.Instance.SetOtherEmission(
                "Overspeed",
                speedSteps[currentSpeedStepIndex].seismoEmission
            );
            overrodePosition.Invoke();
        }
    }

    public void SetSpeedStepIndex(int index)
    {
        if (index >= 0 && index < speedSteps.Count && index != currentSpeedStepIndex)
        {
            if (Harvester.Instance.HasArrivedAtTarget())
            {
                OverrideSpeedStep(0);
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
