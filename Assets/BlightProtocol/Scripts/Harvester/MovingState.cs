using UnityEngine;

public class MovingState : IHarvesterState
{
    private readonly ControlZoneManager harvester;
    private readonly ZoneState state = ZoneState.MOVING;

    public ZoneState State => state;

    public MovingState(ControlZoneManager harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Entering Moving State");
        harvester.mover.StartMoving();

        harvester.waveProgressSlider.enabled = false;
    }

    public void Update()
    {
        if (harvester.HasArrivedAtTarget())
        {
            if (harvester.IsTargetingResourcePoint())
            {
                harvester.SetState(new StartHarvestingState(harvester));
            }
            else
            {
                harvester.SetState(new IdleState(harvester));
            }
        }
    }

    public void Exit()
    {
        Debug.Log("Harvester: Exiting Moving State");
    }
}
