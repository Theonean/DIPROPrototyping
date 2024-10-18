using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class DifficultySettings
{
    public float spawnRadius = 50f;
    public int baseEnemyCount = 20;
    public float ambushOffsetFactor = 8f;
    public int ambushEnemyMultiplier = 1;
    public float ambushRangeScale = 60f;
}

public class WaveManager : MonoBehaviour
{
    enum SurpriseAttackTypes
    {
        AMBUSH_CIRCLE,
        BIG_SINGLE_WAVE,
        SMALL_TRIPPLE_WAVE
    }

    [Header("Wave Manager Settings")]
    public GameObject controlZone;
    public Vector2 MinMaxAmbushesPerMove;
    public EnemySpawner[] spawners;

    [Header("Difficulty Settings")]
    public DifficultySettings difficultySettings;

    private Queue<EnemySpawner> m_InactiveSpawners = new Queue<EnemySpawner>();
    private Queue<SurpriseAttackTypes> m_SurpriseAttacksQueuedThisMove = new Queue<SurpriseAttackTypes>();
    private Queue<float> m_ambushTimes = new Queue<float>();
    private float m_AmbushesThisMove = 0;
    private int m_wavesSurvived = 0;

    private void Awake()
    {
        spawners = spawners.OrderBy(x => UnityEngine.Random.value).ToArray();
        foreach (EnemySpawner spawner in spawners)
        {
            m_InactiveSpawners.Enqueue(spawner);
        }

        ControlZoneManager zoneManager = controlZone.GetComponent<ControlZoneManager>();
        zoneManager.changedState.AddListener(ManageWave);
    }

    void ManageWave(ZoneState zoneState)
    {
        switch (zoneState)
        {
            case ZoneState.MOVING:
                m_wavesSurvived++;
                FrankenGameManager.Instance.IncrementWavesSurvived();
                DeactivateSpawners();
                PrepareAmbushes();
                StartCoroutine(TriggerAmbushes());  // Coroutine to trigger ambushes over time
                break;
            case ZoneState.HARVESTING:
                ActivateSpawners(Mathf.Clamp(1 + m_wavesSurvived * 2, 1, spawners.Length));
                break;
        }
    }

    void PrepareAmbushes()
    {
        m_AmbushesThisMove = Mathf.FloorToInt(MinMaxAmbushesPerMove.x + m_wavesSurvived / 2);
        m_SurpriseAttacksQueuedThisMove.Clear();

        for (int i = 0; i < m_AmbushesThisMove; i++)
        {
            m_SurpriseAttacksQueuedThisMove.Enqueue((SurpriseAttackTypes)UnityEngine.Random.Range(0, Enum.GetNames(typeof(SurpriseAttackTypes)).Length));
        }

        float totalTravelTime = controlZone.GetComponent<ControlZoneManager>().travelTimeLeft;
        float ambushOffset = totalTravelTime / difficultySettings.ambushOffsetFactor;  // Ensures ambush doesn't start right away
        float ambushPossibleTimeFrame = totalTravelTime - ambushOffset * 2;  // Exclude beginning and end times for ambushes

        // Handle case where there is only 1 ambush
        m_ambushTimes.Clear();
        if (m_AmbushesThisMove == 1)
        {
            // Put the single ambush in the middle of the ambush window
            float ambushTime = ambushOffset + ambushPossibleTimeFrame / 2;
            m_ambushTimes.Enqueue(ambushTime);
        }
        else
        {
            // Calculate evenly spaced ambushes over the possible timeframe for more than 1 ambush
            for (int i = 0; i < m_AmbushesThisMove; i++)
            {
                float normalizedPos = (float)i / (m_AmbushesThisMove - 1);  // Normalized between 0 and 1
                float ambushTime = ambushOffset + normalizedPos * ambushPossibleTimeFrame;  // Spread over full ambush timeframe
                m_ambushTimes.Enqueue(ambushTime);
            }
        }

        Debug.Log($"Ambushes This Move: {m_AmbushesThisMove}, Ambush Times: {string.Join(", ", m_ambushTimes)}, Total Travel Time: {totalTravelTime}");
    }


    IEnumerator TriggerAmbushes()
    {
        while (m_ambushTimes.Count > 0 && m_SurpriseAttacksQueuedThisMove.Count > 0)
        {
            float ambushTime = m_ambushTimes.Dequeue();
            SurpriseAttackTypes attackType = m_SurpriseAttacksQueuedThisMove.Dequeue();

            yield return new WaitForSeconds(ambushTime);  // Wait for the appropriate ambush time

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
            spawner.StartWave(m_wavesSurvived);
        }
    }

    IEnumerator AMBUSH_CIRCLE()
    {
        Vector3 controlZonePosition = controlZone.transform.position;
        float radius = difficultySettings.spawnRadius;
        int enemyCount = difficultySettings.baseEnemyCount + m_wavesSurvived * difficultySettings.ambushEnemyMultiplier;

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
        Vector3 spawnPosition = controlZone.transform.position + direction;

        int enemyCount = difficultySettings.baseEnemyCount + m_wavesSurvived * difficultySettings.ambushEnemyMultiplier;
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
        int enemyCount = difficultySettings.baseEnemyCount + m_wavesSurvived * difficultySettings.ambushEnemyMultiplier;
        int iI = Mathf.CeilToInt(enemyCount / 3f);
        int iX = Mathf.CeilToInt(Mathf.Sqrt(iI));
        int iZ = Mathf.CeilToInt(iI / (float)iX);

        for (int i = 0; i < iI; i++)
        {
            Vector3 direction = (Vector3)UnityEngine.Random.insideUnitCircle;
            direction.y = 0f;
            direction.Normalize();
            direction *= difficultySettings.ambushRangeScale;
            Vector3 spawnPosition = controlZone.transform.position + direction;

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
