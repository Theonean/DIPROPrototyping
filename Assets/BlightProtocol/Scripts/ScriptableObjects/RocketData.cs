using UnityEngine;

[CreateAssetMenu(fileName = "RocketData", menuName = "RocketData", order = 0)]
public class RocketData : ScriptableObject
{
    public int damage;
    [Header("Explosion")]
    public bool canExplode;
    public float explosionRadiusBase;
    public float explosionRadius;
    public float explosionChainDelay;
    public float regrowDurationAfterExplosion;
}