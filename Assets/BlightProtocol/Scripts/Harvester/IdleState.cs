using UnityEngine;

public class IdleState : IHarvesterState
{
    private readonly Harvester harvester;
    private readonly HarvesterState state = HarvesterState.IDLE;

    public HarvesterState State => state;

    public IdleState(Harvester harvester)
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
