using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    public GameObject RegularEnemyPrefab;
    public GameObject FastEnemyPrefab;
    public bool AutoSpawnOverride = false;
    public float spawnRate = 1f;
    public bool randomizeSpawn = false;
    public Vector2 randomSpawnRateRange = new Vector2(1f, 5f);
    public float spawnRadius = 5f; //Visualize with gizmo
    public float enemyScale = 1f;
    private float m_SpawnTimer = 0f;
    private int m_EnemyCount = 0;
    private SpawnState m_SpawnState = SpawnState.FINISHED;

    void Start()
    {
        if (randomizeSpawn)
        {
            spawnRate = Random.Range(randomSpawnRateRange.x, randomSpawnRateRange.y);
        }

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
                if (m_SpawnTimer >= spawnRate)
                {
                    m_SpawnTimer = 0f;
                    if (randomizeSpawn)
                    {
                        spawnRate = Random.Range(randomSpawnRateRange.x, randomSpawnRateRange.y);
                    }

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
        spawnPosition.y = 0.5f;

        //Randomly choose between Regular and Fast enemy
        bool random = Random.value > 0.5f;
        GameObject enemyPrefab = random ? RegularEnemyPrefab : FastEnemyPrefab;
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.localScale = new Vector3(enemyScale, enemyScale, enemyScale);
        m_EnemyCount++;
        enemy.GetComponentInChildren<EnemyDamageHandler>().enemyDestroyed.AddListener(() => { m_EnemyCount--; });
    }

    public void StartWave(int waveNumber)
    {
        m_SpawnState = SpawnState.SPAWNING;

        //tighten randomSpawnRateRange for each wave and lower it
        randomSpawnRateRange.x = randomSpawnRateRange.x - (waveNumber * 0.1f);
        randomSpawnRateRange.y = randomSpawnRateRange.y - (waveNumber * 0.4f);
    }

    public void StopWave()
    {
        m_SpawnState = SpawnState.WAITING;
    }
}
