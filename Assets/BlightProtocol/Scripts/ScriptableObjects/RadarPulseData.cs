using UnityEngine;

[CreateAssetMenu(fileName = "RadarData", menuName = "RadarData", order = 0)]
public class RadarPulseData : ScriptableObject
{
    [Header("Pulse")]
    public int pulseCost = 5;
    public float pulseDuration = 1.0f;
    public float pulseRange = 500f;
    public float pulseStartRange = 10f;
    public float pulseSpeed = 100f;
}