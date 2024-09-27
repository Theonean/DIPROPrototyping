using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public float levelTime = 0f;
    public float minutes = 0f;
    public float seconds = 0f;
    private bool timerActive = false;

    private void Update()
    {
        if (timerActive)
        {
            levelTime += Time.deltaTime;
            minutes = Mathf.FloorToInt(levelTime/60);
            seconds = Mathf.FloorToInt(levelTime%60);
        }
    }

    public void SetActive(bool toggle)
    {
        timerActive = toggle;
    }

    public void Reset()
    {
        timerActive = false;
        levelTime = 0f;
    }

    public string GetTime()
    {
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
