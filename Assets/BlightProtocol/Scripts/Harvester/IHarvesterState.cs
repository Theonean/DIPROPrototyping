public interface IHarvesterState
{
    ZoneState State { get; }
    void Enter();
    void Update();
    void Exit();
}

