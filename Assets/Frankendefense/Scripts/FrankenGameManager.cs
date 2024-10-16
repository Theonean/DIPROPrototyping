using System.Collections;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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
    public GameObject YouDiedUIOverlay;
    public GameObject obstaclePrefab;
    private GameState m_GameState = GameState.HARVESTER_MOVING;
    private float m_TotalGameTime = 0f;
    private int m_wavesSurvived = 0;

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

        YouDiedUIOverlay.SetActive(false);
        YouDiedUIOverlay.transform.localScale = Vector3.zero;

        ControlZoneManager zoneManager = controlZone.GetComponent<ControlZoneManager>();
        zoneManager.died.AddListener(() => PlayerDied());
    }

    private void Start()
    {
        GenerateObstaclesOnField();
    }

    private void Update()
    {
        if (m_GameState != GameState.GAMEOVER)
            m_TotalGameTime += Time.deltaTime;

        if (m_GameState == GameState.GAMEOVER)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
    }

    public void IncrementWavesSurvived()
    {
        m_wavesSurvived++;
    }

    void PlayerDied()
    {
        YouDiedUIOverlay.SetActive(true);
        m_GameState = GameState.GAMEOVER;

        FollowPlayer[] followPlayers = FindObjectsOfType<FollowPlayer>();
        foreach (FollowPlayer followPlayer in followPlayers)
        {
            followPlayer.enabled = false;
        }

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
    void GenerateObstaclesOnField()
    {
        //Place obstacles on the game fields navmesh 
        float numObstacles = 100;
        Vector2 mapWidthHeight = new Vector2(500, 500);

        for (int i = 0; i < numObstacles; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-mapWidthHeight.x / 2, mapWidthHeight.x / 2),
                 0,
                 Random.Range(-mapWidthHeight.y / 2, mapWidthHeight.y / 2));
            Instantiate(obstaclePrefab, position, Quaternion.identity);
        }

        // Rebuild NavMesh to account for new obstacles
        NavMeshSurface surface = FindObjectOfType<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }
}
