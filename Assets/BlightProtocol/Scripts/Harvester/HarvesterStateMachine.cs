public class HarvesterStateMachine
{
    private IHarvesterState currentState;

    public void SetState(IHarvesterState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }
}
