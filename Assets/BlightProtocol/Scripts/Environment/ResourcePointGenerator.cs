using UnityEngine;
using UnityEngine.AI;

public class ResourcePointGenerator : MonoBehaviour
{
    public static ResourcePointGenerator instance;
    [SerializeField] private GameObject mapGenerator;
    public GameObject resourcePointPrefab;
    public int resourcePointCount = 10;
    public Vector2 regionSize = new Vector2(300, 300);

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        for (int i = 0; i < resourcePointCount; i++)
        {
            // Randomly generate a position within the specified bounds
            float xPos = Random.Range(-regionSize.x, regionSize.x);
            float zPos = Random.Range(0, regionSize.y);
            Vector3 randomPosition = new Vector3(xPos, 0, zPos);
            
            // Check if the position is on the NavMesh and place an obstacle if it is
            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                Instantiate(resourcePointPrefab, randomPosition, Quaternion.identity);
            }
        }
    }
}