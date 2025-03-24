using UnityEngine;

public class StartHarvestingState : IHarvesterState
{
    private readonly Harvester harvester;
    private readonly ZoneState state = ZoneState.START_HARVESTING;

    public ZoneState State => state;

    public StartHarvestingState(Harvester harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Starting Harvest Animation");
    }

    public void Update()
    {
        if (harvester.animator.GetCurrentAnimationProgress() >= 1f)
        {
            Debug.Log("switching to harvesting state");
            harvester.SetState(new HarvestingState(harvester));
        }
    }

    public void Exit()
    {
        Debug.Log("Harvester: Finished Start Harvesting Animation");
    }
}
