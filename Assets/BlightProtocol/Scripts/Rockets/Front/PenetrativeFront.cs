using UnityEngine;

public class PenetrativeFront : ACRocketFront
{
    protected override void OnActivateAbility(Collider collider)
    {
        // Check if the collider is an enemy armor
        if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyArmor"))
        {
            // Destroy the enemy armor
            Destroy(collider.gameObject);
        }
    }
}
