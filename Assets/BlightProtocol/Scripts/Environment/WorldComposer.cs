using System.Collections;
using System.Linq;
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
            Vector3[] spawnPositions = spawnable.GenerateSpawnPositions(mapBoundsX, mapBoundsZ);
            if (spawnPositions.Length == 0)
            {
                Debug.LogWarning($"No spawn positions generated for {spawnable.name}");
                continue;
            }

            spawnPositions = spawnPositions.OrderBy(pos => Vector3.Distance(Harvester.Instance.transform.position, pos)).ToArray();

            foreach (Vector3 spawnPos in spawnPositions)
            {
                // Instantiate the prefab at the generated position.
                GameObject prefab = spawnable.GetPrefab();
                Quaternion rotation = Quaternion.Euler(0, Random.Range(spawnable.minRotation, spawnable.maxRotation), 0);
                GameObject instance = Instantiate(prefab, spawnPos, rotation, transform);

                // Optionally apply a random scale variance.
                if (spawnable.scaleVariance != 0f)
                {
                    float scaleFactor = 1f + Random.Range(-spawnable.scaleVariance, spawnable.scaleVariance);
                    instance.transform.localScale *= scaleFactor;
                }

                yield return null;
            }
        }
    }
}
