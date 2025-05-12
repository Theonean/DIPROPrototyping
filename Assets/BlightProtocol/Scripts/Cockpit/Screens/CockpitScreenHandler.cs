using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Parent class for all screens for handling events that affect multiple systems
/// </summary>
public class CockpitScreenHandler : MonoBehaviour
{
    public static CockpitScreenHandler Instance { get; private set; }
    private ACScreenValueDisplayer[] valueDisplayers;

    void Start()
    {
        valueDisplayers = GetComponentsInChildren<ACScreenValueDisplayer>();
    }
}