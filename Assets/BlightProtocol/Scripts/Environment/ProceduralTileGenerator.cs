using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ProceduralTileGenerator : MonoBehaviour
{
    public static ProceduralTileGenerator Instance;
    public GameObject mapMask;
    [Header("Map Settings")]
    public Color biomeColor;
    public Vector2 mapBoundsX;
    public Vector2 mapBoundsZ;
    public int mapWidth { get; private set; }
    public int mapDepth { get; private set; }
    public float tileSize = 1f;

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
        mapWidth = Mathf.RoundToInt((mapBoundsX.y - mapBoundsX.x) / tileSize);
        mapDepth = Mathf.RoundToInt((mapBoundsZ.y - mapBoundsZ.x) / tileSize);

        Vector3[] vertices = new Vector3[(mapWidth + 1) * (mapDepth + 1)];
        Vector3[] normals = new Vector3[vertices.Length];
        Vector2[] uv = new Vector2[vertices.Length];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[mapWidth * mapDepth * 6];

        for (int z = 0; z <= mapDepth; z++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                int index = z * (mapWidth + 1) + x;
                float xPos = mapBoundsX.x + x * tileSize;
                float zPos = mapBoundsZ.x + z * tileSize;
                float yPos = 0f; 

                vertices[index] = new Vector3(xPos, yPos, zPos);
                normals[index] = Vector3.up;
                uv[index] = new Vector2((float)x, (float)z);
                colors[index] = biomeColor;
            }
        }


        int triIndex = 0;
        for (int z = 0; z < mapDepth; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                int vertIndex = z * (mapWidth + 1) + x;

                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + mapWidth + 1;
                triangles[triIndex++] = vertIndex + mapWidth + 2;

                triangles[triIndex++] = vertIndex;
                triangles[triIndex++] = vertIndex + mapWidth + 2;
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

        if (mapMask.GetComponent<MeshFilter>() != null)
        {
            // Clone mesh and add new UVs for the Mask
            Mesh maskMesh = new Mesh
            {
                vertices = mesh.vertices,
                normals = mesh.normals,
                triangles = mesh.triangles,
                colors = mesh.colors
            };

            // Create new UVs that span the entire mesh
            Vector2[] maskUVs = new Vector2[mesh.vertices.Length];
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 vertex = mesh.vertices[i];
                maskUVs[i] = new Vector2(
                    Mathf.InverseLerp(mapBoundsX.x, mapBoundsX.y, vertex.x),
                    Mathf.InverseLerp(mapBoundsZ.x, mapBoundsZ.y, vertex.z)
                );
            }

            // Assign new UVs
            maskMesh.uv = maskUVs;

            // Assign the new mesh to the mapMask
            mapMask.GetComponent<MeshFilter>().mesh = maskMesh;
            mapMask.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        }


        GetComponent<MeshFilter>().mesh.RecalculateNormals();
    }
}
