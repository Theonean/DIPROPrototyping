using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrankenGameManager : MonoBehaviour
{
    enum GameState
    {
        HARVESTER_MOVING, //Moving from resource point to resource point
        HARVESTER_VULNERABLE, //"Gathering" Resources and vulnerable to enemy attacks
        GAMEOVER
    }

    enum SurpriseAttackTypes
    {
        AMBUSH_CIRCLE, //Enemies spawn in a circle around the Harvester
        BIG_SINGLE_WAVE, //One big wave of enemy spawns nearby
        SMALL_TRIPPLE_WAVE //Three small waves of enemies spawn nearby
    }

    public GameObject controlZone;
    public TextMeshProUGUI resourcesHarvestedText;
    public Vector2 MinMaxAmbushesPerMove;
    int m_wavesSurvived = 0;
    public GameObject YouDiedUIOverlay;
    public EnemySpawner[] spawners = new EnemySpawner[4];
    Queue<EnemySpawner> m_InactiveSpawners = new Queue<EnemySpawner>();
    Queue<EnemySpawner> m_ActiveSpawners = new Queue<EnemySpawner>();
    private GameState m_GameState = GameState.HARVESTER_MOVING;
    float m_AmbushesThisMove = 0;
    Queue<SurpriseAttackTypes> m_SurpriseAttacksQueuedThisMove = new Queue<SurpriseAttackTypes>();
    float m_TotalGameTime = 0f;
    bool m_HarvesterMoving = true;
    Queue<float> m_ambushTimes = new Queue<float>();

    private void Awake()
    {
        //Prepare the UI Overlay so it's not dependent on editor state
        YouDiedUIOverlay.SetActive(false);
        YouDiedUIOverlay.transform.localScale = Vector3.zero;

        //Connect player died to healthmanager died event on control zone
        ControlZoneManager zoneManager = controlZone.GetComponent<ControlZoneManager>();
        zoneManager.died.AddListener(() => PlayerDied());
        zoneManager.changedState.AddListener((ZoneState state) => ManageWave(state));

        //Randomize spawners array
        spawners = spawners.OrderBy(x => UnityEngine.Random.value).ToArray();

        //Add all spawners in a random order to the inactive spawners queue
        foreach (EnemySpawner spawner in spawners)
        {
            m_InactiveSpawners.Enqueue(spawner);
        }
    }
    private void Update()
    {
        //Count how long player needed to survive
        if (m_GameState != GameState.GAMEOVER)
            m_TotalGameTime += Time.deltaTime;

        if (m_GameState == GameState.GAMEOVER)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.LogWarning("Properly reset game without jarring reload");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    void ManageWave(ZoneState zoneState)
    {
        switch (zoneState)
        {
            //When moving have 1 + wavesSurvived/2 spawners active
            case ZoneState.MOVING:
                m_wavesSurvived++;
                DeactivateSpawners();

                //Determine number of ambushes this move cycle
                m_AmbushesThisMove = Mathf.RoundToInt(UnityEngine.Random.Range(MinMaxAmbushesPerMove.x, MinMaxAmbushesPerMove.y));
                m_SurpriseAttacksQueuedThisMove = new Queue<SurpriseAttackTypes>();

                //Create a random ambush type for each ambush
                for (int i = 0; i < m_AmbushesThisMove; i++)
                {
                    m_SurpriseAttacksQueuedThisMove.Enqueue((SurpriseAttackTypes)UnityEngine.Random.Range(0, Enum.GetNames(typeof(SurpriseAttackTypes)).Length));
                }

                //Calculate time between ambushes
                float totalTravelTime = controlZone.GetComponent<ControlZoneManager>().travelTimeLeft;
                float ambushOffset = totalTravelTime / 8;

                //Reduce travel time by 1 quarter, the first and last eigth of travel time should be safe without ambushes
                float ambushPossibleTimeFrame = totalTravelTime - (totalTravelTime / 4);

                //For each of the ambushes, calculate the time to spawn and put it in an array
                //Ambushes can start from the beginning of ambushPossibleTimeFrame to the end and happen in sequence 
                m_ambushTimes = new Queue<float>();
                for (int i = 1; i <= m_AmbushesThisMove; i++)
                {
                    m_ambushTimes.Enqueue(Mathf.Lerp(0, ambushPossibleTimeFrame, i / m_AmbushesThisMove) + ambushOffset);
                }
                m_HarvesterMoving = true;

                //Put all Information into the debug.log for easy validation
                Debug.Log("Moving with wave difficulty: " + m_wavesSurvived);
                Debug.Log("Ambushes this move: " + m_AmbushesThisMove);
                Debug.Log("Ambush times: " + string.Join(", ", m_ambushTimes));
                Debug.Log("Ambush types: " + string.Join(", ", m_SurpriseAttacksQueuedThisMove));
                Debug.Log("Total travel time: " + totalTravelTime);
                Debug.Log("Ambush possible time frame: " + ambushPossibleTimeFrame);
                Debug.Log("Ambush offset: " + ambushOffset);

                //Spawn Specific wave function in x seconds
                for (int i = 0; i < m_AmbushesThisMove; i++)
                {
                    Invoke(m_SurpriseAttacksQueuedThisMove.Dequeue().ToString(), m_ambushTimes.Dequeue());
                }

                break;
            //When Harvesting have 1 + wavesSurvived/2 spawners active
            case ZoneState.HARVESTING:
                m_HarvesterMoving = false;
                int harvestWaveDifficulty = Mathf.Clamp(1 + m_wavesSurvived / 2, 1, spawners.Length);
                ActivateSpawners(harvestWaveDifficulty);
                Debug.Log("Harvesting with wave difficulty: " + harvestWaveDifficulty);
                break;
        }
    }

    void PlayerDied()
    {
        YouDiedUIOverlay.SetActive(true);
        m_GameState = GameState.GAMEOVER;

        //Get all Follow player scripts and disable them
        FollowPlayer[] followPlayers = FindObjectsOfType<FollowPlayer>();
        foreach (FollowPlayer followPlayer in followPlayers)
        {
            followPlayer.enabled = false;
        }

        //Disable playercore
        PlayerCore playerCore = FindObjectOfType<PlayerCore>();
        playerCore.enabled = false;

        resourcesHarvestedText.text = "You harvested " + m_wavesSurvived + " waves worth of resources!";

        StartCoroutine(ScaleUpUI(YouDiedUIOverlay));
    }

    IEnumerator ScaleUpUI(GameObject uiOverlay)
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;
            uiOverlay.transform.localScale = Vector3.one * time;
            yield return null;
        }
    }

    void DeactivateSpawners()
    {
        //Deactivate all spawners
        foreach (EnemySpawner spawner in m_ActiveSpawners)
        {
            spawner.StopWave();
            m_InactiveSpawners.Enqueue(spawner);
        }
    }

    void ActivateSpawners(int numberOfSpawners)
    {
        //Activate the specified number of spawners
        for (int i = 0; i < numberOfSpawners; i++)
        {
            EnemySpawner spawner = m_InactiveSpawners.Dequeue();
            spawner.StartWave(m_wavesSurvived);
            m_ActiveSpawners.Enqueue(spawner);
        }
    }

    void AMBUSH_CIRCLE()
    {
        Debug.Log("Ambush Circle");
        //Spawn 20 enemies in a circle around the harvester

        //Get the control zone position
        Vector3 controlZonePosition = controlZone.transform.position;
        float radius = 50f;
        float enemyCount = 20;
        //Spawn 20 enemies in a circle around the harvester
        for (int i = 0; i < enemyCount; i++)
        {
            //Calculate the angle of the enemy
            float angle = i * Mathf.PI * 2 / enemyCount;
            //Calculate the position of the enemy
            Vector3 spawnPosition = new Vector3(controlZonePosition.x + Mathf.Cos(angle) * radius, controlZonePosition.y, controlZonePosition.z + Mathf.Sin(angle) * radius);
            Instantiate(spawners[0].enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void BIG_SINGLE_WAVE()
    {
        Debug.Log("Big Single Wave");
        float distanceToHarvester = 60f;
        //Determine a random direction
        Vector2 randomDir = UnityEngine.Random.insideUnitCircle;
        Vector3 spawnPosition = controlZone.transform.position + new Vector3(randomDir.x, 0, randomDir.y) * distanceToHarvester;

        //Spawn 10 enemies in X-Z Grid around spawnposition
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 5; z++)
            {
                Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                Vector3 enemyPosition = spawnPosition + offset;
                Instantiate(spawners[0].enemyPrefab, enemyPosition, Quaternion.identity);
            }
        }
    }

    void SMALL_TRIPPLE_WAVE()
    {
        Debug.Log("Small Tripple Wave");
        float distanceToHarvester = 50f;
        //Determine a random direction 3 times and create a wave of enemies in that direction
        for (int i = 0; i < 3; i++)
        {
            //Determine a random direction
            Vector2 randomDir = UnityEngine.Random.insideUnitCircle;
            Vector3 spawnPosition = controlZone.transform.position + new Vector3(randomDir.x, 0, randomDir.y) * distanceToHarvester;

            //Spawn 4 enemies in X-Z Grid around spawnposition
            for (int x = 0; x < m_wavesSurvived + 1; x++)
            {
                for (int z = 0; z < m_wavesSurvived + 1; z++)
                {
                    Vector3 offset = new Vector3(x * 5 - 5, 0, z * 5 - 7.5f);
                    Vector3 enemyPosition = spawnPosition + offset;
                    Instantiate(spawners[0].enemyPrefab, enemyPosition, Quaternion.identity);
                }
            }
        }
    }
}
