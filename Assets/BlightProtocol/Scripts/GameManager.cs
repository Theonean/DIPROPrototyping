using Assets.BlightProtocol.Scripts;
using System.Collections;
using TMPro;
using UnityEngine;

public class FrankenGameManager : MonoBehaviour
{
    public enum GameState
    {
        HARVESTER_MOVING,
        HARVESTER_VULNERABLE,
        GAMEOVER
    }

    public static FrankenGameManager Instance { get; private set; }
    [Header("DEBUG SETTINGS")]
    public bool DevMode = false;
    public bool overrideStartInDrone = false;

    [Header("GAME SETTINGS")]
    public GameState m_GameState = GameState.HARVESTER_MOVING;
    
    public bool startWithTutorial = false;

    public float m_TotalGameTime = 0f;

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
        
        if(DevMode)
        {
            Debug.LogAssertion("Dev Mode is enabled, gameplay will differ from standard gameplay experience");
        }
    }

    private void Update()
    {
        if (m_GameState != GameState.GAMEOVER)
        {
            if (EndOfGameManager.Instance && !EndOfGameManager.Instance.isPaused)
                m_TotalGameTime += Time.deltaTime;
        }
    }

    public void SetStartWithTutorial(bool enabled) { startWithTutorial = enabled; }
    public void ResetAfterRespawn()
    {
        // bring your game‐state back to “playing”
        m_GameState = GameState.HARVESTER_MOVING;
        Harvester.Instance.Reset();
        Time.timeScale = EndOfGameManager.Instance.isPaused ? 0f : 1f;
    }
}
