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
    public CanvasGroup pauseGroup;
    private GameState m_GameState = GameState.HARVESTER_MOVING;
    
    public bool startWithTutorial = false;

    public float m_TotalGameTime = 0f;
    public bool isPaused = true;
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

        DontDestroyOnLoad(gameObject);
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
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        if (pauseFadeRoutine != null)
        {
            StopCoroutine(pauseFadeRoutine);
        }
        pauseFadeRoutine = StartCoroutine(FadeUI(pauseGroup, isPaused, 0.5f));
    }

    public void SetStartWithTutorial() { startWithTutorial = true; }
    public void ResetAfterRespawn()
    {
        // bring your game‐state back to “playing”
        m_GameState = GameState.HARVESTER_MOVING;
        Harvester.Instance.Reset();
        Time.timeScale = isPaused ? 0f : 1f;
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
}
