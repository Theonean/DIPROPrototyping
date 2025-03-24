using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SeismographEmission
{
    public ZoneState zoneState;
    public float strength;
}

public class Seismograph : MonoBehaviour
{
    private Harvester harvester;
    [SerializeField] private List<SeismographEmission> seismographEmissions;
    private List<SeismographEmission> activeEmissions = new List<SeismographEmission>();
    private ZoneState previousZoneState;
    void Start()
    {
        harvester = Harvester.Instance;
        harvester.changedState.AddListener(HarvesterChangedState);
        previousZoneState = harvester.GetZoneState();
    }

    private void HarvesterChangedState(ZoneState zoneState)
    {
        if (zoneState != previousZoneState)
        {
            activeEmissions.Add(seismographEmissions.Find(seismographEmission => seismographEmission.zoneState == zoneState));
            activeEmissions.Remove(seismographEmissions.Find(seismographEmission => seismographEmission.zoneState == previousZoneState));
        }
    }
}
