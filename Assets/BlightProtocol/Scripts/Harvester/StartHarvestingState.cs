using UnityEngine;

public class StartHarvestingState : IHarvesterState
{
    private readonly Harvester harvester;
    private readonly HarvesterState state = HarvesterState.START_HARVESTING;

    public HarvesterState State => state;

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
        if (harvester.animator.GetCurrentAnimationProgress(HARVESTER_ANIMATION.Start_Harvesting) >= 1f)
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
