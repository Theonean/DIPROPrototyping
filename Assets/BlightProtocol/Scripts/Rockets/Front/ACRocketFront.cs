
using UnityEngine;

public abstract class ACRocketFront : ACRocketComponent
{
    public int abilityUsesLeft = 1;
    public int maxAbilityUses = 1;
    public abstract void ActivateAbility(Collider collider);

    public bool HasAbilityUsesLeft()
    {
        return abilityUsesLeft > 0;
    }
}