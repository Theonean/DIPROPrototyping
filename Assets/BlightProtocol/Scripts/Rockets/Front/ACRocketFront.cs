using System.Collections;
using UnityEngine;

public abstract class ACRocketFront : ACRocketComponent
{
    [HideInInspector] public int abilityUsesLeft = 1;
    public int maxAbilityUses = 1;
    [Header("Ability Cooldown")]
    public bool hasAbilityCooldown = false;
    public float abilityCooldown = 0f;
    private float abilityCooldownTimer = 0f;
    private bool isOnCooldown = false;

    // This is the public method called from outside
    public void ActivateAbility(Collider collider)
    {
        if (isOnCooldown)
            return;

        if (!HasAbilityUsesLeft())
        {
            if (FrankenGameManager.Instance.destroyRocketsAfterAbiltieUsesDepleted)
            {
                parentRocket.Explode();
            }
            return;
        }

        abilityUsesLeft--;

        OnActivateAbility(collider);

        if (hasAbilityCooldown) StartCoroutine(CooldownCoroutine());
    }

    // Abstract method that children must implement
    protected abstract void OnActivateAbility(Collider collider);

    public bool HasAbilityUsesLeft()
    {
        return abilityUsesLeft > 0;
    }

    private IEnumerator CooldownCoroutine()
    {
        isOnCooldown = true;
        abilityCooldownTimer = abilityCooldown;

        while (abilityCooldownTimer > 0)
        {
            abilityCooldownTimer -= Time.deltaTime;
            yield return null;
        }

        isOnCooldown = false;
    }
}
