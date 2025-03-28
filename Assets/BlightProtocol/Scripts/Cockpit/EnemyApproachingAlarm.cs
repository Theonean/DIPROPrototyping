using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyApproachingAlarm : MonoBehaviour
{
    public Renderer bulbRenderer;

    void Start()
    {
        HarvesterAlarmHandler.Instance.OnHarvesterAlarm.AddListener(EnableAlarm);
    }

    private void EnableAlarm()
    {
        bulbRenderer.material.SetFloat("_Enabled", 1);
    }

    private void DisableAlarm() {
        bulbRenderer.material.SetFloat("_Enabled", 0);
    }
}
