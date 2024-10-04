using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public float levelMaxTime = 0f;
    public float levelTimer;
    public float minutes = 0f;
    public float seconds = 0f;
    public bool timerActive = false;

    public PlayerMovement pm;
    public UIManager uiManager;
    private bool gameStarted = false;

    private void Awake()
    {
        levelTimer = levelMaxTime;
    }
    private void Update()
    {
        if (timerActive)
        {
            levelTimer -= Time.deltaTime;
            minutes = Mathf.FloorToInt(levelTimer/60);
            seconds = Mathf.FloorToInt(levelTimer%60);

            if (levelTimer <= 0)
            {
                uiManager.DisplayLose();
                SetActive(false);
            }
        }
        if ((pm.horizontalInput > 0 || pm.verticalInput > 0) && !gameStarted)
        {
            gameStarted = true;
            SetActive(true);
        }

    }

    public void SetActive(bool toggle)
    {
        timerActive = toggle;
    }

    public void Reset()
    {
        timerActive = false;
        levelTimer = levelMaxTime;
    }

    public string GetTime()
    {
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
