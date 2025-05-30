using UnityEngine;

public class EnemyHiveManager : MonoBehaviour
{
    [Header("Health settings")]
    [SerializeField] private int maxHealth = 2;
    [SerializeField] private Color damagedColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;
    public int currentHealth { get; private set; }
    [Header("Enemy settings")]
    [SerializeField] private EnemySpawner[] enemySpawners;
    [SerializeField] private int maxEnemies = 10;
    private int spawnedEnemies = 0;
    private bool harvesterInRange = false;
    private bool hasEnemiesLeft = true;
    private int maximumDifficultyLevel = 20;
    private int currentDifficultyLevel = 0;
    private float maxDistanceLowestDifficulty = 100f;
    private float minDistanceHighestDifficulty = 50f;

    private EnergySignature mapUIComponent;

    void Start()
    {
        foreach (EnemySpawner spawner in enemySpawners)
        {
            spawner.spawnedEnemy.AddListener(IncrementEnemySpawned);
        }
        currentHealth = maxHealth;

        mapUIComponent = GetComponentInChildren<EnergySignature>();
    }

    void Update()
    {
        // Check if the harvester is in range and update the difficulty level accordingly
        if (harvesterInRange)
        {
            Vector3 playerPosition = PlayerCore.Instance.transform.position;
            Vector3 harvesterPosition = Harvester.Instance.transform.position;

            float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);
            float distanceToHarvester = Vector3.Distance(transform.position, harvesterPosition);
            float distanceToClosestEntity = Mathf.Min(distanceToPlayer, distanceToHarvester);

            int calculatedDifficulty = Mathf.Clamp(Mathf.FloorToInt((maxDistanceLowestDifficulty - distanceToClosestEntity) / (maxDistanceLowestDifficulty - minDistanceHighestDifficulty) * maximumDifficultyLevel), 0, maximumDifficultyLevel);
            if (calculatedDifficulty != currentDifficultyLevel)
            {
                currentDifficultyLevel = calculatedDifficulty;
                foreach (EnemySpawner spawner in enemySpawners)
                {
                    spawner.SetSpawnRate(currentDifficultyLevel);
                }
            }
        }
    }

    public void DestroySpawner()
    {
        // Destroy the spawner game object
        Destroy(gameObject);
    }

    public void HarvesterEnteredRange()
    {
        if (!hasEnemiesLeft) return;

        harvesterInRange = true;
        // Start spawning enemies
        foreach (EnemySpawner spawner in enemySpawners)
        {
            spawner.StartWave(1);
        }
    }

    public void HarvesterExitedRange()
    {
        harvesterInRange = false;
        // Stop spawning enemies
        foreach (EnemySpawner spawner in enemySpawners)
        {
            spawner.StopWave();
        }
    }
    public void TakeDamage()
    {
        currentHealth--;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
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

    private void IncrementEnemySpawned()
    {
        spawnedEnemies++;
        if (mapUIComponent.displayer) {
            mapUIComponent.displayer.FlashSignature();
        }
        
        if (spawnedEnemies >= maxEnemies)
        {
            hasEnemiesLeft = false;
            foreach (EnemySpawner spawner in enemySpawners)
            {
                spawner.StopWave();
            }
        }

    }
}
