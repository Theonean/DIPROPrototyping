using System.Collections.Generic;
using UnityEngine;

public enum ScreenType
{
    GAS,
    HEALTH,
    SEISMOGRAPH,
    SPEED
}

public class CockpitScreenHandler : MonoBehaviour
{
    public static CockpitScreenHandler Instance { get; private set; }

    private Dictionary<ScreenType, ScreenValueDisplayer> _displayers = new Dictionary<ScreenType, ScreenValueDisplayer>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        ScreenValueDisplayer[] displayers = gameObject.GetComponentsInChildren<ScreenValueDisplayer>();
        foreach (ScreenValueDisplayer displayer in displayers) {
            RegisterDisplayer(displayer.screenType, displayer);
            Debug.Log(displayer.screenType);
        }
    }

    public void RegisterDisplayer(ScreenType screenType, ScreenValueDisplayer displayer)
    {
        if (!_displayers.ContainsKey(screenType))
        {
            _displayers.Add(screenType, displayer);
        }
        else
        {
            Logger.Log($"Displayer for {screenType} already registered!", LogLevel.WARNING, LogType.COCKPIT);
        }
    }

    public void UnregisterDisplayer(ScreenType screenType)
    {
        if (_displayers.ContainsKey(screenType))
        {
            _displayers.Remove(screenType);
        }
    }

    public void SetValue(ScreenType screenType, float value, float duration = -1)
    {
        if (_displayers.TryGetValue(screenType, out var displayer))
        {
            displayer.SetValue(value, duration);
        }
        else
        {
            Logger.Log($"No displayer found for {screenType}", LogLevel.WARNING, LogType.COCKPIT);
        }
    }

    public void SetMaxValue(ScreenType screenType, float maxValue)
    {
        if (_displayers.TryGetValue(screenType, out var displayer))
        {
            displayer.SetMaxValue(maxValue);
        }
        else
        {
            Logger.Log($"No displayer found for {screenType}", LogLevel.WARNING, LogType.COCKPIT);
        }
    }
}