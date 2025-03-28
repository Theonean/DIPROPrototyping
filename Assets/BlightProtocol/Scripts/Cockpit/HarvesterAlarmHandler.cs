using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HarvesterAlarmHandler : MonoBehaviour
{
    public static HarvesterAlarmHandler Instance;
    public UnityEvent OnHarvesterAlarm;
    public UnityEvent OnHarvesterAlarmDisable;
    public bool enemyAlarm;
    public float alarmDisableTime = 10f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else {
            Instance = this;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy") && !enemyAlarm) {
            enemyAlarm = true;
            OnHarvesterAlarm.Invoke();
            StartCoroutine(DisableAlarm());
        }
    }

    private IEnumerator DisableAlarm() {
        yield return new WaitForSeconds(alarmDisableTime);
        OnHarvesterAlarmDisable.Invoke();
    }
}
