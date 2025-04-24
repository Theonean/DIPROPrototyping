using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum EnergySignatureBaseType
{
    AGGRESSIVE,
    NEUTRAL,
    ARTIFICIAL
}

public class EnergySignatureBase : MonoBehaviour
{
    public EnergySignatureBaseType type;
    public List<Transform> magnitudePositions;
}
