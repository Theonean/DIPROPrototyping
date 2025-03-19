using System.Collections;
using UnityEngine;

public class WorldComposer : MonoBehaviour
{
    public static WorldComposer Instance { get; private set; }
    public SpawnableEntity[] spawnableResources = new SpawnableEntity[0];

    private Vector2 mapBoundsX;
    private Vector2 mapBoundsZ;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Get the world bounds from your ProceduralTileGenerator (or another source)
        mapBoundsX = ProceduralTileGenerator.Instance.mapBoundsX;
        mapBoundsZ = ProceduralTileGenerator.Instance.mapBoundsZ;

        StartCoroutine(GenerateInitialObstacles());
    }

    private IEnumerator GenerateInitialObstacles()
    {
        foreach (SpawnableEntity spawnable in spawnableResources)
        {
            for (int i = 0; i < spawnable.numEntities; i++)
            {
                // Get the spawn position using the configured strategy.
                Vector3 spawnPos = GetSpawnPosition(spawnable);

                // Get a random prefab from the scriptable object.
                GameObject prefab = spawnable.GetPrefab();

                // Create a random rotation between the provided min and max angles.
                float randomRotation = Random.Range(spawnable.minRotation, spawnable.maxRotation);
                Quaternion rotation = Quaternion.Euler(0, randomRotation, 0);

                // Instantiate the prefab with the chosen position and rotation.
                GameObject instance = Instantiate(prefab, spawnPos, rotation, transform);

                // Optionally apply a random scale variance.
                if (spawnable.scaleVariance != 0f)
                {
                    float scaleFactor = 1f + Random.Range(-spawnable.scaleVariance, spawnable.scaleVariance);
                    instance.transform.localScale *= scaleFactor;
                }

                // Optionally, wait a frame between spawns.
                yield return null;
            }
        }
    }

    private Vector3 GetSpawnPosition(SpawnableEntity spawnable)
    {
        switch (spawnable.spawnStrategy)
        {
            case SpawnStrategy.Random:
                {
                    float xPos = Random.Range(mapBoundsX.x, mapBoundsX.y);
                    float zPos = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
                    return new Vector3(xPos, 0, zPos);
                }
            case SpawnStrategy.Grid:
                {
                    // Example: evenly distribute positions across the bounds.
                    float gridX = Mathf.Lerp(mapBoundsX.x, mapBoundsX.y, Random.value);
                    float gridZ = Mathf.Lerp(mapBoundsZ.x, mapBoundsZ.y, Random.value);
                    return new Vector3(gridX, 0, gridZ);
                }
            case SpawnStrategy.Noise:
                {
                    // Example: use Perlin noise to vary the position.
                    float noiseX = Mathf.PerlinNoise(Random.value, Random.value) * (mapBoundsX.y - mapBoundsX.x) + mapBoundsX.x;
                    float noiseZ = Mathf.PerlinNoise(Random.value, Random.value) * (mapBoundsZ.y - mapBoundsZ.x) + mapBoundsZ.x;
                    return new Vector3(noiseX, 0, noiseZ);
                }
            case SpawnStrategy.Custom:
                {
                    // Example: use custom spawn area if provided.
                    float xPos = Random.Range(spawnable.customSpawnAreaX.x, spawnable.customSpawnAreaX.y);
                    float zPos = Random.Range(spawnable.customSpawnAreaZ.x, spawnable.customSpawnAreaZ.y);
                    return new Vector3(xPos, 0, zPos);
                }
            default:
                {
                    // Fallback to random spawn if strategy is unrecognized.
                    float xPos = Random.Range(mapBoundsX.x, mapBoundsX.y);
                    float zPos = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
                    return new Vector3(xPos, 0, zPos);
                }
        }
    }
}
