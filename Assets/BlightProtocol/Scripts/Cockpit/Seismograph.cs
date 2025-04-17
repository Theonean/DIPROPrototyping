using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HarvesterEmission
{
    public ZoneState zoneState;
    public float strength;
}

[System.Serializable]
public class OtherEmission
{
    public string sourceId;
    public float strength;
}

[System.Serializable]
public class VibrationDangerLevel
{
    public float threshold;
    public Color color;
}

public class Seismograph : MonoBehaviour
{
    private Harvester harvester;
    public static Seismograph Instance { get; private set; }

    [SerializeField] private List<HarvesterEmission> harvesterEmissions;
    private HarvesterEmission currentHarvesterEmission;

    private List<OtherEmission> otherEmissionsList = new List<OtherEmission>();
    private Dictionary<string, float> otherEmissions = new Dictionary<string, float>();
    public UnityEvent vibrationChanged;

    public float totalVibration = 0f;
    public int currentDangerLevel = 0;
    [SerializeField] public List<VibrationDangerLevel> vibrationDangerLevels = new List<VibrationDangerLevel>();

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
        harvester = Harvester.Instance;
        harvester.changedState.AddListener(UpdateHarvesterEmission);

        ZoneState initialState = harvester.GetZoneState();
        currentHarvesterEmission = harvesterEmissions.Find(e => e.zoneState == initialState);
        if (currentHarvesterEmission != null)
        {
            totalVibration = currentHarvesterEmission.strength;
            vibrationChanged.Invoke();
        }
    }

    private void UpdateHarvesterEmission(ZoneState newZoneState)
    {
        if (currentHarvesterEmission != null)
        {
            totalVibration -= currentHarvesterEmission.strength;
        }

        currentHarvesterEmission = harvesterEmissions.Find(e => e.zoneState == newZoneState);

        if (currentHarvesterEmission != null)
        {
            totalVibration += currentHarvesterEmission.strength;
        }
        vibrationChanged.Invoke();
    }

    public void SetOtherEmission(string sourceId, float strength, float? duration = null)
    {
        
        if (!otherEmissions.ContainsKey(sourceId))
        {
            otherEmissionsList.Add(new OtherEmission { sourceId = sourceId, strength = strength });
        }
        else {
            totalVibration -= otherEmissions[sourceId];
            otherEmissionsList.Find(e => e.sourceId == sourceId).strength = strength;
        }
        otherEmissions[sourceId] = strength;
        totalVibration += strength;

        if (duration.HasValue)
        {
            StartCoroutine(RemoveEmissionAfterDuration(sourceId, duration.Value));
        }
        vibrationChanged.Invoke();
    }

    private IEnumerator RemoveEmissionAfterDuration(string sourceId, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveOtherEmission(sourceId);
    }

    public void RemoveOtherEmission(string sourceId)
    {
        if (otherEmissions.TryGetValue(sourceId, out float strength))
        {
            totalVibration -= strength;
            otherEmissions.Remove(sourceId);
            otherEmissionsList.RemoveAll(entry => entry.sourceId == sourceId);
        }
        vibrationChanged.Invoke();
    }

    public float GetTotalVibration()
    {
        return totalVibration;
    }

    public int GetCurrentDangerLevel() {
        foreach (var level in vibrationDangerLevels) {
            if (totalVibration >= level.threshold) {
                currentDangerLevel = vibrationDangerLevels.IndexOf(level);
            }
        }
        return currentDangerLevel;
    }
}
