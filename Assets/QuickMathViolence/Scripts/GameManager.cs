using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Win Condition")]
    public int targetValue;
    public int amountOfPairs;

    public bool randomTargetAndPairs = false;
    public int minValue = 5;
    public int maxValue = 10;
    public int minPairs = 5;
    public int maxPairs = 10;

    [Header("diegetic target")]
    public bool enableDiegeticTarget = false;
    public GameObject diegeticTarget;
    

    private int playerProgress;

    public UIManager uiManager;

    [Header("Blob Spawning")]
    public GameObject blobPrefab;
    public float range = 10.0f;

    [Header("Timer")]
    private LevelTimer levelTimer;
    

    private void Start()
    {
        levelTimer = GetComponent<LevelTimer>();
        // generate random values and pairs
        if (randomTargetAndPairs)
        {
            targetValue = Random.Range(minValue, maxValue);
            amountOfPairs = Random.Range(minPairs, maxPairs);
        }
        // initiate UI
        uiManager.SetTarget(targetValue);
        uiManager.progressGoal = amountOfPairs;
        uiManager.SetProgress(playerProgress);

        diegeticTarget.SetActive(enableDiegeticTarget);
        if (enableDiegeticTarget)
            diegeticTarget.GetComponent<DiegeticTarget>().Initiate(targetValue, amountOfPairs);

        SpawnBlobs();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetGame();
        }
    }

    private void SpawnBlobs()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        for (int i = 0; i < amountOfPairs; i++)
        {
            // spawn blob 1
            int vertexIndex1 = Random.Range(0, triangulation.vertices.Length);
            int blobValue1 = Random.Range(1, targetValue);

            CreateBlob(triangulation, vertexIndex1, blobValue1);

            // spawn blob 2
            int vertexIndex2 = Random.Range(0, triangulation.vertices.Length);
            int blobValue2 = targetValue - blobValue1;

            CreateBlob(triangulation, vertexIndex2, blobValue2);
        }
    }

    private void CreateBlob(NavMeshTriangulation triangulation, int vertexIndex, int value)
    {
        Vector3 point = Vector3.zero;
        while (point == Vector3.zero)
        {
            if (RandomPoint(transform.position, range, out point))
            {
                GameObject blob = Instantiate(blobPrefab, point, Quaternion.identity);
                if (blob.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler blobFamilyHandler))
                {
                    blobFamilyHandler.value = value;
                    blobFamilyHandler.targetValue = targetValue;
                    blobFamilyHandler.gameManager = this;
                    blobFamilyHandler.Initiate();
                }
            }
        }
    }

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }

    public void AddProgress()
    {
        playerProgress++;
        uiManager.SetProgress(playerProgress);
        if (enableDiegeticTarget)
            diegeticTarget.GetComponent<DiegeticTarget>().SetProgress(playerProgress);
        if (playerProgress >= amountOfPairs)
        {
            levelTimer.SetActive(false);
            uiManager.DisplayWin();
        }
    }

    private void ResetGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
        
}
