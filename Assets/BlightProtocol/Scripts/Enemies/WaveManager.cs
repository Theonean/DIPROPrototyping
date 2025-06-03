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
    public static WaveManager Instance { get; private set; }
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
    public GameObject regularEnemyPrefab;
    public GameObject chargerEnemyPrefab;
    public GameObject tankEnemyPrefab;

    [Header("Difficulty Settings")]
    public DifficultySettings difficultySettings;

    private Queue<EnemySpawner> m_InactiveSpawners = new Queue<EnemySpawner>();
    private HarvesterState harvesterState = HarvesterState.IDLE;
    private WaveMode waveMode = WaveMode.IDLE;
    public int difficultyLevel = 5;
    private float ambushCounter = 0f;
    private int counterMultiplier = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        spawners = spawners.OrderBy(x => UnityEngine.Random.value).ToArray();
        foreach (EnemySpawner spawner in spawners)
        {
            m_InactiveSpawners.Enqueue(spawner);
        }

        Harvester.Instance.changedState.AddListener(HarvesterChangedState);

        ambushCounter = UnityEngine.Random.Range(difficultySettings.ambushWaveDelayRange.x, difficultySettings.ambushWaveDelayRange.y);
    }

    private void OnEnable()
    {
        if(DifficultyManager.Instance) DifficultyManager.Instance.OnDifficultyLevelChanged.AddListener(UpdateAmbushTimes);
        if(Seismograph.Instance) Seismograph.Instance.OnDangerLevelChanged.AddListener(ConnectSeismographToAmbushCountdown);
    }

    private void OnDisable()
    {
        DifficultyManager.Instance.OnDifficultyLevelChanged.RemoveListener(UpdateAmbushTimes);
        if (Seismograph.Instance) Seismograph.Instance.OnDangerLevelChanged.RemoveListener(ConnectSeismographToAmbushCountdown);
    }

    void Update()
    {
        if (TutorialManager.Instance.IsTutorialOngoing())
            return;

        switch (waveMode)
        {
            case WaveMode.AMBUSH_POSSIBLE:
                ambushCounter -= Time.deltaTime * counterMultiplier;
                if (ambushCounter <= 0f)
                {
                    ambushCounter = UnityEngine.Random.Range(difficultySettings.ambushWaveDelayRange.x, difficultySettings.ambushWaveDelayRange.y);
                    TriggerRandomAmbush();
                }
                break;
        }


        if (FrankenGameManager.Instance.DevMode)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                difficultyLevel++;
                Logger.Log("Increased difficulty level to " + difficultyLevel, LogLevel.FORCE, LogType.WAVEMANAGEMENT);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                difficultyLevel--;
                if (difficultyLevel < 0)
                {
                    difficultyLevel = 0;
                }
                Logger.Log("Decreased difficulty level to " + difficultyLevel, LogLevel.FORCE, LogType.WAVEMANAGEMENT);

            }
            else if(Input.GetKeyDown(KeyCode.E))
            {
                TriggerRandomAmbush();
            }
        }
    }

    public GameObject GetRandomEnemyPrefab()
    {
        int randomValue = UnityEngine.Random.Range(0, 100);
        int difficultyRegion = DifficultyManager.Instance.difficultyLevel;

        if(difficultyRegion == 0)
        {
            return regularEnemyPrefab;
        }
        if(difficultyRegion == 1)
        {
         if (randomValue < 30)
            {
                return chargerEnemyPrefab;
            }
            else
            {
                return regularEnemyPrefab;
            }
        }
        else
        {
            if (randomValue < 20)
            {
                return tankEnemyPrefab;
            }
            else if (randomValue < 30)
            {
                return chargerEnemyPrefab;
            }
            else
            {
                return regularEnemyPrefab;
            }
        }
    }

    private void ConnectSeismographToAmbushCountdown(int amount)
    {
        counterMultiplier = amount;
    }

    private void UpdateAmbushTimes()
    {
        float previousAverageRange = (difficultySettings.ambushWaveDelayRange.y + difficultySettings.ambushWaveDelayRange.x) / 2f;

        int difficultyRegion = DifficultyManager.Instance.difficultyLevel;
        int maximumDifficultyRegions = DifficultyManager.Instance.maximumDifficultyRegions - 1; //difficulty regions start at 0

        difficultyLevel = 3 + difficultyRegion * 3;

        float ambushIntervalBottomValue = Mathf.Lerp(30, 15, difficultyRegion / maximumDifficultyRegions);
        float ambushIntervalTopValue = Mathf.Lerp(60, 30, difficultyRegion / maximumDifficultyRegions);

        difficultySettings.ambushWaveDelayRange = new Vector2(ambushIntervalBottomValue, ambushIntervalTopValue);

        float newAverageRange = (difficultySettings.ambushWaveDelayRange.y + difficultySettings.ambushWaveDelayRange.x) / 2f;
        float differenceBetweenRanges = previousAverageRange - newAverageRange;
        ambushCounter -= differenceBetweenRanges;
    }

    void HarvesterChangedState(HarvesterState zoneState)
    {
        if (harvesterState == zoneState)
        {
            return;
        }

        if (TutorialManager.Instance.IsTutorialOngoing())
            return;

        //When switching away from harvesting, stop continuous attack
        if (harvesterState == HarvesterState.HARVESTING)
        {
            DeactivateSpawners();
        }

        harvesterState = zoneState;
        /*float? numGas = ItemManager.Instance.GetGas();
        difficultyLevel = numGas.HasValue ? Mathf.FloorToInt(numGas.Value / 150) : 0;*/

        switch (harvesterState)
        {
            case HarvesterState.IDLE:
                waveMode = WaveMode.IDLE;
                Logger.Log("Harvester is idle, no enemies will spawn", LogLevel.INFO, LogType.WAVEMANAGEMENT);
                break;
            case HarvesterState.START_HARVESTING:
                waveMode = WaveMode.CONTINUOUS_ATTACK;
                ActivateSpawners(Mathf.Clamp(2 + difficultyLevel / 4, 1, spawners.Length));
                Logger.Log("Harvester is harvesting, enemies will spawn", LogLevel.INFO, LogType.WAVEMANAGEMENT);
                break;
            case HarvesterState.MOVING:
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
        Vector3 controlZonePosition = Harvester.Instance.transform.position;
        float radius = difficultySettings.spawnRadius;
        int enemyCount = difficultySettings.baseEnemyCount + difficultyLevel * difficultySettings.ambushEnemyMultiplier;

        for (int i = 0; i < enemyCount; i++)
        {
            float angle = i * Mathf.PI * 2 / enemyCount;
            Vector3 spawnPosition = new Vector3(controlZonePosition.x + Mathf.Cos(angle) * radius, controlZonePosition.y, controlZonePosition.z + Mathf.Sin(angle) * radius);
            Instantiate(GetRandomEnemyPrefab(), spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(0.1f); // Slight delay to desynchronize animations
        }
    }

    IEnumerator BIG_SINGLE_WAVE()
    {
        Vector3 direction = (Vector3)UnityEngine.Random.insideUnitCircle;
        direction.y = 0f;
        direction.Normalize();
        direction *= difficultySettings.ambushRangeScale;
        Vector3 spawnPosition = Harvester.Instance.transform.position + direction;

        int enemyCount = difficultySettings.baseEnemyCount + difficultyLevel * difficultySettings.ambushEnemyMultiplier;
        int iX = Mathf.CeilToInt(Mathf.Sqrt(enemyCount));
        int iZ = Mathf.CeilToInt(enemyCount / (float)iX);

        for (int x = 0; x < iX; x++)
        {
            for (int z = 0; z < iZ; z++)
            {
                Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                Instantiate(GetRandomEnemyPrefab(), spawnPosition + offset, Quaternion.identity);
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
            Vector3 spawnPosition = Harvester.Instance.transform.position + direction;

            for (int x = 0; x < iX; x++)
            {
                for (int z = 0; z < iZ; z++)
                {
                    Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                    Instantiate(GetRandomEnemyPrefab(), spawnPosition + offset, Quaternion.identity);
                    yield return new WaitForSeconds(0.1f); // Slight delay to desynchronize animations
                }
            }
        }
    }
}
