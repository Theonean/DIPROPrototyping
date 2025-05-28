using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class ACRocketBody : ACRocketComponent
{
    [Header("Debug")]
    public bool debugExplosionSphere; //If true, a gizmo will be drawn to show the explosion radius.
    [Header("Explosion Settings")]
    public Material explosionMaterial; //Material for the explosion effect when the rocket explodes.

    public GameObject explosionPrefab; //The explosion prefab to spawn when the rocket explodes.
    [SerializeField] protected float explosionRadius = 10f; // Radius of the explosion when the rocket explodes.
    [SerializeField] protected float[] explosionRadiusPerLevel = new float[5] { 10f, 15f, 20f, 25f, 30f }; // Explosion radius per level.
    public bool canExplode;
    public float explosionRadiusBase;
    public float explosionChainDelay;
    public float regrowDurationAfterExplosion;

    //Private settings
    protected abstract void Explode();
    public void TryExplode()
    {
        if (ParentRocket.CanExplode())
        {
            ParentRocket.SetState(RocketState.EXPLODING);
            Logger.Log("Rocket clicked", LogLevel.INFO, LogType.ROCKETS);
            Explode();

            parentRocket.ReattachRocketToDrone(RocketState.REGROWING);

            StartCoroutine(RegrowRocket());
        }
    }

    private IEnumerator RegrowRocket()
    {
        float t = 0;
        Vector3 startScale = Vector3.zero;
        rocketTransform.localScale = startScale;

        while (Vector3.Distance(rocketTransform.localScale, rocketOriginalScale) > 0.1f)
        {
            t += Time.deltaTime;
            rocketTransform.localScale = Vector3.Lerp(startScale, rocketOriginalScale, t / regrowDurationAfterExplosion);
            yield return null;
        }

        parentRocket.ReattachRocketToDrone();
    }

    protected override void SetStatsToLevel()
    {
        explosionRadiusBase = explosionRadiusPerLevel[componentLevel];
        explosionRadius = explosionRadiusBase;
        canExplode = true;

        Logger.Log($"Leveling up {DescriptiveName} to level {componentLevel}. Explosion radius: {explosionRadius}", LogLevel.INFO, LogType.ROCKETS);
    }

    public override string GetResearchDescription()
    {
        if (componentLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + explosionRadius;
        }
        else
        {
            return upgradeDescription + " " + explosionRadius + " -> " + explosionRadiusPerLevel[componentLevel + 1];
        }
    }
    public override string GetResearchDescription(int customLevel)
    {
        if (customLevel == maxComponentLevel)
        {
            return upgradeDescription + " " + explosionRadiusPerLevel[customLevel];
        }
        else
        {
            return upgradeDescription + " " + explosionRadiusPerLevel[customLevel] + " -> " + explosionRadiusPerLevel[customLevel + 1];
        }
    }
}