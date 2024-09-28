using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float spawnRate = 1f;
    public bool randomizeSpawn = false;
    public Vector2 randomSpawnRateRange = new Vector2(1f, 5f);
    public float spawnRadius = 5f; //Visualize with gizmo
    public float enemyScale = 1f;
    private float m_SpawnTimer = 0f;

    void Start()
    {
        if (randomizeSpawn)
        {
            spawnRate = Random.Range(randomSpawnRateRange.x, randomSpawnRateRange.y);
        }
    }

    void Update()
    {
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
        GameObject enemy = Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);
        enemy.transform.localScale = new Vector3(enemyScale, enemyScale, enemyScale);
    }
}
