using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using UnityEngine;
using UnityEngine.ProBuilder;

[System.Serializable]
public class MapGenerationData
{
    public Color color;
    public float positionX;

    public MapGenerationData(float xPos)
    {
        color = Color.white;
        positionX = xPos;
    }

    override public string ToString()
    {
        return $"Color: {color}, PositionX: {positionX}";
    }

}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralTileGenerator : MonoBehaviour
{
    //Thoughts on how to implement map generation:
    /*
        Generate a Path List of Type Vector2 on the controlzonemanager
        This List gets generated at the beginning of the game for the first n waves
        from this path array the tilemap gets generated

    */

    //Rework script to work with this
    public Color[] gradientColors;
    public int wavesPerColourRegion;
    public int sizeX = 10;
    public int sizeZ = 10;
    public float tileSize = 1f;

    private void Start()
    {
        BuildMesh();
    }

    public void BuildMesh()
    {
        //Size x now gets calculated with the number of paths in the control zone 
        //the total length (distance first to last path point) divided by tilesize should be the new x
        sizeX = Mathf.CeilToInt(ControlZoneManager.Instance.getPathDistance() / tileSize);
        Debug.Log($"Tilemap Sizes: sizeX: {sizeX}, sizeZ: {sizeZ}");
        int numTiles = sizeX * sizeZ;
        int numTris = numTiles * 2;

        int vSize_X = sizeX + 1;
        int vSize_Z = sizeZ + 1;
        int numVerts = vSize_X * vSize_Z;

        Vector3[] vertices = new Vector3[numVerts];
        Vector3[] normals = new Vector3[numVerts];
        Vector2[] uv = new Vector2[numVerts];
        Color[] colors = new Color[numVerts];

        MapGenerationData startGradient;
        MapGenerationData endGradient;


        //Initialize vertices information
        int x, z;
        for (z = 0; z <= sizeZ; z++) // Note <= to account for the last row
        {
            for (x = 0; x <= sizeX; x++) // Note <= to account for the last column
            {
                float xPos = x * tileSize;
                float zPos = z * tileSize;

                vertices[z * vSize_X + x] = new Vector3(xPos, 0, zPos);
                normals[z * vSize_X + x] = Vector3.up;
                uv[z * vSize_X + x] = new Vector2((float)x, (float)z); // UV normalized by size

                // Get colors for the current x position
                (startGradient, endGradient) = GetColorGradientAtPosition(xPos);

                // Normalize xPos between startGradient.positionX and endGradient.positionX
                float t = Mathf.InverseLerp(startGradient.positionX, endGradient.positionX, xPos);

                // Interpolate the color using the normalized value of t
                colors[z * vSize_X + x] = Color.Lerp(startGradient.color, endGradient.color, t);
            }
        }

        int[] triangles = new int[numTris * 3];
        for (z = 0; z < sizeZ; z++)
        {
            //Generate two triangles for each square from left to right
            for (x = 0; x < sizeX; x++)
            {
                int squareIndex = z * sizeX + x;
                int triOffset = squareIndex * 6;
                int vertOffset = z * vSize_X + x;

                //Triangle A
                triangles[triOffset] = vertOffset;
                triangles[triOffset + 1] = vertOffset + vSize_X;
                triangles[triOffset + 2] = vertOffset + vSize_X + 1;

                //Triangle B
                triangles[triOffset + 3] = vertOffset;
                triangles[triOffset + 4] = vertOffset + vSize_X + 1;
                triangles[triOffset + 5] = vertOffset + 1;
            }
        }

        // Update the mesh with the new data
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;

        // Assign the updated mesh
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void ExtendMesh(int additionalRows)
    {
        // Update sizeZ to accommodate the new rows
        sizeZ += additionalRows;
        BuildMesh();
    }
    // Iterate over color gradient and return start and end colors at a given position
    private (MapGenerationData, MapGenerationData) GetColorGradientAtPosition(float xPos)
    {
        ControlZoneManager ctrlZone = ControlZoneManager.Instance;

        // Check if the pathPositions array is empty or if xPos is out of range
        if (ctrlZone.pathPositions.Length == 0 || xPos < ctrlZone.getDistanceAlongPathFromPoint(0))
        {
            Debug.LogError("Invalid pathPositions array or pos is out of range.");
            Debug.Log("ctrlZone.pathPositions.Length: " + ctrlZone.pathPositions.Length + " xPos: " + xPos + " pathPositions[0].z: " + ctrlZone.getDistanceAlongPathFromPoint(0));
            // Return the first color if xPos is before the first path position
            return (new MapGenerationData(ctrlZone.getDistanceAlongPathFromPoint(0)) { color = gradientColors[0] },
                    new MapGenerationData(ctrlZone.getDistanceAlongPathFromPoint(0)) { color = gradientColors[0] });
        }

        if (xPos >= ctrlZone.getDistanceAlongPathFromPoint(ctrlZone.pathPositions.Length - 1))
        {
            Debug.LogError("Position is beyond the last path position.");
            Debug.Log("xPos: " + xPos + " lastPathPosition.z: " + ctrlZone.getDistanceAlongPathFromPoint(ctrlZone.pathPositions.Length - 1));
            // Return the last color if xPos is beyond the last path position
            return (new MapGenerationData(ctrlZone.getDistanceAlongPathFromPoint(ctrlZone.pathPositions.Length - 1)) { color = gradientColors[^1] },
                    new MapGenerationData(ctrlZone.getDistanceAlongPathFromPoint(ctrlZone.pathPositions.Length - 1)) { color = gradientColors[^1] });
        }

        // Iterate through path positions to find the range containing xPos, but in chunks of `wavesPerColourRegion`
        for (int i = 0; i < ctrlZone.pathPositions.Length - 1; i += wavesPerColourRegion)
        {
            int regionEndIndex = Mathf.Min(i - i % wavesPerColourRegion + wavesPerColourRegion, ctrlZone.pathPositions.Length - 1); // End of current color region
            float regionStartDistance = ctrlZone.getDistanceAlongPathFromPoint(i);
            float regionEndDistance = ctrlZone.getDistanceAlongPathFromPoint(regionEndIndex);

            Debug.Log("Comparing xPos: " + xPos + " with regionStartDistance: " + regionStartDistance + " and regionEndDistance: " + regionEndDistance);

            if (xPos >= regionStartDistance && xPos <= regionEndDistance)
            {
                // Determine color index based on the region's starting index in terms of `wavesPerColourRegion`
                int colorIndex = (i / wavesPerColourRegion) % gradientColors.Length;

                // Create start and end gradient data for this color region
                MapGenerationData startGradientData = new MapGenerationData(regionStartDistance) { color = gradientColors[colorIndex] };
                MapGenerationData endGradientData = new MapGenerationData(regionEndDistance) { color = gradientColors[(colorIndex + 1) % gradientColors.Length] };

                Debug.Log($"Start Gradient: {startGradientData}, End Gradient: {endGradientData}");

                return (startGradientData, endGradientData);
            }
        }

        // Default return if no match found (though ideally should never reach here with correct input)
        Debug.LogWarning("No matching gradient range found for xPos; returning default color.");
        return (new MapGenerationData(xPos) { color = Color.white },
                new MapGenerationData(xPos) { color = Color.white });
    }
}
