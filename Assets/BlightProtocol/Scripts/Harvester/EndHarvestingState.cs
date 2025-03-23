using UnityEngine;

public class EndHarvestingState : IHarvesterState
{
    private readonly ControlZoneManager harvester;
    private readonly ZoneState state = ZoneState.END_HARVESTING;

    public ZoneState State => state;

    public EndHarvestingState(ControlZoneManager harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Playing End Harvesting Animation");
    }

    public void Update()
    {
        if (harvester.animator.GetCurrentAnimationProgress() >= 1f)
        {
            harvester.SetState(new IdleState(harvester));
        }
    }

    public void Exit()
    {
        Debug.Log("Harvester: Transitioning to Move State");
    }
}
