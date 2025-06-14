using UnityEngine;
using System;
using System.Collections.Generic;

public class DistortionZone : MonoBehaviour
{
    private enum DistortionZoneState
    {
        IDLE,
        HARVESTER_OUTERLAYER,
        HARVESTER_INNERLAYER
    }

    private float enemyMinDistanceToTargetBeforeRecalculation = 10f;
    private int enemiesToSpawnPerDifficultyRegion = 30;
    private List<ACEnemyMovementBehaviour> enemyBehaviours = new List<ACEnemyMovementBehaviour>();
    private const float timeToRecalculateDestionations = 1f;
    private float recalculateDestionationsTimer = 0f;
    private DistortionZoneState distortionZoneState = DistortionZoneState.IDLE;

    [Header("Enemies walk toward center settings")]
    [SerializeField] private SphereCollider innerZoneCollider;
    [SerializeField] private SphereCollider outerZoneCollider;

    [SerializeField] private float outerLayerEnemySpawnInterval = 5f;
    [SerializeField] private int maxOuterEnemies = 10;
    [SerializeField] private float centerDespawnRadius = 5f;

    private float outerEnemySpawnTimer = 0f;
    private List<GameObject> outerLayerEnemies = new List<GameObject>();

    private void Start()
    {
        int enemiesToSpawn = enemiesToSpawnPerDifficultyRegion; //TODO add GetDifficultyRegion(Vector3 fromPosition) to difficulty manager so that distortion zones scale

        for (int i = 0; i < enemiesToSpawnPerDifficultyRegion; i++)
        {
            Vector3 enemySpawnPosition = GetRandomPositionInZone();

            GameObject randomEnemy = WaveManager.Instance.GetRandomEnemyPrefab();
            GameObject spawnedEnemy = Instantiate(randomEnemy, enemySpawnPosition, Quaternion.identity);

            ACEnemyMovementBehaviour enemyMovement = spawnedEnemy.GetComponent<ACEnemyMovementBehaviour>();
            enemyMovement.SetMovementType(EnemyMovementType.REMOTECONTROLLED);

            enemyBehaviours.Add(enemyMovement);
        }
    }

    private void Update()
    {
        if (distortionZoneState == DistortionZoneState.IDLE) return;

        recalculateDestionationsTimer -= Time.deltaTime;
        if(recalculateDestionationsTimer < 0)
        {
            recalculateDestionationsTimer = timeToRecalculateDestionations;

            List<ACEnemyMovementBehaviour> behavioursToRemove = new List<ACEnemyMovementBehaviour>();
            foreach(ACEnemyMovementBehaviour enemyBehaviour in enemyBehaviours)
            {
                if(!enemyBehaviour)
                {
                    behavioursToRemove.Add(enemyBehaviour);
                    continue;
                }

                Vector3 targetPosition = enemyBehaviour.navMeshAgent.pathEndPosition;
                if(Vector3.Distance(enemyBehaviour.transform.position, targetPosition) < enemyMinDistanceToTargetBeforeRecalculation)
                {
                    enemyBehaviour.SetDestination(GetRandomPositionInZone());
                }
            }

            foreach(ACEnemyMovementBehaviour behaviourToRemove in behavioursToRemove)
            {
                enemyBehaviours.Remove(behaviourToRemove);
            }
        }

        if (distortionZoneState == DistortionZoneState.HARVESTER_OUTERLAYER)
        {
            HandleOuterLayerSpawning();
        }

        HandleOuterEnemyDespawn();
    }

    public void HarvesterEnteredInnerLayer()
    {
        if (distortionZoneState == DistortionZoneState.HARVESTER_INNERLAYER) return;
        distortionZoneState = DistortionZoneState.HARVESTER_INNERLAYER;

        foreach (ACEnemyMovementBehaviour enemyBehaviour in enemyBehaviours)
        {
            enemyBehaviour.SetMovementType(EnemyMovementType.CUSTOM);
        }
    }

    public void HarvesterEnteredOuterLayer()
    {
        distortionZoneState = DistortionZoneState.HARVESTER_OUTERLAYER;
    }

    public void HarvesterExited()
    {
        distortionZoneState = DistortionZoneState.IDLE;
    }

    private Vector3 GetRandomPositionInZone()
    {
        Vector3 enemySpawnPosition = transform.position + UnityEngine.Random.insideUnitSphere * innerZoneCollider.radius;
        enemySpawnPosition.y = 0f;

        if(Vector3.Distance(transform.position, enemySpawnPosition) < centerDespawnRadius)
        {
            enemySpawnPosition *= centerDespawnRadius;
        }

        return enemySpawnPosition;
    }

    private void HandleOuterLayerSpawning()
    {
        outerEnemySpawnTimer -= Time.deltaTime;

        if (outerEnemySpawnTimer <= 0f && outerLayerEnemies.Count < maxOuterEnemies)
        {
            outerEnemySpawnTimer = outerLayerEnemySpawnInterval;

            Vector3 spawnPos = transform.position + UnityEngine.Random.onUnitSphere * outerZoneCollider.radius;
            spawnPos.y = 0f;

            GameObject prefab = WaveManager.Instance.GetRandomEnemyPrefab();
            GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

            ACEnemyMovementBehaviour movement = enemy.GetComponent<ACEnemyMovementBehaviour>();
            movement.SetMovementType(EnemyMovementType.REMOTECONTROLLED);
            movement.SetDestination(transform.position); // Go to center

            outerLayerEnemies.Add(enemy);
        }
    }

    private void HandleOuterEnemyDespawn()
    {
        for (int i = outerLayerEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = outerLayerEnemies[i];
            if (!enemy)
            {
                outerLayerEnemies.RemoveAt(i);
                continue;
            }

            if (Vector3.Distance(enemy.transform.position, transform.position) <= centerDespawnRadius)
            {
                Destroy(enemy);
                outerLayerEnemies.RemoveAt(i);
            }
        }
    }

}
