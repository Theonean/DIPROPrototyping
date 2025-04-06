using UnityEngine;
using UnityEngine.UI;

public class MovingState : IHarvesterState
{
    private readonly Harvester harvester;
    private readonly ZoneState state = ZoneState.MOVING;

    public ZoneState State => state;

    public MovingState(Harvester harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Entering Moving State");
        harvester.mover.StartMoving();

        foreach (Slider slider in harvester.waveProgressSliders)
        {
            slider.enabled = false;
        }
    }

    public void Update()
    {
        if (harvester.HasArrivedAtTarget())
        {
            harvester.SetState(new IdleState(harvester));
            /*
            if (harvester.IsTargetingResourcePoint())
            {
                harvester.SetState(new StartHarvestingState(harvester));
            }
            else
            {
                harvester.SetState(new IdleState(harvester));
            }*/
        }
    }

    public void Exit()
    {
        Debug.Log("Harvester: Exiting Moving State");
    }
}
