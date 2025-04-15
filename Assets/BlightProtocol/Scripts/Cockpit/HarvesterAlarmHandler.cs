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
    private List<GameObject> detectedEnemies = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PL_IsEnemy") && !enemyAlarm)
        {
            detectedEnemies.Add(other.gameObject);
            enemyAlarm = true;
            OnHarvesterAlarm.Invoke();
            StartCoroutine(DisableAlarm());
        }
    }

    private IEnumerator DisableAlarm()
    {
        yield return new WaitForSeconds(alarmDisableTime);
        foreach (GameObject enemy in detectedEnemies)
        {
            if (enemy != null)
            {
                StartCoroutine(DisableAlarm());
                yield break;
            }
        }
        enemyAlarm = false;
        OnHarvesterAlarmDisable.Invoke();
        detectedEnemies.Clear();
    }
}
