using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class EnergySignature : MonoBehaviour
{
    public EnergySignatureBaseType baseType;
    public int eMagnitude;
    public int areaSize;
    public EnergySignatureDisplayer displayer;

    public UnityEvent onDestroy;

    void OnDestroy() {
        onDestroy.Invoke();
    }

    public void FlashSignature()
    {
        if(displayer)
        {
            displayer.FlashSignature();
        }
    }
}