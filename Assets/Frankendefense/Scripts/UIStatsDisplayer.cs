using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStatsDisplayer : MonoBehaviour
{
    public static UIStatsDisplayer Instance { get; private set; }

    public TextMeshProUGUI explosionRangeNumber;
    public TextMeshProUGUI shotspeedNumber;
    private void Awake()
    {
        // Ensure there's only one instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void IncrementExplosionRange()
    {
        explosionRangeNumber.text = (float.Parse(explosionRangeNumber.text) + 1f).ToString();
    }

    public void IncrementShotSpeed()
    {
        shotspeedNumber.text = (float.Parse(shotspeedNumber.text) + 1f).ToString();
    }
}
