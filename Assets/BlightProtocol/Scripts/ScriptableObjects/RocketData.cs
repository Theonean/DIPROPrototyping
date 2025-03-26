using UnityEngine;

[CreateAssetMenu(fileName = "RocketData", menuName = "RocketData", order = 0)]
public class RocketData : ScriptableObject
{
    public float flySpeed;
    public float flySpeedBase;
    public float flyScaleMultiplier;
    public AnimationCurve flySpeedCurve;
    public int damage;
    [Header("Explosion")]
    public bool canExplode;
    public float explosionRadiusBase;
    public float explosionRadius;
    public float explosionChainDelay;
    public float regrowDurationAfterExplosion;

    public void ApplyTemporaryBoost(ECollectibleType type)
    {
        switch (type)
        {
            case ECollectibleType.ExplosionRange:
                explosionRadius = explosionRadiusBase * Collectible.explosionRangeMultiplier;
                UIStatsDisplayer.Instance.explosionRangeBuffTimerFinished.AddListener(() => explosionRadius = explosionRadiusBase);
                break;
            case ECollectibleType.ShotSpeed:
                flySpeed = flySpeedBase * Collectible.shotSpeedMultiplier;
                UIStatsDisplayer.Instance.shotspeedBuffTimerFinished.AddListener(() => flySpeed = flySpeedBase);
                break;
            case ECollectibleType.FullHealth:
                break;
        }
    }
}