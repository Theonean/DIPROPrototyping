
public class RegularEnemyMovement : ACEnemyMovementBehaviour
{
    // Update is called once per frame
    protected override void CustomMovementUpdate()
    {
        if (CanMove())
        {
            navMeshAgent.SetDestination(harvester.transform.position);
        }
    }
}
