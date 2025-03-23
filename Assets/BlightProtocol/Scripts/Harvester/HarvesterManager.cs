using UnityEngine;

public class HarvesterManager : MonoBehaviour
{
    private HarvesterStateMachine stateMachine;

    private void Start()
    {
        stateMachine = new HarvesterStateMachine();
        //SetState(new IdleState(this)); // Start in idle
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void SetState(IHarvesterState newState)
    {
        stateMachine.SetState(newState);
    }

    // === Example methods used by state classes ===
    public void StartMoving() { /* play move anims, SFX */ }
    public void UpdateMovement() { /* move & rotate */ }
    //public bool HasArrivedAtTarget() => Vector3.Distance(transform.position, /*target*/) < 0.5f;
    //public bool IsOnResourcePoint() => /* logic to check */;
    public void BeginHarvesting() { /* reset timer, play VFX */ }
    public void UpdateHarvesting() { /* tick timer, collect resource */ }
    //public bool HasCompletedHarvest() => /* timer check */;
    //public void StopHarvesting() { /* stop SFX/VFX */ }
}
