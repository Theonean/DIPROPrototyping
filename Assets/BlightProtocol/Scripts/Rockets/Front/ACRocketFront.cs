using System.Collections;
using UnityEngine;

public abstract class ACRocketFront : ACRocketComponent
{
    public int abilityUsesLeft = 1;
    public int maxAbilityUses = 1;
    public int[] abilityUsesPerLevel = new int[5] { 1, 2, 3, 4, 5 };
    [Header("Ability Cooldown")]
    public bool hasAbilityCooldown = false;
    public float abilityCooldown = 0f;
    private float abilityCooldownTimer = 0f;
    private bool isOnCooldown = false;
    [Header("Ability Depletion Behaviour")]
    [SerializeField] private AnimationCurve depletionAnimationCurve;
    [SerializeField] private GameObject meshToPopOff;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (ParentRocket != null)
        {
            ParentRocket.OnRocketStateChange.AddListener(ShowFrontMesh);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (ParentRocket != null)
        {
            ParentRocket.OnRocketStateChange.RemoveListener(ShowFrontMesh);
        }
    }

    #region Level Up
    protected override void SetStatsToLevel()
    {
        maxAbilityUses = abilityUsesPerLevel[componentLevel];
        abilityUsesLeft = maxAbilityUses;
        Logger.Log($"Leveling up {DescriptiveName} to level {componentLevel}. Max ability uses: {maxAbilityUses}", LogLevel.INFO, LogType.ROCKETS);
    }
    
    #endregion
    #region  Ability usage
    // This is the public method called from outside
    public void ActivateAbility(Collider collider)
    {
        if (isOnCooldown)
            return;

        if (!HasAbilityUsesLeft())
        {
            parentRocket.Explode();
            return;
        }

        abilityUsesLeft--;

        OnActivateAbility(collider);

        if (abilityUsesLeft <= 0)
        {
            StartCoroutine(PopComponentOffInBezierArc());
        }

        if (hasAbilityCooldown) StartCoroutine(CooldownCoroutine());
    }

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
    public override string GetResearchDescription()
    {
        if (componentLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + maxAbilityUses;
        }
        else
        {
            return upgradeDescription + " " + maxAbilityUses + " -> " + abilityUsesPerLevel[componentLevel + 1];
        }
    }
    public override string GetResearchDescription(int customLevel)
    {
        if (customLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + abilityUsesPerLevel[customLevel];
        }
        else
        {
            return upgradeDescription + " " + abilityUsesPerLevel[customLevel] + " -> " + abilityUsesPerLevel[customLevel + 1];
        }
    }

    #endregion
    #region Pop Front off

    private void ShowFrontMesh(RocketState state)
    {
        if (state == RocketState.ATTACHED || state == RocketState.REGROWING || state == RocketState.RETURNING)
        {
            ToggleFrontMesh(true);
        }
        else
        {
            ToggleFrontMesh(false);
        }
    }

    private void ToggleFrontMesh(bool isActive)
    {
        meshToPopOff.SetActive(isActive);
    }

    private IEnumerator PopComponentOffInBezierArc()
    {
        GameObject mesh = Instantiate(meshToPopOff, transform.position, transform.localRotation, null);
        mesh.gameObject.SetActive(true);
        mesh.transform.localScale = transform.localScale;

        float timer = 0f;
        Vector3 startPos = transform.position;

        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0f; // Keep the direction horizontal
        randomDirection.Normalize();

        Vector3 endPos = startPos + randomDirection * 10f;
        Vector3 controlPoint = startPos + (endPos - startPos) / 2f + Vector3.up * 20f;

        Vector3 startScale = transform.localScale;
        float scaleMultiplier = 2f;

        while (timer < 1f)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer);
            mesh.transform.position = BezierCurve(startPos, controlPoint, endPos, t);
            mesh.transform.localScale = Vector3.Lerp(startScale, startScale * scaleMultiplier, depletionAnimationCurve.Evaluate(t));
            yield return null;
        }

        yield return new WaitForSeconds(10f);
        Destroy(mesh);
    }

    private Vector3 BezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 point = uu * p0; // (1-t)^2 * p0
        point += 2 * u * t * p1; // 2(1-t)t * p1
        point += tt * p2;        // t^2 * p2

        return point;
    }
    #endregion
}
