using UnityEngine;

public class ResourcePointGenerator : MonoBehaviour
{
    public static ResourcePointGenerator instance;
    [SerializeField] private GameObject mapGenerator;
    public GameObject resourcePointPrefab;
    public int resourcePointCount = 10;

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
            Vector3 randomPosition = Vector3.zero; //POSITION GENERIEREN WELCHE RANDOM ON NAVMESH MESH IST, siehe obstacleplacer script 
            randomPosition.y = 0;
            Instantiate(resourcePointPrefab, randomPosition, Quaternion.identity);
        }
    }
}