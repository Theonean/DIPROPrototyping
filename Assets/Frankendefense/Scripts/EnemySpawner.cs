using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float SpawnRate = 1f;
    public float SpawnRadius = 5f; //Visualize with gizmo
    private float m_SpawnTimer = 0f;

    void Update()
    {
        m_SpawnTimer += Time.deltaTime;
        if (m_SpawnTimer >= SpawnRate)
        {
            m_SpawnTimer = 0f;
            SpawnEnemy();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, SpawnRadius);
    }

    void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position + Random.insideUnitSphere * SpawnRadius;
        spawnPosition.y = 0;
        Instantiate(EnemyPrefab, spawnPosition, Quaternion.identity);
    }
}
