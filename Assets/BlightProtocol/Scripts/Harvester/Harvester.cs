using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.VFX;

public enum ZoneState
{
    MOVING, //Moving from resource point to resource point
    START_HARVESTING, //Starting to harvest resources, animation
    HARVESTING, //"Gathering" Resources and vulnerable to enemy attacks
    END_HARVESTING, //Ending harvest, animation
    IDLE, //Not doing anything
    DIED //Dead, no longer doing anything
}

public class Harvester : MonoBehaviour
{

    public static Harvester Instance { get; private set; }

    private float timeUntilResourcePointEmpty;
    private float harvestingTimer = 0f;

    public float moveSpeed = 5f;
    ZoneState state = ZoneState.IDLE;
    public List<Slider> waveProgressSliders = new List<Slider>();
    public UnityEvent<ZoneState> changedState;
    public float travelTimeLeft;

    //Harvester Sub-Systems with specific tasks
    public HarvesterAnimator animator { get; private set; }
    public HarvesterMover mover { get; private set; }
    public HarvesterHealth health { get; private set; }
    public HarvesterSFX sFX { get; private set; }
    public HarvesterResourcePointDetector resourcePointDetector { get; private set; }

    // VFX
    [SerializeField] private VisualEffect drillingVFX;

    [Header("Resource Harvesting")]
    public float resourceHarvestingSpeed = 1f;
    private HarvesterStateMachine stateMachine;

    // Harvesting

    private void Awake()
    {
        // Ensure there's only one instance of the FrankenGameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        animator = GetComponentInChildren<HarvesterAnimator>();
        mover = GetComponentInChildren<HarvesterMover>();
        sFX = GetComponentInChildren<HarvesterSFX>();
        health = GetComponentInChildren<HarvesterHealth>();
        resourcePointDetector = GetComponentInChildren<HarvesterResourcePointDetector>();
    }

    void Start()
    {
        stateMachine = new HarvesterStateMachine();

        health.died.AddListener(() => { state = ZoneState.DIED; changedState.Invoke(state); });

        harvestingTimer = 0f;

        foreach (Slider slider in waveProgressSliders)
        {
            slider.maxValue = timeUntilResourcePointEmpty;
            slider.value = 0f;
        }

        changedState.Invoke(state);
    }
    private void Update()
    {
        stateMachine.Update();
    }


    public void SetState(IHarvesterState newState)
    {
        state = newState.State;
        changedState.Invoke(state);
        stateMachine.SetState(newState);
    }

    public void UpdateHarvesting()
    {
        //Collect Resource
        ResourcePoint resourcePoint = resourcePointDetector.activeResourcePoints[0].GetComponent<ResourcePoint>();

        harvestingTimer += Time.deltaTime;
        foreach (Slider slider in waveProgressSliders)
        {
            slider.value = harvestingTimer;
        }
        resourcePoint.HarvestResource(resourceHarvestingSpeed * Time.deltaTime);
    }

    public void BeginHarvesting()
    {
        drillingVFX.Play();

        ResourcePoint resourcePoint = mover.targetPosObject.activeResourcePoint.GetComponent<ResourcePoint>();
        timeUntilResourcePointEmpty = resourcePoint.resourceAmount / resourceHarvestingSpeed;

        foreach (Slider slider in waveProgressSliders)
        {
            slider.enabled = true;
            slider.maxValue = timeUntilResourcePointEmpty;
        }
        harvestingTimer = 0f;
    }

    public void StopHarvesting()
    {
        ResourcePoint resourcePoint = resourcePointDetector.activeResourcePoints[0].GetComponent<ResourcePoint>();

        if (resourcePoint.resourceAmount <= 0f)
            Destroy(resourcePoint.gameObject);

        WaveManager.Instance.IncreaseDifficultyLevel(1);

        drillingVFX.Stop();
        StartCoroutine(ReduceWaveTimerOverTimeIDontKnowHowToNameThis(1f));
    }
    public bool HasCompletedHarvest() => harvestingTimer >= timeUntilResourcePointEmpty;
    public bool HasArrivedAtTarget() => Vector3.Distance(transform.position, mover.targetPosObject.transform.position) < 0.5f;
    public bool IsTargetingResourcePoint() => mover.targetPosObject.isOnResourcePoint;

    IEnumerator ReduceWaveTimerOverTimeIDontKnowHowToNameThis(float time)
    {
        float timer = 0f;
        while (timer < time)
        {
            timer += Time.deltaTime;

            //Lerp wave timer slider to 0 over given time
            foreach (Slider slider in waveProgressSliders)
            {
                slider.value = Mathf.Lerp(timeUntilResourcePointEmpty, 0f, timer / time);
            }
            yield return null;
        }
    }

    public ZoneState GetZoneState()
    {
        return state;
    }
}