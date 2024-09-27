using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Win Condition")]
    public TextMeshProUGUI target;
    public TextMeshProUGUI progress;
    public int progressGoal = 0;

    [Header("Timer")]
    public LevelTimer levelTimer;
    public TextMeshProUGUI timerText;

    

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
        timerText.text = levelTimer.GetTime();
    }
}
