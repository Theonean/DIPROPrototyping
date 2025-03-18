using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DifficultySettings
{
    public float spawnRadius = 50f;
    public int baseEnemyCount = 20;
    public int ambushEnemyMultiplier = 1;
    public float ambushRangeScale = 60f;
    public Vector2 ambushWaveDelayRange = new Vector2(60f, 90f);
}

public class WaveManager : MonoBehaviour
{
    enum SurpriseAttackTypes
    {
        AMBUSH_CIRCLE,
        BIG_SINGLE_WAVE,
        SMALL_TRIPPLE_WAVE
    }

    enum WaveMode
    {
        IDLE, //when harvester is standing still
        AMBUSH_POSSIBLE, //When harvester is moving
        CONTINUOUS_ATTACK //when harvester is collecting resources (creating a lot of vibration)
    }

    [Header("Wave Manager Settings")]
    public EnemySpawner[] spawners;

    [Header("Difficulty Settings")]
    public DifficultySettings difficultySettings;

    private Queue<EnemySpawner> m_InactiveSpawners = new Queue<EnemySpawner>();
    private ZoneState harvesterState = ZoneState.IDLE;
    private WaveMode waveMode = WaveMode.IDLE;
    private int difficultyLevel = 5;
    private float ambushCounter = 0f;

    private void Awake()
    {
        spawners = spawners.OrderBy(x => UnityEngine.Random.value).ToArray();
        foreach (EnemySpawner spawner in spawners)
        {
            m_InactiveSpawners.Enqueue(spawner);
        }

        ControlZoneManager.Instance.changedState.AddListener(HarvesterChangedState);

        ambushCounter = UnityEngine.Random.Range(difficultySettings.ambushWaveDelayRange.x, difficultySettings.ambushWaveDelayRange.y);
    }

    void Update()
    {
        switch (waveMode)
        {
            case WaveMode.AMBUSH_POSSIBLE:
                ambushCounter -= Time.deltaTime;
                if (ambushCounter <= 0f)
                {
                    ambushCounter = UnityEngine.Random.Range(difficultySettings.ambushWaveDelayRange.x, difficultySettings.ambushWaveDelayRange.y);
                    TriggerRandomAmbush();
                }
                break;
        }
    }

    void HarvesterChangedState(ZoneState zoneState)
    {
        if(harvesterState == zoneState)
        {
            return;
        }

        //When switching away from harvesting, stop continuous attack
        if(harvesterState == ZoneState.HARVESTING)
        {
            DeactivateSpawners();
        }

        harvesterState = zoneState;
        float? numFuel = ResourceHandler.Instance.CheckResource(ResourceHandler.Instance.fuelResource);
        difficultyLevel = numFuel.HasValue ? Mathf.FloorToInt(numFuel.Value / 100) : 0;

        switch (harvesterState)
        {
            case ZoneState.IDLE:
                waveMode = WaveMode.IDLE;
                Logger.Log("Harvester is idle, no enemies will spawn", LogLevel.INFO, LogType.WAVEMANAGEMENT);
                break;
            case ZoneState.HARVESTING:
                waveMode = WaveMode.CONTINUOUS_ATTACK;
                ActivateSpawners(Mathf.Clamp(2 + difficultyLevel / 4, 1, spawners.Length));
                Logger.Log("Harvester is harvesting, enemies will spawn", LogLevel.INFO, LogType.WAVEMANAGEMENT);
                break;
            case ZoneState.MOVING:
                waveMode = WaveMode.AMBUSH_POSSIBLE;
                Logger.Log("Harvester is moving, ambushes are possible", LogLevel.INFO, LogType.WAVEMANAGEMENT);
                break;
        }
    }


    void TriggerRandomAmbush()
    {
        SurpriseAttackTypes attackType = (SurpriseAttackTypes)UnityEngine.Random.Range(0, Enum.GetValues(typeof(SurpriseAttackTypes)).Length);
        Logger.Log("Triggering ambush: " + attackType, LogLevel.INFO, LogType.WAVEMANAGEMENT);

        // Call the appropriate ambush method
        switch (attackType)
        {
            case SurpriseAttackTypes.AMBUSH_CIRCLE:
                StartCoroutine(AMBUSH_CIRCLE());
                break;
            case SurpriseAttackTypes.BIG_SINGLE_WAVE:
                StartCoroutine(BIG_SINGLE_WAVE());
                break;
            case SurpriseAttackTypes.SMALL_TRIPPLE_WAVE:
                StartCoroutine(SMALL_TRIPPLE_WAVE());
                break;
        }

    }

    void DeactivateSpawners()
    {
        foreach (EnemySpawner spawner in spawners)
        {
            spawner.StopWave();
            m_InactiveSpawners.Enqueue(spawner);
        }
    }

    void ActivateSpawners(int numberOfSpawners)
    {
        for (int i = 0; i < numberOfSpawners; i++)
        {
            EnemySpawner spawner = m_InactiveSpawners.Dequeue();
            spawner.StartWave(difficultyLevel);
        }
    }

    IEnumerator AMBUSH_CIRCLE()
    {
        Vector3 controlZonePosition = ControlZoneManager.Instance.transform.position;
        float radius = difficultySettings.spawnRadius;
        int enemyCount = difficultySettings.baseEnemyCount + difficultyLevel * difficultySettings.ambushEnemyMultiplier;

        for (int i = 0; i < enemyCount; i++)
        {
            float angle = i * Mathf.PI * 2 / enemyCount;
            Vector3 spawnPosition = new Vector3(controlZonePosition.x + Mathf.Cos(angle) * radius, controlZonePosition.y, controlZonePosition.z + Mathf.Sin(angle) * radius);
            Instantiate(spawners[0].enemyPrefab, spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.1f); // Slight delay to desynchronize animations
        }
    }

    IEnumerator BIG_SINGLE_WAVE()
    {
        Vector3 direction = (Vector3)UnityEngine.Random.insideUnitCircle;
        direction.y = 0f;
        direction.Normalize();
        direction *= difficultySettings.ambushRangeScale;
        Vector3 spawnPosition = ControlZoneManager.Instance.transform.position + direction;

        int enemyCount = difficultySettings.baseEnemyCount + difficultyLevel * difficultySettings.ambushEnemyMultiplier;
        int iX = Mathf.CeilToInt(Mathf.Sqrt(enemyCount));
        int iZ = Mathf.CeilToInt(enemyCount / (float)iX);

        for (int x = 0; x < iX; x++)
        {
            for (int z = 0; z < iZ; z++)
            {
                Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                Instantiate(spawners[0].enemyPrefab, spawnPosition + offset, Quaternion.identity);
                yield return new WaitForSeconds(0.1f); // Slight delay to desynchronize animations
            }
        }
    }

    IEnumerator SMALL_TRIPPLE_WAVE()
    {
        int enemyCount = difficultySettings.baseEnemyCount + difficultyLevel * difficultySettings.ambushEnemyMultiplier;
        int iI = Mathf.CeilToInt(enemyCount / 3f);
        int iX = Mathf.CeilToInt(Mathf.Sqrt(iI));
        int iZ = Mathf.CeilToInt(iI / (float)iX);

        for (int i = 0; i < iI; i++)
        {
            Vector3 direction = (Vector3)UnityEngine.Random.insideUnitCircle;
            direction.y = 0f;
            direction.Normalize();
            direction *= difficultySettings.ambushRangeScale;
            Vector3 spawnPosition = ControlZoneManager.Instance.transform.position + direction;

            for (int x = 0; x < iX; x++)
            {
                for (int z = 0; z < iZ; z++)
                {
                    Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                    Instantiate(spawners[0].enemyPrefab, spawnPosition + offset, Quaternion.identity);
                    yield return new WaitForSeconds(0.1f); // Slight delay to desynchronize animations
                }
            }
        }
    }
}
