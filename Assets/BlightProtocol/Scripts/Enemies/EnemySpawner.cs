using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum SpawnState
{
    SPAWNING, //Spawns enemies
    WAITING, //Waits for enemies to die
    FINISHED //No more enemies to spawn and idle
}

public enum SpawnType
{
    ALL_RANDOM, //Picks a new enemy type each time
    ALL_SINGLE,
    SINGLE,
    LIST
}

public class EnemySpawner : MonoBehaviour
{
    [HideInInspector] public UnityEvent AllEnemiesDead = new UnityEvent();
    [HideInInspector] public UnityEvent spawnedEnemy = new UnityEvent();

    [SerializeField] public LayerMask layerToHit;
    [SerializeField] private const SpawnType enemyTypesToSpawn = SpawnType.ALL_RANDOM;
    public SOEnemySpawnPattern[] enemySpawnPatterns = new SOEnemySpawnPattern[0];
    public bool FiresOnce = false;
    private bool isSpawningEnemies = false;
    public bool AutoSpawnOverride = false;
    public bool overrideIsGroundUnderSpawner = false;

    //How many enemies per second this spawner generates
    float spawnRate = 0.1f;
    float spawnEnemyInNSeconds = 0f;
    public float spawnRadius = 5f; //Visualize with gizmo
    public float enemyScale = 1f;
    private float m_SpawnTimer = 0f;
    private int m_EnemyCount = 0;
    private SpawnState m_SpawnState = SpawnState.FINISHED;
    public List<ACEnemyMovementBehaviour> spawnedEnemies = new List<ACEnemyMovementBehaviour>();

    void Start()
    {
        spawnEnemyInNSeconds = 1f / spawnRate;

        if (AutoSpawnOverride)
        {
            StartWave(1);
        }
    }

    void Update()
    {
        switch (m_SpawnState)
        {
            case SpawnState.SPAWNING:
                m_SpawnTimer += Time.deltaTime;
                if (m_SpawnTimer >= spawnEnemyInNSeconds)
                {
                    m_SpawnTimer = 0f;
                    SpawnEnemy();
                }
                break;
            case SpawnState.WAITING:
                if (m_EnemyCount <= 0)
                {
                    m_SpawnState = SpawnState.FINISHED;
                    AllEnemiesDead.Invoke();
                }
                break;
            //Idle when finished
            case SpawnState.FINISHED:
                break;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    public void SpawnEnemy(bool overrideCollisionCheck = false)
    {
        // Early exit if FiresOnce and already spawning
        if (FiresOnce && isSpawningEnemies) return;

        isSpawningEnemies = true;

        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = 0f;

        GameObject enemyPrefab;

        int difficultyRegion = DifficultyManager.Instance.difficultyLevel;

        SOEnemySpawnPattern[] elligiblePatterns = enemySpawnPatterns.Where(pattern => pattern.minimumdifficultyRegion <= difficultyRegion).ToArray();
        SOEnemySpawnPattern spawnPattern = elligiblePatterns[Random.Range(0, elligiblePatterns.Length)];
        if (spawnPattern == null)
        {
            Debug.LogError("Could not find valid spawn pattern for spawner");
            return;
        }

        foreach (EnemySpawnPosition enemyPos in spawnPattern.spawnPositions)
        {
            if (enemyPos.enemyType == EnemyType.NONE)
                continue;

            enemyPrefab = null;
            switch (enemyPos.enemyType)
            {
                case EnemyType.REGULAR:
                    enemyPrefab = WaveManager.Instance.regularEnemyPrefab;
                    break;
                case EnemyType.CHARGER:
                    enemyPrefab = WaveManager.Instance.chargerEnemyPrefab;
                    break;
                case EnemyType.CRABTANK:
                    enemyPrefab = WaveManager.Instance.tankEnemyPrefab;
                    break;
                case EnemyType.ALL:
                    enemyPrefab = WaveManager.Instance.GetRandomEnemyPrefab();
                    break;
            }

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition + enemyPos.position * spawnPattern.spacing, Quaternion.identity);
            spawnedEnemies.Add(enemy.GetComponent<ACEnemyMovementBehaviour>());
            m_EnemyCount++;
            spawnedEnemy.Invoke();

            if (FiresOnce)
            {
                continue; //Don't subscribe if only fires once -> don't want any memory leaks due to destroyed listener
            }

            enemy.GetComponentInChildren<EnemyDamageHandler>().enemyDestroyed.AddListener(() => { m_EnemyCount--; });
        }

        if (FiresOnce)
        {
            Destroy(this);
        }
        else
        {
            // Only reset if not FiresOnce
            isSpawningEnemies = false;
        }
    }

    IEnumerator SpawnEnemyDesynced()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));
        SpawnEnemy();
    }

    public void SetSpawnRate(int difficultyLevel)
    {
        //Increase enemies spawned per second by 0.2 per wave
        spawnRate = 0.1f + difficultyLevel * 0.05f
            + Random.Range(-0.01f, 0.01f); //Slightly randome the spawn rate so not all animations are synced across spawners;
        spawnEnemyInNSeconds = 1f / spawnRate;
        Logger.Log("Spawner creating " + spawnRate + " enemies per second at an interval of " + spawnEnemyInNSeconds, LogLevel.INFO, LogType.WAVEMANAGEMENT);
    }

    public void StartWave()
    {
        StartWave(DifficultyManager.Instance.difficultyLevel);
    }

    public void StartWave(int waveNumber)
    {
        SetSpawnRate(waveNumber);

        m_SpawnState = SpawnState.SPAWNING;

        int initialSpawnCount = Mathf.CeilToInt(Mathf.Sqrt(waveNumber));

        //Spawn enemies immediately at start of wave to create some intial pressure on player
        for (int i = 0; i < initialSpawnCount; i++)
        {
            StartCoroutine(SpawnEnemyDesynced());
        }
    }

    public void StopWave()
    {
        m_SpawnState = SpawnState.WAITING;
    }

    public bool IsGroundUnderSpawnerApproximate()
    {
        if (overrideIsGroundUnderSpawner)
            return true;

        float startingHeight = 150f;
        Vector3 origin = transform.position + Vector3.up * startingHeight;
        float maxDistance = startingHeight * 2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDistance, layerToHit))
        {
            // If the first hit was tagged "Ground", treat it as no hit
            if (hit.collider.CompareTag("Ground"))
                return true;

            return false;
        }

        return true;
    }

}
