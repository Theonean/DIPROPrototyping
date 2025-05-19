public interface IHarvesterState
{
    HarvesterState State { get; }
    void Enter();
    void Update();
    void Exit();
}

