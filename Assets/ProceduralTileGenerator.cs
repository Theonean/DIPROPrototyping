using System.Collections;
using System.Collections.Generic;
using System.Security;
using UnityEngine;
using UnityEngine.ProBuilder;

[System.Serializable]
public class MapGenerationData
{
    public Color color;
    public float positionX;

}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralTileGenerator : MonoBehaviour
{
    public MapGenerationData[] gradientColorsAtPositions;
    public int sizeX = 10;
    public int sizeZ = 10;
    public float tileSize = 1f;

    public void BuildMesh()
    {
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
                (startGradient, endGradient) = GetColorGradientAtPosition(x);

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
    public void AddColourRegion(MapGenerationData mapGenerationData)
    {
        // Add the new color to the gradientColorsAtPositions array
        List<MapGenerationData> gradientColorsList = new List<MapGenerationData>(gradientColorsAtPositions);
        gradientColorsList.Add(mapGenerationData);
        gradientColorsAtPositions = gradientColorsList.ToArray();

        // Sort the gradientColorsAtPositions array by positionX
        System.Array.Sort(gradientColorsAtPositions, (x, y) => x.positionX.CompareTo(y.positionX));

        // Rebuild the mesh with the updated color gradient
        BuildMesh();
    }

    // Iterate over color gradient and return start and end colors at a given position
    private (MapGenerationData, MapGenerationData) GetColorGradientAtPosition(float xPos)
    {
        // Check if the gradient is empty or the position is outside the range
        if (gradientColorsAtPositions.Length == 0 || xPos <= gradientColorsAtPositions[0].positionX)
        {
            return (gradientColorsAtPositions[0], gradientColorsAtPositions[0]);
        }
        if (xPos >= gradientColorsAtPositions[gradientColorsAtPositions.Length - 1].positionX)
        {
            int lastIndex = gradientColorsAtPositions.Length - 1;
            return (gradientColorsAtPositions[lastIndex], gradientColorsAtPositions[lastIndex]);
        }

        // Iterate through positions to find the range containing xPos
        for (int i = 0; i < gradientColorsAtPositions.Length - 1; i++)
        {
            if (xPos >= gradientColorsAtPositions[i].positionX && xPos <= gradientColorsAtPositions[i + 1].positionX)
            {
                return (gradientColorsAtPositions[i], gradientColorsAtPositions[i + 1]);
            }
        }

        // Default return if no match found
        return (null, null);
    }


}
