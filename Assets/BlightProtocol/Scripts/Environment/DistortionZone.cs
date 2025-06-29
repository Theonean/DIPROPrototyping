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
    private List<ACEnemyMovementBehaviour> enemyBehaviours = new List<ACEnemyMovementBehaviour>();
    private const float timeToRecalculateDestionations = 1f;
    private float recalculateDestionationsTimer = 0f;
    private DistortionZoneState distortionZoneState = DistortionZoneState.IDLE;

    [Header("Enemies walk toward center settings")]
    [SerializeField] private SphereCollider innerZoneCollider;
    [SerializeField] private SphereCollider outerZoneCollider;

    [Header("Enemy Settings")]
    [SerializeField] private float outerLayerEnemySpawnInterval = 5f;
    [SerializeField] private int maxOuterEnemies = 10;
    [SerializeField] private int enemiesToSpawnPerDifficultyRegion = 30;
    [SerializeField] private float centerDespawnRadius = 5f;

    private float outerEnemySpawnTimer = 0f;
    private List<GameObject> outerLayerEnemies = new List<GameObject>();

    [Header("Health")]
    public Color damagedColor = Color.red;
    public int maxHealth = 2;
    private int currentHealth = 2;

    [Header("Spawner Settings")]
    [SerializeField] private GameObject RegularHive;
    [SerializeField] private GameObject ChargerHive;
    [SerializeField] private GameObject CrabtankHive;
    [SerializeField] private int hivesPerDifficultyRegion;
    private int difficultyLevel;
    private EnemyHiveManager[] spawners;

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, centerDespawnRadius);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        difficultyLevel = WorldComposer.Instance.GetDifficultyLevelFromZ(transform.position.z);

        int spawnersToSpawn = hivesPerDifficultyRegion * difficultyLevel;
        spawners = new EnemyHiveManager[spawnersToSpawn];

        //Guarantee at least one spawner of same difficulty region inside a zone
        switch(difficultyLevel)
        {
            case 0:
                CreateSpawner(RegularHive, 0);
                break;
                case 1:
                CreateSpawner(ChargerHive, 0);
                break;
                case 2:
                CreateSpawner(CrabtankHive, 0);
                break;
            default:
                CreateSpawner(GetRandomSpawnerPrefab(), 0);
                break;

        }

        for (int i = 0; i < spawnersToSpawn - 1; i++)
        {
            CreateSpawner(GetRandomSpawnerPrefab(), i);
        }

        int enemiesToSpawn = enemiesToSpawnPerDifficultyRegion * difficultyLevel + 1;
            
        for (int i = 0; i < enemiesToSpawn; i++)
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

        foreach (EnemyHiveManager spawnerManager in spawners)
        {
            spawnerManager.HarvesterEnteredRange();
        }
    }

    public void HarvesterEnteredOuterLayer()
    {
        distortionZoneState = DistortionZoneState.HARVESTER_OUTERLAYER;

        foreach (EnemyHiveManager spawnerManager in spawners)
        {
            spawnerManager.HarvesterExitedRange();
        }
    }

    public void HarvesterExited()
    {
        distortionZoneState = DistortionZoneState.IDLE;
    }

    private Vector3 GetRandomPositionInZone()
    {
        Vector3 enemySpawnPosition = Vector3.zero;

        while (Vector3.Distance(enemySpawnPosition, Harvester.Instance.transform.position) < 50 || Vector3.Distance(transform.position, enemySpawnPosition) < centerDespawnRadius)
        {
            enemySpawnPosition = transform.position + (UnityEngine.Random.insideUnitSphere * (innerZoneCollider.radius * 0.8f));
            enemySpawnPosition.y = 0f;
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

    public void TakeDamage()
    {
        currentHealth--;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
            GetComponent<ItemDropper>().DropItems();
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                material.color = damagedColor;
            }
        }
    }
    public GameObject GetRandomSpawnerPrefab()
    {
        int randomValue = UnityEngine.Random.Range(0, 100);

        if (difficultyLevel == 0)
        {
            return RegularHive;
        }
        if (difficultyLevel == 1)
        {
            if (randomValue < 30)
            {
                return ChargerHive;
            }
            else
            {
                return RegularHive;
            }
        }
        else
        {
            if (randomValue < 20)
            {
                return CrabtankHive;
            }
            else if (randomValue < 50)
            {
                return ChargerHive;
            }
            else
            {
                return RegularHive;
            }
        }
    }

    private void CreateSpawner(GameObject spawnerPrefab, int spawnerIndex)
    {
        Vector3 spawnPosition = GetRandomPositionInZone();

        GameObject spawner = Instantiate(spawnerPrefab, spawnPosition, Quaternion.identity);
        EnemyHiveManager spawnerManager = spawner.GetComponent<EnemyHiveManager>();
        spawners[spawnerIndex] = spawnerManager;
    }
}
