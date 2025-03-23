using UnityEngine;

public class IdleState : IHarvesterState
{
    private readonly ControlZoneManager harvester;
    private readonly ZoneState state = ZoneState.IDLE;

    public ZoneState State => state;

    public IdleState(ControlZoneManager harvester)
    {
        this.harvester = harvester;
    }

    public void Enter()
    {
        Debug.Log("Harvester: Entering Idle State");
    }

    public void Update()
    {
        // Could wait for orders or timeouts to move again
    }

    public void Exit()
    {
        Debug.Log("Harvester: Exiting Idle State");
    }
}
