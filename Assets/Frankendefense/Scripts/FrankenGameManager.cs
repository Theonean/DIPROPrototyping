using System.Collections;
using System.Collections.Generic;
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

    public GameObject controlZone;
    public TextMeshProUGUI resourcesHarvestedText;
    public Slider gameProgressSlider;
    int m_MaxWaves = 5;
    int m_wavesSurvived = 0;
    public GameObject YouDiedUIOverlay;
    public EnemySpawner[] spawners = new EnemySpawner[4];
    private GameState m_GameState = GameState.HARVESTER_MOVING;
    float m_TotalGameTime = 0f;

    private void Start()
    {
        //Prepare the UI Overlay so it's not dependent on editor state
        YouDiedUIOverlay.SetActive(false);
        YouDiedUIOverlay.transform.localScale = Vector3.zero;

        gameProgressSlider.maxValue = m_MaxWaves;
        gameProgressSlider.value = 0;

        //Connect player died to healthmanager died event on control zone
        ControlZoneManager zoneManager = controlZone.GetComponent<ControlZoneManager>();
        zoneManager.died.AddListener(() => PlayerDied());
        zoneManager.changedState.AddListener((ZoneState state) => ManageWave(state));

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
            case ZoneState.MOVING:
                m_wavesSurvived++;
                gameProgressSlider.value = m_wavesSurvived;

                foreach (EnemySpawner spawner in spawners)
                {
                    spawner.StopWave();
                }
                break;
            case ZoneState.HARVESTING:
                Debug.Log("Harvesting with wave difficulty: " + m_wavesSurvived);
                //Start a new wave
                foreach (EnemySpawner spawner in spawners)
                {
                    spawner.StartWave(m_wavesSurvived);
                }
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
