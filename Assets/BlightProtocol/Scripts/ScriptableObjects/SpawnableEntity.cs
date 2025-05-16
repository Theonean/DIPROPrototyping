using System.ComponentModel;
using System.Linq;
using UnityEngine;

public enum SpawnStrategy
{
    Random,
    Noise,
    VoronoiNoise,
    RandomCustomArea
}

[CreateAssetMenu(menuName = "World/SpawnableEntity")]
public class SpawnableEntity : ScriptableObject
{
    public GameObject[] worldEntityPrefabs;
    public int numEntities = 1;
    public SpawnStrategy spawnStrategy = SpawnStrategy.Random;
    public Vector2 customSpawnAreaBoundsX; // if needed
    public Vector2 customSpawnAreaBoundsZ; // if needed[Header("Voronoi Settings")]
    [Description("Number of Voronoi regions to generate.")]
    public int voronoiSiteCount = 10; // Number of regions
    [Description("Spread radius around each Voronoi cell point.")]
    public float siteSpreadRadius = 3f; // Spread around each site
    [Description("Extra randomization to avoid clustering.")]
    public float siteJitter = 0.2f; // Extra randomization to avoid clustering

    public float minRotation;
    public float maxRotation;
    public Vector2 scaleVariance = new Vector2(1,1);

    [Description("Auto-Calculated Bounds radius for better position generation")]
    public float boundingRadius = 1f; // Auto-calculated
    [Description("Margin for objects with specific boundary radius needs")]
    public float boundRadiusMargin = 0f;

    public GameObject GetPrefab()
    {
        return worldEntityPrefabs[Random.Range(0, worldEntityPrefabs.Length)];
    }

    public Vector3[] GenerateSpawnPositions(Vector2 mapBoundsX, Vector2 mapBoundsZ)
    {
        Vector3[] positions = new Vector3[numEntities];
        Vector3 spawnPos = Vector3.zero;
        for (int i = 0; i < numEntities; i++)
        {
            switch (spawnStrategy)
            {
                case SpawnStrategy.Random:
                    {
                        positions[i] = GenerateRandomSpawnPosition(mapBoundsX, mapBoundsZ);
                        break;
                    }
                case SpawnStrategy.Noise:
                    {
                        positions[i] = GenerateNoiseSpawnPosition(mapBoundsX, mapBoundsZ);
                        break;
                    }
                case SpawnStrategy.RandomCustomArea:
                    {
                        positions[i] = GenerateRandomCustomSpawnPosition();
                        break;
                    }
                case SpawnStrategy.VoronoiNoise:
                    {
                        positions[i] = GenerateVoronoiSpawnPosition(mapBoundsX, mapBoundsZ);
                        break;
                    }
                default:
                    {
                        Debug.LogError("Invalid spawn strategy selected. Defaulting to Random.");
                        positions[i] = Vector3.zero;
                        break;
                    }
            }

        }
        return positions;
    }


    public Vector3 GenerateSingleSpawnPosition(Vector2 mapBoundsX, Vector2 mapBoundsZ)
    {
        switch (spawnStrategy)
        {
            case SpawnStrategy.Random:
                return GenerateRandomSpawnPosition(mapBoundsX, mapBoundsZ);
            case SpawnStrategy.Noise:
                return GenerateNoiseSpawnPosition(mapBoundsX, mapBoundsZ);
            case SpawnStrategy.RandomCustomArea:
                return GenerateRandomCustomSpawnPosition();
            case SpawnStrategy.VoronoiNoise:
                return GenerateVoronoiSpawnPosition(mapBoundsX, mapBoundsZ);
            default:
                return Vector3.zero;
        }
    }

    private Vector3 GenerateRandomSpawnPosition(Vector2 xBounds, Vector2 zBounds)
    {
        return new Vector3(
            Random.Range(xBounds.x, xBounds.y),
            0f,
            Random.Range(zBounds.x, zBounds.y));
    }

    private Vector3 GenerateRandomCustomSpawnPosition()
    {
        return new Vector3(
            Random.Range(customSpawnAreaBoundsX.x, customSpawnAreaBoundsX.y),
            0f,
            Random.Range(customSpawnAreaBoundsZ.x, customSpawnAreaBoundsZ.y));
    }

    private Vector3 GenerateNoiseSpawnPosition(Vector2 xBounds, Vector2 zBounds)
    {
        float x = Mathf.PerlinNoise(Random.value, Random.value) * (xBounds.y - xBounds.x) + xBounds.x;
        float z = Mathf.PerlinNoise(Random.value, Random.value) * (zBounds.y - zBounds.x) + zBounds.x;
        return new Vector3(x, 0f, z);
    }

    private Vector3 GenerateVoronoiSpawnPosition(Vector2 xBounds, Vector2 zBounds)
    {
        int siteCount = Mathf.Max(1, voronoiSiteCount);
        Vector2[] sites = new Vector2[siteCount];

        for (int i = 0; i < siteCount; i++)
            sites[i] = new Vector2(Random.Range(xBounds.x, xBounds.y), Random.Range(zBounds.x, zBounds.y));

        Vector2 randPoint = new Vector2(Random.Range(xBounds.x, xBounds.y), Random.Range(zBounds.x, zBounds.y));
        Vector2 closestSite = sites.OrderBy(s => Vector2.SqrMagnitude(s - randPoint)).First();
        Vector2 jittered = closestSite + Random.insideUnitCircle * siteSpreadRadius + Random.insideUnitCircle * siteJitter;

        return new Vector3(
            Mathf.Clamp(jittered.x, xBounds.x, xBounds.y),
            0f,
            Mathf.Clamp(jittered.y, zBounds.x, zBounds.y));
    }



#if UNITY_EDITOR
    [ContextMenu("Recalculate Bounding Radius")]
    public void RecalculateBoundingRadius()
    {
        float maxRadius = 0f;

        foreach (GameObject prefab in worldEntityPrefabs)
        {
            if (prefab == null) continue;

            // Use Renderer bounds if available
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
            Bounds combinedBounds = new Bounds(prefab.transform.position, Vector3.zero);

            foreach (Renderer renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }

            // Fallback to colliders if no renderers
            if (renderers.Length == 0)
            {
                Collider[] colliders = prefab.GetComponentsInChildren<Collider>();
                foreach (Collider collider in colliders)
                {
                    combinedBounds.Encapsulate(collider.bounds);
                }
            }

            float radius = combinedBounds.extents.magnitude;
            if (radius > maxRadius)
                maxRadius = radius;
        }

        boundingRadius = maxRadius;
        Debug.Log($"[{name}] Bounding radius recalculated: {boundingRadius}");
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif


}