using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralTileGenerator : MonoBehaviour
{
    public static ProceduralTileGenerator Instance;
    public Color[] gradientColors;
    public int wavesPerColourRegion;
    public int sizeX = 10;
    public int sizeZ = 10;
    public float tileSize = 1f;
    public int initialPathPositions = 20;
    public int pathAngle = 35;
    public float minTravelTime = 20f;
    public float maxTravelTime = 30f;
    public float moveSpeed = 4f;

    private Vector3[] pathPositions;

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

        BuildMesh();
    }

    public void BuildMesh()
    {
        pathPositions = GeneratePath(Vector3.zero);

        sizeX = Mathf.CeilToInt(GetPathDistance() / tileSize);
        Debug.Log($"Tilemap Sizes: sizeX: {sizeX}, sizeZ: {sizeZ}");

        Vector3[] vertices = new Vector3[(sizeX + 1) * (sizeZ + 1)];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[sizeX * sizeZ * 6];

        for (int z = 0; z <= sizeZ; z++)
        {
            for (int x = 0; x <= sizeX; x++)
            {
                int index = z * (sizeX + 1) + x;
                float xPos = x * tileSize;
                float zPos = z * tileSize;

                vertices[index] = new Vector3(xPos, 0, zPos);
                normals[index] = Vector3.up;
                uv[index] = new Vector2((float)x, (float)z);

                // Calculate the color gradient based on the x position
                colors[index] = GetColorForPosition(xPos);
            }
        }

        int triIndex = 0;
        for (int z = 0; z < sizeZ; z++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                int vertIndex = z * (sizeX + 1) + x;

                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + sizeX + 1;
                triangles[triIndex++] = vertIndex + sizeX + 2;

                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + sizeX + 2;
                triangles[triIndex++] = vertIndex + 1;
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            normals = normals,
            uv = uv,
            colors = colors,
            triangles = triangles
        };

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void ExtendMesh(int additionalRows)
    {
        sizeZ += additionalRows;
        BuildMesh();
    }
    private Color GetColorForPosition(float xPos)
    {
        float pathDistance = GetPathDistance();
        if (pathDistance <= 0 || gradientColors.Length == 0 || wavesPerColourRegion <= 0)
            return Color.white;

        // Determine the total number of color regions based on the path distance and region size
        float totalRegions = pathDistance / (wavesPerColourRegion * tileSize);
        float normalizedPosition = Mathf.Clamp01(xPos / pathDistance);

        // Calculate current region index within total regions
        int regionIndex = Mathf.FloorToInt(normalizedPosition * totalRegions);

        // Map the region index to the gradientColors array by cycling through colors
        int startColorIndex = regionIndex % gradientColors.Length;
        int endColorIndex = (startColorIndex + 1) % gradientColors.Length;

        // Interpolate between colors based on position within the region
        float t = (normalizedPosition * totalRegions) - regionIndex;
        return Color.Lerp(gradientColors[startColorIndex], gradientColors[endColorIndex], t);
    }


    public Vector3[] GeneratePath(Vector3 startPosition)
    {
        pathPositions = new Vector3[initialPathPositions];
        pathPositions[0] = startPosition;

        // Define the x-range limits, centered on the object's position, reduced by half for safety
        Vector2 minMax_X = new Vector2(-250, 250);

        Debug.Log($"MinMax_X: {minMax_X}");

        for (int i = 1; i < initialPathPositions; i++)
        {
            // Generate a new path direction based on Vector3.forward with slight random angle variation
            float angleVariance = Random.Range(-pathAngle, pathAngle);
            Vector3 pathDirection = Quaternion.Euler(0, angleVariance, 0) * Vector3.forward;

            // Calculate the new position based on the previous point and the adjusted path direction
            Vector3 newPosition = CalculatePathPosition(pathPositions[i - 1], pathDirection);

            // Ensure the new position's x-value stays within bounds
            newPosition.x = Mathf.Clamp(newPosition.x, minMax_X.x, minMax_X.y);

            // Update the path position
            pathPositions[i] = newPosition;
        }

        return pathPositions;
    }

    private void OnDrawGizmos()
    {
        if (pathPositions == null) return;

        for (int i = 0; i < pathPositions.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(pathPositions[i], 5f);
        }
    }



    public Vector3[] GetPath()
    {
        return pathPositions;
    }

    private Vector3 CalculatePathPosition(Vector3 startPosition, Vector3 direction)
    {
        float randomDistance = Random.Range(minTravelTime * moveSpeed, maxTravelTime * moveSpeed);
        return startPosition + (direction * randomDistance);
    }

    public float GetPathDistance()
    {
        if (pathPositions == null || pathPositions.Length < 2) return 0f;
        return Vector3.Distance(pathPositions[0], pathPositions[^1]);
    }
}
