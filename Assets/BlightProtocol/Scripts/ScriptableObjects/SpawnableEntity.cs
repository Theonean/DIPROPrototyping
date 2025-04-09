using System.ComponentModel;
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
    [Description("Scale will be increased / decreased by a random value between -scaleVariance and scaleVariance.")]
    public float scaleVariance = 0f;

    public GameObject GetPrefab()
    {
        return worldEntityPrefabs[Random.Range(0, worldEntityPrefabs.Length)];
    }


    public Vector3[] GenerateSpawnPositions(Vector2 mapBoundsX, Vector2 mapBoundsZ)
    {
        Vector3 spawnPos = Vector3.zero;
        switch (spawnStrategy)
        {
            case SpawnStrategy.Random:
                {
                    return GenerateRandomSpawnPositions(mapBoundsX, mapBoundsZ);
                }
            case SpawnStrategy.Noise:
                {
                    return GenerateNoiseSpawnPositions(mapBoundsX, mapBoundsZ);
                }
            case SpawnStrategy.RandomCustomArea:
                {
                    return GenerateCustomSpawnPositions();
                }
            case SpawnStrategy.VoronoiNoise:
                {
                    return GenerateVoronoiNoiseSpawnPositions(mapBoundsX, mapBoundsZ);
                }
            default:
                {
                    Debug.LogError("Invalid spawn strategy selected. Defaulting to Random.");
                    return new Vector3[0];
                }
        }
    }

    private Vector3[] GenerateRandomSpawnPositions(Vector2 mapBoundsX, Vector2 mapBoundsZ)
    {
        Vector3[] spawnPositions = new Vector3[numEntities];
        for (int i = 0; i < numEntities; i++)
        {
            float xPos = Random.Range(mapBoundsX.x, mapBoundsX.y);
            float zPos = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
            spawnPositions[i] = new Vector3(xPos, 0, zPos);
        }
        return spawnPositions;
    }

    private Vector3[] GenerateCustomSpawnPositions()
    {
        Vector3[] spawnPositions = new Vector3[numEntities];
        for (int i = 0; i < numEntities; i++)
        {
            float xPos = Random.Range(customSpawnAreaBoundsX.x, customSpawnAreaBoundsX.y);
            float zPos = Random.Range(customSpawnAreaBoundsZ.x, customSpawnAreaBoundsZ.y);
            spawnPositions[i] = new Vector3(xPos, 0, zPos);
        }
        return spawnPositions;
    }

    private Vector3[] GenerateNoiseSpawnPositions(Vector2 mapBoundsX, Vector2 mapBoundsZ)
    {
        Vector3[] spawnPositions = new Vector3[numEntities];
        for (int i = 0; i < numEntities; i++)
        {
            float noiseX = Mathf.PerlinNoise(Random.value, Random.value) * (mapBoundsX.y - mapBoundsX.x) + mapBoundsX.x;
            float noiseZ = Mathf.PerlinNoise(Random.value, Random.value) * (mapBoundsZ.y - mapBoundsZ.x) + mapBoundsZ.x;
            spawnPositions[i] = new Vector3(noiseX, 0, noiseZ);
        }
        return spawnPositions;
    }

    private Vector3[] GenerateVoronoiNoiseSpawnPositions(Vector2 mapBoundsX, Vector2 mapBoundsZ)
    {
        Vector3[] spawnPositions = new Vector3[numEntities];
        int numSites = Mathf.Max(1, voronoiSiteCount); // Allow 1 if needed

        // Generate Voronoi sites (random points in bounds)
        Vector2[] sites = new Vector2[numSites];
        for (int i = 0; i < numSites; i++)
        {
            float x = Random.Range(mapBoundsX.x, mapBoundsX.y);
            float z = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
            sites[i] = new Vector2(x, z);
        }

        // Assign each entity to the nearest Voronoi site
        for (int i = 0; i < numEntities; i++)
        {
            // Generate a random point within bounds
            float randX = Random.Range(mapBoundsX.x, mapBoundsX.y);
            float randZ = Random.Range(mapBoundsZ.x, mapBoundsZ.y);
            Vector2 randPoint = new Vector2(randX, randZ);

            // Find closest site
            Vector2 closestSite = sites[0];
            float closestDist = Vector2.Distance(randPoint, closestSite);
            for (int j = 1; j < sites.Length; j++)
            {
                float dist = Vector2.Distance(randPoint, sites[j]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestSite = sites[j];
                }
            }

            // Place entity near the site with some jitter/spread
            Vector2 offset = Random.insideUnitCircle * siteSpreadRadius;
            offset += Random.insideUnitCircle * siteJitter; // optional noise

            float finalX = Mathf.Clamp(closestSite.x + offset.x, mapBoundsX.x, mapBoundsX.y);
            float finalZ = Mathf.Clamp(closestSite.y + offset.y, mapBoundsZ.x, mapBoundsZ.y);

            spawnPositions[i] = new Vector3(finalX, 0f, finalZ);
        }

        return spawnPositions;
    }


}