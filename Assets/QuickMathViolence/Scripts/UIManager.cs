using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI target;
    public TextMeshProUGUI progress;

    public int progressGoal = 0;

    public void SetTarget(int targetValue)
    {
        target.text = targetValue.ToString();
    }

    public void SetProgress(int progressValue)
    {
        progress.text = progressValue.ToString() + "/" + progressGoal.ToString();
    }
}
