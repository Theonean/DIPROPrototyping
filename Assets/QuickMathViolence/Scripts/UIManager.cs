using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Win Condition")]
    public TextMeshProUGUI target;
    public TextMeshProUGUI progress;
    public int progressGoal = 0;
    public GameObject winScreen;
    public GameObject loseScreen;

    [Header("Timer")]
    public LevelTimer levelTimer;
    public TextMeshProUGUI timerText;
    public Slider slider;

    private void Awake()
    {
        timerText.text = levelTimer.GetTime();
        slider.maxValue = levelTimer.levelMaxTime;
        slider.value = levelTimer.levelTimer;
    }


    public void SetTarget(int targetValue)
    {
        target.text = targetValue.ToString();
    }

    public void SetProgress(int progressValue)
    {
        progress.text = progressValue.ToString() + "/" + progressGoal.ToString();
    }

    private void Update()
    {
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (levelTimer.timerActive)
        {
            timerText.text = levelTimer.GetTime();
            slider.value = levelTimer.levelTimer;
        }
    }

    public void DisplayWin()
    {
        winScreen.SetActive(true);
    }

    public void DisplayLose()
    {
        loseScreen.SetActive(true);
    }
}
