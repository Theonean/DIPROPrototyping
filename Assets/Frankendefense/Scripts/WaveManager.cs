using System;
using System.Collections.Generic;
using System.Linq;
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
                DeactivateSpawners();
                PrepareAmbushes();
                TriggerAmbushes();
                break;
            case ZoneState.HARVESTING:
                ActivateSpawners(Mathf.Clamp(1 + m_wavesSurvived / 2, 1, spawners.Length));
                break;
        }
    }

    void PrepareAmbushes()
    {
        m_AmbushesThisMove = Mathf.RoundToInt(UnityEngine.Random.Range(MinMaxAmbushesPerMove.x, MinMaxAmbushesPerMove.y));
        m_SurpriseAttacksQueuedThisMove.Clear();

        for (int i = 0; i < m_AmbushesThisMove; i++)
        {
            m_SurpriseAttacksQueuedThisMove.Enqueue((SurpriseAttackTypes)UnityEngine.Random.Range(0, Enum.GetNames(typeof(SurpriseAttackTypes)).Length));
        }

        float totalTravelTime = controlZone.GetComponent<ControlZoneManager>().travelTimeLeft;
        float ambushOffset = totalTravelTime / difficultySettings.ambushOffsetFactor; //Guarantees that ambush will happen before the zone reaches the end and not immediately at beginning of move
        float ambushPossibleTimeFrame = totalTravelTime - (totalTravelTime / 4);

        // Center events around the middle of the possible ambush timeframe
        float midPoint = ambushPossibleTimeFrame / 2f;

        m_ambushTimes.Clear();

        // Generate ambush times symmetrically around the midpoint
        for (int i = 1; i <= m_AmbushesThisMove; i++)
        {
            // Calculate a normalized position between -1 and 1 where -1 is the start and 1 is the end
            float normalizedPos = ((float)i - 0.5f) / m_AmbushesThisMove; // Spread around the middle
            float timeOffsetFromMid = normalizedPos * (ambushPossibleTimeFrame / 2); // Spread out from the middle
            float ambushTime = midPoint + timeOffsetFromMid + ambushOffset; // Shift the event time accordingly

            m_ambushTimes.Enqueue(ambushTime);
        }

        // Debug the information
        Debug.Log($"Ambushes This Move: {m_AmbushesThisMove}, Ambush Offset: {ambushOffset}, Ambush Times: {string.Join(", ", m_ambushTimes)}, Total Travel Time: {totalTravelTime}");
    }


    void TriggerAmbushes()
    {
        for (int i = 0; i < m_AmbushesThisMove; i++)
        {
            Invoke(m_SurpriseAttacksQueuedThisMove.Dequeue().ToString(), m_ambushTimes.Dequeue());
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

    void AMBUSH_CIRCLE()
    {
        Vector3 controlZonePosition = controlZone.transform.position;
        float radius = difficultySettings.spawnRadius;
        int enemyCount = difficultySettings.baseEnemyCount + m_wavesSurvived * difficultySettings.ambushEnemyMultiplier;

        for (int i = 0; i < enemyCount; i++)
        {
            float angle = i * Mathf.PI * 2 / enemyCount;
            Vector3 spawnPosition = new Vector3(controlZonePosition.x + Mathf.Cos(angle) * radius, controlZonePosition.y, controlZonePosition.z + Mathf.Sin(angle) * radius);
            Instantiate(spawners[0].enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void BIG_SINGLE_WAVE()
    {
        Vector3 spawnPosition = controlZone.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * difficultySettings.ambushRangeScale;
        spawnPosition.y = 0f;

        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                Instantiate(spawners[0].enemyPrefab, spawnPosition + offset, Quaternion.identity);
            }
        }
    }

    void SMALL_TRIPPLE_WAVE()
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = controlZone.transform.position + (Vector3)UnityEngine.Random.insideUnitCircle * difficultySettings.ambushRangeScale;
            spawnPosition.y = 0f;

            for (int x = 0; x < m_wavesSurvived + 1; x++)
            {
                for (int z = 0; z < m_wavesSurvived + 1; z++)
                {
                    Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                    Instantiate(spawners[0].enemyPrefab, spawnPosition + offset, Quaternion.identity);
                }
            }
        }
    }
}
