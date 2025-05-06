using System.Collections;
using TMPro;
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
    public CanvasGroup pauseGroup;
    private GameState m_GameState = GameState.HARVESTER_MOVING;
    private float m_TotalGameTime = 0f;
    private int m_wavesSurvived = 0;
    public bool isPaused = false;
    Coroutine pauseFadeRoutine;

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

        Harvester harvester = controlZone.GetComponent<Harvester>();
        harvester.health.died.AddListener(() => PlayerDied());
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
            StopAllCoroutines();
            StartCoroutine(RespawnPlayer());
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        if (pauseFadeRoutine != null)
        {
            StopCoroutine(pauseFadeRoutine);
        }
        pauseFadeRoutine = StartCoroutine(FadeUI(pauseGroup, isPaused, 0.5f));
    }

    public void IncrementWavesSurvived()
    {
        m_wavesSurvived++;
    }

    void PlayerDied()
    {
        gameOverGroup.alpha = 0;

        m_GameState = GameState.GAMEOVER;

        PlayerCore playerCore = PlayerCore.Instance;
        playerCore.enabled = false;

        resourcesHarvestedText.text = "";
        StartCoroutine(FadeUI(gameOverGroup, true, 20f));

        // Unpause if the game is over
        if (isPaused)
        {
            TogglePause();
        }
    }

    IEnumerator FadeUI(CanvasGroup uiOverlay, bool fadeIn, float maxTime)
    {
        float time = fadeIn ? uiOverlay.alpha * maxTime : (1 - uiOverlay.alpha) * maxTime;
        while (time < maxTime)
        {
            time += Time.unscaledDeltaTime;
            uiOverlay.alpha = fadeIn ? time / maxTime : 1 - time / maxTime;
            yield return null;
        }
        uiOverlay.alpha = fadeIn ? 1 : 0;
    }

    IEnumerator RespawnPlayer()
    {
        Vector3 spawnPosition = Harvester.Instance.respawnPoint;
        AnimationCurve inOutSmooth = AnimationCurve.EaseInOut(0, 0, 1, 1);

        float startHeight = 200f;
        Vector3 startPosition = new Vector3(spawnPosition.x, startHeight, spawnPosition.z);

        float animationTime = 2f;
        float t = 0f;

        while(t < animationTime)
        {
            gameOverGroup.alpha = Mathf.Lerp(1, 0, inOutSmooth.Evaluate(t));
            Harvester.Instance.transform.position = Vector3.Lerp(startPosition, spawnPosition, inOutSmooth.Evaluate(t / animationTime));
            t+= Time.deltaTime;
            yield return null;
        }

        m_GameState = GameState.HARVESTER_MOVING;
        Harvester.Instance.Reset();

        PlayerCore playerCore = PlayerCore.Instance;
        playerCore.enabled = true;
        CameraTracker.Instance.objectToTrack = playerCore.gameObject;

    }
}
