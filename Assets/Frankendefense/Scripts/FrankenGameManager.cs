using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrankenGameManager : MonoBehaviour
{
    public float zoneRadius = 10f;
    public float zoneWaitTime = 5f;
    float m_zoneTimer = 0f;
    public GameObject MapBoundaries;
    public PlayerDetector playerDetector;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI BestScoreText;
    public GameObject YouDiedUIOverlay;
    Vector3[] m_BoundaryPositions;
    bool m_PlayerInZone = false;
    float m_Points = 0f;
    static float k_BestScore = 0f;
    float m_ContinuousTimeInZone = 0f;
    bool gameActive = true;

    private void Start()
    {
        //Load the two corners of the map boundaries
        m_BoundaryPositions = new Vector3[2];
        m_BoundaryPositions[0] = MapBoundaries.transform.GetChild(0).position;
        m_BoundaryPositions[1] = MapBoundaries.transform.GetChild(1).position;

        if (playerDetector != null)
        {
            playerDetector.OnPlayerDetected.AddListener(() =>
            {
                m_PlayerInZone = true;
                m_ContinuousTimeInZone = 0f;
            });

            playerDetector.OnPlayerLost.AddListener(() =>
            {
                m_PlayerInZone = false;
            });
        }

        PlayerCore playerCore = FindObjectOfType<PlayerCore>();
        playerCore.PlayerDeath.AddListener(() => PlayerDied());

        //Instantly move zone at start of game
        m_zoneTimer = zoneWaitTime;

        //Prepare the UI Overlay so it's not dependent on editor state
        YouDiedUIOverlay.SetActive(false);
        YouDiedUIOverlay.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        m_zoneTimer += Time.deltaTime;
        if (m_zoneTimer >= zoneWaitTime)
        {
            m_zoneTimer = 0f;
            MoveZone();
        }

        if (m_PlayerInZone)
        {
            m_ContinuousTimeInZone += Time.deltaTime;

            m_Points += Time.deltaTime * Mathf.Sqrt(m_ContinuousTimeInZone);
            ScoreText.text = "Score: " + m_Points.ToString("F2");

            //This is hacky, but it works for now
            if (m_Points > k_BestScore)
            {
                k_BestScore = m_Points;
                BestScoreText.text = "Best Score: " + k_BestScore.ToString("F2");
            }
        }

        if (!gameActive)
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
        playerDetector.transform.position = newPosition;
    }

    void PlayerDied()
    {
        m_Points = 0f;
        YouDiedUIOverlay.SetActive(true);
        gameActive = false;

        StartCoroutine(ShowYouDiedUI());
    }

    IEnumerator ShowYouDiedUI()
    {
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime;
            YouDiedUIOverlay.transform.localScale = Vector3.one * time;
            yield return null;
        }
    }
}
