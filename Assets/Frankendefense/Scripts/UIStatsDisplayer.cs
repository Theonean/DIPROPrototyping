using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;

public class UIStatsDisplayer : MonoBehaviour
{
    public static UIStatsDisplayer Instance { get; private set; }

    public TextMeshProUGUI explosionRangeNumber;
    public TextMeshProUGUI shotspeedNumber;
    public LegHandler legInstance;
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

        //Set initial value for Shot speed and explosion range from leghandler
        explosionRangeNumber.text = legInstance.explosionRadius.ToString() + "m";
        shotspeedNumber.text = legInstance.legFlySpeed.ToString() + "m/s";

    }

    public void UpdateUIExplosionRange()
    {
        string temp = legInstance.explosionRadius.ToString() + "m";
        explosionRangeNumber.text = temp;
    }

    public void UpdateUIShotSpeed()
    {
        string temp = legInstance.legFlySpeed.ToString() + "m/s";
        shotspeedNumber.text = temp;
    }
}
