using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ObstaclePlacer : MonoBehaviour
{
    public GameObject[] obstaclePatternPrefabs = new GameObject[0];
    public int obstaclesPerRegion = 75;

    private GameObject[] m_obstacles0;
    private GameObject[] m_obstacles1;
    private GameObject[] m_obstacles2;
    public Vector2 regionWidth = new Vector2(-300, 300); // Define x-axis range
    public float regionOvershoot = 50f;
    public float regionUndershoot = 50f;

    private void Start()
    {
        ControlZoneManager.Instance.changedState.AddListener(OnZoneStateChanged);
        StartCoroutine(GenerateInitialObstacles());
    }

    private void OnZoneStateChanged(ZoneState state)
    {
        if (state == ZoneState.START_HARVESTING && ControlZoneManager.Instance.pathPositionsIndex > 2) //Harveszer just arrived at new position
        {
            Debug.Log($"ObstaclePlacer: OnZoneStateChanged: {state}" + " " + ControlZoneManager.Instance.pathPositionsIndex);
            if (ControlZoneManager.Instance.pathPositionsIndex % 3 == 0)
                MoveObstaclesToRegion(ControlZoneManager.Instance.pathPositionsIndex, m_obstacles0);
            else if (ControlZoneManager.Instance.pathPositionsIndex % 3 == 1)
                MoveObstaclesToRegion(ControlZoneManager.Instance.pathPositionsIndex, m_obstacles1);
            else if (ControlZoneManager.Instance.pathPositionsIndex % 3 == 2)
                MoveObstaclesToRegion(ControlZoneManager.Instance.pathPositionsIndex, m_obstacles2);
        }
    }

    // Generates obstacles on the field within the specified bounds and places them on the NavMesh
    private IEnumerator GenerateInitialObstacles()
    {
        Vector3[] pathPositions = ProceduralTileGenerator.Instance.GetPath();
        m_obstacles0 = new GameObject[obstaclesPerRegion];
        m_obstacles1 = new GameObject[obstaclesPerRegion];
        m_obstacles2 = new GameObject[obstaclesPerRegion];

        //Create Obstacles for first and second path index
        for (int pathIndex = 0; pathIndex <= 2; pathIndex++)
        {
            // Define region bounds based on path positions
            Vector2 regionHeight = new Vector2(pathPositions[pathIndex].z - regionUndershoot, pathPositions[pathIndex + 1].z + regionOvershoot);

            for (int i = 0; i < obstaclesPerRegion; i++)
            {
                // Randomly generate a position within the specified bounds
                float xPos = Random.Range(regionWidth.x, regionWidth.y);
                float zPos = Random.Range(regionHeight.x, regionHeight.y);
                Vector3 randomPosition = new Vector3(xPos, 0, zPos);

                // Check if the position is on the NavMesh and place an obstacle if it is
                if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
                {
                    // Choose a random obstacle pattern prefab
                    GameObject obstaclePrefab = obstaclePatternPrefabs[Random.Range(0, obstaclePatternPrefabs.Length)];

                    // Instantiate the obstacle at the NavMesh position
                    GameObject gO = Instantiate(obstaclePrefab, hit.position, Quaternion.identity);
                    gO.transform.parent = transform;

                    gO.GetComponent<ObstaclePattern>().SetMeshColoursToRegion();

                    // Add the obstacle to the appropriate array
                    if (pathIndex == 0)
                        m_obstacles0[i] = gO;
                    else if (pathIndex == 1)
                        m_obstacles1[i] = gO;
                    else if (pathIndex == 2)
                        m_obstacles2[i] = gO;
                }

                //Wait a short amount of time before creating next obstacle
                yield return null;
            }
        }
    }

    private void MoveObstaclesToRegion(int startPathIndex, GameObject[] obstacles)
    {
        Vector3[] pathPositions = ProceduralTileGenerator.Instance.GetPath();

        // Define region bounds based on path positions
        Vector2 regionHeight = new Vector2(pathPositions[startPathIndex].z - regionUndershoot, pathPositions[startPathIndex + 1].z + regionOvershoot);

        for (int i = 0; i < obstacles.Length; i++)
        {
            // Randomly generate a position within the specified bounds
            float xPos = Random.Range(regionWidth.x, regionWidth.y);
            float zPos = Random.Range(regionHeight.x, regionHeight.y);
            Vector3 randomPosition = new Vector3(xPos, 0, zPos);
            GameObject obstacle = obstacles[i];

            // Check if the position is on the NavMesh and place an obstacle if it is
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                if (obstacle != null)
                {
                    obstacle.GetComponent<ObstaclePattern>().MovePattern(hit.position);
                }
            }
        }
        Debug.Log("Moved Obstacles to new region " + startPathIndex);
    }
}
