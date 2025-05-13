using System.Collections;
using System.Linq;
using UnityEngine;

public class WorldComposer : MonoBehaviour
{
    public static WorldComposer Instance { get; private set; }
    public DifficultyRegion[] difficultyRegions = new DifficultyRegion[0];

    private Vector2 mapBoundsX;
    [SerializeField] private float mapBoundsXMargins = 100;
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
        mapBoundsX = ProceduralTileGenerator.Instance.mapBoundsX + new Vector2(mapBoundsXMargins, -mapBoundsXMargins);
        mapBoundsZ = ProceduralTileGenerator.Instance.mapBoundsZ;

        StartCoroutine(GenerateInitialObstacles());
    }

    private IEnumerator GenerateInitialObstacles()
    {
        SpatialHashGrid hashGrid = new SpatialHashGrid(cellSize: 40f);

        foreach (DifficultyRegion region in difficultyRegions)
        {
            foreach (SpawnableEntity spawnable in region.spawnableEntities)
            {
                float spacing = spawnable.boundingRadius * 2f;
                int placedCount = 0;
                int attemptsPerEntity = 3;

                while (placedCount < spawnable.numEntities)
                {
                    bool success = false;

                    for (int attempt = 0; attempt < attemptsPerEntity; attempt++)
                    {
                        Vector3 tryPos = spawnable.GenerateSingleSpawnPosition(mapBoundsX, region.boundsZ);

                        if (Vector3.Distance(Harvester.Instance.transform.position, tryPos) < 100f)
                            continue;

                        if (!hashGrid.IsPositionOccupied(tryPos, spacing))
                        {
                            GameObject prefab = spawnable.GetPrefab();
                            Quaternion rotation = Quaternion.Euler(0, Random.Range(spawnable.minRotation, spawnable.maxRotation), 0);
                            GameObject instance = Instantiate(prefab, tryPos, rotation, transform);

                            float scaleFactor = Random.Range(spawnable.scaleVariance.x, spawnable.scaleVariance.y);
                            instance.transform.localScale *= scaleFactor;

                            hashGrid.AddPosition(tryPos);
                            placedCount++;
                            success = true;
                            break;
                        }
                    }

                    if (!success)
                    {
                        Debug.LogWarning($"[{spawnable.name}] Could not place entity {placedCount + 1}/{spawnable.numEntities} after {attemptsPerEntity} tries.");
                        placedCount++; // Move on to avoid infinite loop
                    }
                }
            }

            yield return null;
        }
    }
}