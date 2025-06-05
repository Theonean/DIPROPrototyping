using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public class EnergySignature : MonoBehaviour
{
    [SerializeField] public GameObject pingPrefab;
    public int eMagnitude;
    public int areaSize;
    public EnergySignatureDisplayer displayer;

    public UnityEvent onDisable;

    void OnDestroy()
    {
        onDisable.Invoke();
    }

    public void FlashSignature()
    {
        if (displayer)
        {
            displayer.FlashSignature();
        }
    }

    public void DestroyWithDelay(float delay)
    {
        StartCoroutine(DestroyWithDelayCoroutine(delay));
    }

    private IEnumerator DestroyWithDelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }
}