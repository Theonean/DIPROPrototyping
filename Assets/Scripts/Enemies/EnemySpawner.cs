using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    enum SpawnState
    {
        SPAWNING, //Spawns enemies
        WAITING, //Waits for enemies to die
        FINISHED //No more enemies to spawn and idle
    }
    public UnityEvent AllEnemiesDead = new UnityEvent();
    public GameObject enemyPrefab;
    public bool AutoSpawnOverride = false;

    //How many enemies per second this spawner generates
    float spawnRate = 0.1f;
    float spawnEnemyInNSeconds = 0f;
    public float spawnRadius = 5f; //Visualize with gizmo
    public float enemyScale = 1f;
    private float m_SpawnTimer = 0f;
    private int m_EnemyCount = 0;
    private SpawnState m_SpawnState = SpawnState.FINISHED;

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
                if (m_EnemyCount == 0)
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

    void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        spawnPosition.y = -0.12f;

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        m_EnemyCount++;
        enemy.GetComponentInChildren<EnemyDamageHandler>().enemyDestroyed.AddListener(() => { m_EnemyCount--; });
    }

    IEnumerator SpawnEnemyDesynced()
    {
        yield return new WaitForSeconds(Random.Range(0f, 1.5f));
        SpawnEnemy();
    }

    public void StartWave(int waveNumber)
    {
        //Increase enemies spawned per second by 0.2 per wave
        spawnRate = 0.1f + waveNumber * 0.05f
            + Random.Range(-0.15f, 0.15f); //Slightly randome the spawn rate so not all animations are synced across spawners;
        spawnEnemyInNSeconds = 1f / spawnRate;

        //Debug.Log("Spawner creating " + spawnRate + " enemies per second at an intervall of" + spawnEnemyInNSeconds);

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
}
