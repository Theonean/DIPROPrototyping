using UnityEngine;

public class EndHarvestingState : IHarvesterState
{
    private readonly Harvester harvester;
    private readonly HarvesterState state = HarvesterState.END_HARVESTING;

    public HarvesterState State => state;

    public EndHarvestingState(Harvester harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Playing End Harvesting Animation");
    }

    public void Update()
    {
        if (harvester.animator.GetCurrentAnimationProgress(HARVESTER_ANIMATION.Stop_Harvesting) >= 1f)
        {
            harvester.SetState(new IdleState(harvester));
        }
    }

    public void Exit()
    {
        Debug.Log("Harvester: Transitioning to Move State");
    }
}
