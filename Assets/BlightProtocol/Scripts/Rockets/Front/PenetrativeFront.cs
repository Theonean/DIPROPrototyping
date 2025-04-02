using UnityEngine;

public class PenetrativeFront : ACRocketFront
{
    public override void ActivateAbility(Collider collider)
    {
        if (HasAbilityUsesLeft())
        {
            // Check if the collider is an enemy armor
            if (collider.gameObject.layer == LayerMask.NameToLayer("EnemyArmor"))
            {
                // Destroy the enemy armor
                Destroy(collider.gameObject);
                abilityUsesLeft--;
            }
        }
        else
        {
            // If no ability uses left, destroy the rocket
            parentRocket.Explode();
        }
    }
}
