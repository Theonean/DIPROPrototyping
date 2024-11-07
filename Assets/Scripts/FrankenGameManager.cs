using System.Collections;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;

public class FrankenGameManager : MonoBehaviour
{
    enum GameState
    {
        HARVESTER_MOVING,
        HARVESTER_VULNERABLE,
        GAMEOVER
    }

    public static FrankenGameManager Instance { get; private set; }
    public GameObject controlZone;
    public TextMeshProUGUI resourcesHarvestedText;
    public CanvasGroup gameOverGroup;
    private GameState m_GameState = GameState.HARVESTER_MOVING;
    private float m_TotalGameTime = 0f;
    private int m_wavesSurvived = 0;
    public bool isPaused = false;

    private void Awake()
    {
        // Ensure there's only one instance of the FrankenGameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        gameOverGroup.alpha = 0;

        ControlZoneManager zoneManager = controlZone.GetComponent<ControlZoneManager>();
        zoneManager.died.AddListener(() => PlayerDied());
    }

    private void Update()
    {
        if (m_GameState != GameState.GAMEOVER)
        {
            if (!isPaused)
                m_TotalGameTime += Time.deltaTime;

            // Toggle pause with spacebar
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        // Reload the scene when 'R' is pressed if the game is over
        if (m_GameState == GameState.GAMEOVER && Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
    }

    public void IncrementWavesSurvived()
    {
        m_wavesSurvived++;
    }

    void PlayerDied()
    {
        gameOverGroup.alpha = 0;

        m_GameState = GameState.GAMEOVER;

        FollowPlayer[] followPlayers = FindObjectsOfType<FollowPlayer>();
        foreach (FollowPlayer followPlayer in followPlayers)
        {
            followPlayer.enabled = false;
        }

        PlayerCore playerCore = FindObjectOfType<PlayerCore>();
        playerCore.enabled = false;

        resourcesHarvestedText.text = "You harvested " + m_wavesSurvived + " waves worth of resources!";
        StartCoroutine(ScaleUpUI(gameOverGroup));

        // Unpause if the game is over
        if (isPaused)
        {
            TogglePause();
        }
    }

    IEnumerator ScaleUpUI(CanvasGroup uiOverlay)
    {
        float time = 0f;
        float maxTime = 20f;
        while (time < maxTime)
        {
            time += Time.deltaTime;
            uiOverlay.alpha = Mathf.Lerp(0, 1, time / maxTime);
            yield return null;
        }
        uiOverlay.alpha = 1;
    }
}
