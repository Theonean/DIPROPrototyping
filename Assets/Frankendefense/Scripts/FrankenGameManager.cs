using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrankenGameManager : MonoBehaviour
{
    enum GameState
    {
        BETWEENWAVES,
        WAVEONGOING,
        ENDOFWAVE, //Wait until all enemies are cleared
        GAMEOVER
    }

    public float waveTime = 5f;
    float m_WaveTimer = 0f;
    public GameObject controlZone;
    public GameObject MapBoundaries;
    public TextMeshProUGUI TimeToNextWaveText;
    public TextMeshProUGUI TimeNeededToSurviveText;
    public Slider gameProgressSlider;
    public Slider waveProgressSlider;
    int m_MaxWaves = 5;
    int m_wavesSurvived = 0;
    public GameObject YouDiedUIOverlay;
    public GameObject YouWonUIOverlay;
    public EnemySpawner[] spawners = new EnemySpawner[4];
    int m_SpawnersNoEnemyLeftCount = 0;
    private GameState m_GameState = GameState.BETWEENWAVES;
    Vector3[] m_BoundaryPositions;
    float m_TimeBetweenWaves = 3f;
    float m_TimeBetweenWavesLeft = 0f;
    float m_TotalGameTime = 0f;

    private void Start()
    {
        //Load the two corners of the map boundaries
        m_BoundaryPositions = new Vector3[2];
        m_BoundaryPositions[0] = MapBoundaries.transform.GetChild(0).position;
        m_BoundaryPositions[1] = MapBoundaries.transform.GetChild(1).position;

        //Instantly move zone at start of game
        m_WaveTimer = waveTime;

        m_TimeBetweenWavesLeft = m_TimeBetweenWaves;

        //Prepare the UI Overlay so it's not dependent on editor state
        YouDiedUIOverlay.SetActive(false);
        YouDiedUIOverlay.transform.localScale = Vector3.zero;

        gameProgressSlider.maxValue = m_MaxWaves;
        gameProgressSlider.value = 0;

        waveProgressSlider.maxValue = waveTime;
        waveProgressSlider.value = waveTime;

        //Connect player died to healthmanager died event on control zone
        HealthManager healthManager = controlZone.GetComponent<HealthManager>();
        healthManager.died.AddListener(() => PlayerDied());

        //Connect the spawners all enemy died event to the variable
        foreach (EnemySpawner spawner in spawners)
        {
            spawner.AllEnemiesDead.AddListener(() =>
            {
                m_SpawnersNoEnemyLeftCount++;
                if (m_SpawnersNoEnemyLeftCount == spawners.Length)
                {
                    m_GameState = GameState.BETWEENWAVES;
                    m_SpawnersNoEnemyLeftCount = 0;

                    MoveZone();
                    StartCoroutine(ScaleUpDownUI(gameProgressSlider.gameObject, 1.2f));

                    m_wavesSurvived++;
                    gameProgressSlider.value = m_wavesSurvived;

                    if (m_wavesSurvived >= m_MaxWaves)
                    {
                        TimeToNextWaveText.enabled = false;
                        PlayerWon();
                    }
                    else
                    {
                        TimeToNextWaveText.enabled = true;
                    }
                }
            });
        }

        //Move zone to a random spot on the map at the start
        MoveZone();
    }

    private void Update()
    {
        //Count how long player needed to survive
        if (m_GameState != GameState.GAMEOVER)
            m_TotalGameTime += Time.deltaTime;

        //Countdown to next wave and display it on the UI
        if (m_GameState == GameState.BETWEENWAVES)
        {
            //Reduce time between waves and put it on TimeToNextWaveText
            m_TimeBetweenWavesLeft -= Time.deltaTime;
            TimeToNextWaveText.text = "Next wave in: " + m_TimeBetweenWavesLeft.ToString("0.00");

            //If time between waves is over, start a new wave
            if (m_TimeBetweenWavesLeft <= 0f)
            {
                TimeToNextWaveText.enabled = false;
                m_TimeBetweenWavesLeft = m_TimeBetweenWaves;
                m_GameState = GameState.WAVEONGOING;
                foreach (EnemySpawner spawner in spawners)
                {
                    spawner.StartWave(m_wavesSurvived);
                }
            }
        }
        //Stop spawners from spawning enemies after wave is done
        else if (m_GameState == GameState.WAVEONGOING)
        {
            m_WaveTimer -= Time.deltaTime;
            waveProgressSlider.value = m_WaveTimer;
            if (m_WaveTimer <= 0f)
            {
                m_WaveTimer = waveTime;
                m_GameState = GameState.ENDOFWAVE;
                foreach (EnemySpawner spawner in spawners)
                {
                    spawner.StopWave();
                }
            }
        }


        if (m_GameState == GameState.GAMEOVER)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.LogWarning("Properly reset game without jarring reload");
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    void MoveZone()
    {
        //Move the zone to a random position within the map boundaries
        Vector3 newPosition = new Vector3(
            Random.Range(m_BoundaryPositions[0].x, m_BoundaryPositions[1].x),
            Random.Range(m_BoundaryPositions[0].y, m_BoundaryPositions[1].y),
            Random.Range(m_BoundaryPositions[0].z, m_BoundaryPositions[1].z)
        );

        controlZone.transform.position = newPosition;
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

        TimeNeededToSurviveText.text = "You survived for " + m_TotalGameTime.ToString("0.0") + " seconds!";

        StartCoroutine(ScaleUpUI(YouDiedUIOverlay));
    }

    void PlayerWon()
    {
        YouWonUIOverlay.SetActive(true);
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

        StartCoroutine(ScaleUpUI(YouWonUIOverlay));
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

    // Scale an element up by a certain percentage amount (smoothly with AnimationCurve) and down again.
    // The target scale after scaling up should be current scale * scaleMultiplier.
    IEnumerator ScaleUpDownUI(GameObject uiOverlay, float scaleMultiplier)
    {
        // Duration for a complete up and down cycle.
        float duration = 2.0f;
        // The original scale of the UI element.
        Vector3 originalScale = uiOverlay.transform.localScale;
        // The target scale after scaling up.
        Vector3 targetScale = originalScale * scaleMultiplier;

        // You can customize this AnimationCurve as needed.
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Track elapsed time.
        float elapsedTime = 0f;

        // Loop for the duration of the scale animation.
        while (elapsedTime < duration)
        {
            // Increase elapsed time.
            elapsedTime += Time.deltaTime;
            // Use Mathf.PingPong to smoothly oscillate between 0 and 1.
            float curveTime = Mathf.PingPong(elapsedTime / (duration / 2), 1);
            // Evaluate the curve and interpolate between original and target scales.
            uiOverlay.transform.localScale = Vector3.Lerp(originalScale, targetScale, curve.Evaluate(curveTime));
            yield return null; // Wait for the next frame.
        }

        // Ensure the final scale is exactly the original scale after the animation.
        uiOverlay.transform.localScale = originalScale;
    }

}
