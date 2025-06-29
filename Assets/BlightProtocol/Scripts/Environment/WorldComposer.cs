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

    SpatialHashGrid hashGrid = new SpatialHashGrid(cellSize: 40f);

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
        foreach (DifficultyRegion region in difficultyRegions)
        {
            SpawnableEntity[] gameplayEnvironmentPieces = region.spawnableEntities.Where(x => x.isGamePlayRelevant).ToArray();
            foreach (SpawnableEntity spawnable in gameplayEnvironmentPieces)
            {
                PlaceSpawnableEntity(spawnable, region);
            }

            SpawnableEntity[] environmentPieces = region.spawnableEntities.Where(x => !x.isGamePlayRelevant).ToArray();
            foreach (SpawnableEntity spawnable in environmentPieces)
            {
                PlaceSpawnableEntity(spawnable, region);
            }

            yield return null;
        }
    }

    public int GetDifficultyLevelFromZ(float z)
    {
        int i = 0;
        foreach(DifficultyRegion dr in difficultyRegions)
        {
            if (z > dr.boundsZ.x && z < dr.boundsZ.y)
            {
                return i;
            }

            i++;
        }

        Debug.LogError("Position z" + z + " not valid for difficulty regions");
        return -1;
    }

    private void PlaceSpawnableEntity(SpawnableEntity spawnable, DifficultyRegion region)
    {

        float spacing = (spawnable.boundingRadius + spawnable.boundRadiusMargin) * 2f;
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
}