
using UnityEngine;

public abstract class ACRocketFront : ACRocketComponent
{
    public int abilityUsesLeft = 1;
    public abstract void ActivateAbility(Collider collider);
}