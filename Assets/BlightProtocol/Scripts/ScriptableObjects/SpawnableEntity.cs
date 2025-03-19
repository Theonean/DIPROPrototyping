using System.ComponentModel;
using UnityEngine;

public enum SpawnStrategy
{
    Random,
    Grid,
    Noise,
    Custom
}

[CreateAssetMenu(menuName = "World/SpawnableEntity")]
public class SpawnableEntity : ScriptableObject
{
    public GameObject[] worldEntityPrefabs;
    public int numEntities = 1;
    public SpawnStrategy spawnStrategy = SpawnStrategy.Random;
    public Vector2 customSpawnAreaX; // if needed
    public Vector2 customSpawnAreaZ; // if needed
    public float minRotation;
    public float maxRotation;
    [Description("Scale will be increased / decreased by a random value between -scaleVariance and scaleVariance.")]
    public float scaleVariance = 0f;

    public GameObject GetPrefab()
    {
        return worldEntityPrefabs[Random.Range(0, worldEntityPrefabs.Length)];
    }
}