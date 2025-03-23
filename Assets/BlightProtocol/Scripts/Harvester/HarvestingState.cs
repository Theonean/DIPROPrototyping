using UnityEngine;

public class HarvestingState : IHarvesterState
{
    private readonly ControlZoneManager harvester;
    private readonly ZoneState state = ZoneState.HARVESTING;

    public ZoneState State => state;

    public HarvestingState(ControlZoneManager harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Started Harvesting");
        harvester.BeginHarvesting();
    }

    public void Update()
    {
        harvester.UpdateHarvesting();
        
        if (harvester.HasCompletedHarvest())
        {
            harvester.SetState(new EndHarvestingState(harvester));
        }
    }

    public void Exit()
    {
        harvester.StopHarvesting(); // stop VFX/SFX
        Debug.Log("Harvester: Finished Harvesting");
    }
}
