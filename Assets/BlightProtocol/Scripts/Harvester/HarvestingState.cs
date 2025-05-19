using UnityEngine;

public class HarvestingState : IHarvesterState
{
    private readonly Harvester harvester;
    private readonly HarvesterState state = HarvesterState.HARVESTING;

    public HarvesterState State => state;

    public HarvestingState(Harvester harvester)
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
