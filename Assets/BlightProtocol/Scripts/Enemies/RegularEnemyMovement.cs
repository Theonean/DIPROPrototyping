
public class RegularEnemyMovement : ACEnemyMovementBehaviour
{
    // Update is called once per frame
    protected override void Update()
    {
        if (CanMove())
        {
            navMeshAgent.SetDestination(harvester.transform.position);
        }
    }
}
