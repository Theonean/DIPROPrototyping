using UnityEngine;
using System.Collections.Generic;
using TMPro;

public enum ArenaState
{
    IDLE, //at the start or when all waves have been finished
    PAUSED, //paused by player
    SPAWNING, //Spawning enemies during a wave
    FINISHED_SPAWNING //wave timer depleted but enemies still alive
}

public class CombatArenaManager : MonoBehaviour
{
    public int waveTime = 60;
    private float waveTimer = 0f;
    public int difficultyStartLevel = 3;
    public int difficultyIncreasePerWave = 4;
    public int spawnersStartAmount = 1;
    public int spawnersIncreasePerWave = 1;

    private int currentWave = 0;
    private int arenaRepeatsSurvived = 0;
    public SOEnemySpawnPattern[] spawnPatternsForWaves;

    private ArenaState state = ArenaState.IDLE;
    private int spawnersWithAllEnemiesKilled = 0;

    public KeyCode toggleArenaInput;
    [SerializeField] private EnemySpawner[] spawners = new EnemySpawner[4];
    private EnemySpawner[] activeSpawners;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI waveStatusText;
    [SerializeField] private TextMeshProUGUI waveProgressText;

    void Start()
    {
        waveStatusText.text = "press " + toggleArenaInput + " to start/pause";
        UpdateWaveUI();

        foreach (EnemySpawner spawner in spawners) 
            spawner.AllEnemiesDead.AddListener(AllEnemiesKilledOnSpawner);
    }

    void Update()
    {
        if(Input.GetKeyDown(toggleArenaInput))
        {
            if (state == ArenaState.IDLE)
            {
                StartWave();
            }
            else if(state == ArenaState.PAUSED)
            {
                StartSpawners();
            }
            else
            {
                state = ArenaState.PAUSED;
                StopSpawners();
            }
        }

        if(state == ArenaState.SPAWNING)
        {
            waveTimer -= Time.deltaTime;
            if(waveTimer <= 0f)
            {
                TransitionInbetweenWaves();
            }
        }

    }

    private void UpdateWaveUI()
    {
        waveText.text = state == ArenaState.IDLE || state == ArenaState.PAUSED ? "paused" : "ongoing";
        if (state == ArenaState.IDLE && arenaRepeatsSurvived > 0)
            waveText.text = "fantabulous performance! survived the arena " + arenaRepeatsSurvived + " times";

        waveProgressText.text = currentWave + " / " + spawnPatternsForWaves.Length;
    }

    private void AllEnemiesKilledOnSpawner()
    {
        if (state != ArenaState.FINISHED_SPAWNING) return;

        spawnersWithAllEnemiesKilled++;
        if (spawnersWithAllEnemiesKilled == activeSpawners.Length)
        {
            if (currentWave == spawnPatternsForWaves.Length)
            {
                state = ArenaState.IDLE;
                currentWave = 0;
                arenaRepeatsSurvived++;
                UpdateWaveUI();
            }
            else
                StartWave();
        }
    }


    private void StartWave()
    {
        waveTimer = waveTime;
        currentWave++;

        // Calculate number of spawners to activate this wave
        int spawnerCount = Mathf.Min(spawners.Length, spawnersStartAmount + spawnersIncreasePerWave * (currentWave - 1));

        // Randomly pick spawners
        List<EnemySpawner> spawnerPool = new List<EnemySpawner>(spawners);
        activeSpawners = new EnemySpawner[spawnerCount];

        for (int i = 0; i < spawnerCount; i++)
        {
            int index = Random.Range(0, spawnerPool.Count);
            activeSpawners[i] = spawnerPool[index];
            spawnerPool.RemoveAt(index);
        }

        StartSpawners();
    }


    private void StartSpawners()
    {
        foreach (EnemySpawner spawner in activeSpawners)
        {
            spawner.enemySpawnPatterns = new SOEnemySpawnPattern[] { spawnPatternsForWaves[currentWave - 1] };
            spawner.StartWave(difficultyStartLevel + difficultyIncreasePerWave * currentWave);
        }
        state = ArenaState.SPAWNING;
        UpdateWaveUI();
    }


    private void StopSpawners()
    {
        if (activeSpawners == null) return;

        foreach (EnemySpawner spawner in activeSpawners)
        {
            spawner.StopWave();
        }
        UpdateWaveUI();
    }


    private void TransitionInbetweenWaves()
    {
        state = ArenaState.FINISHED_SPAWNING;
        StopSpawners();
        spawnersWithAllEnemiesKilled = 0;
    }
}
