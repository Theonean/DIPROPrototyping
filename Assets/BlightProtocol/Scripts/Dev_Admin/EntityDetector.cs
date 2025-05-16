using UnityEngine;
using UnityEngine.Events;

public class EntityDetector : MonoBehaviour
{
    public UnityEvent OnAgentEnter;
    public UnityEvent OnAgentExit;
    public string layerToLookFor = "Player";

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(layerToLookFor))
        {
            OnAgentEnter.Invoke();
        }
    }
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(layerToLookFor))
        {
            OnAgentExit.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(layerToLookFor))
        {
            OnAgentEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(layerToLookFor))
        {
            OnAgentExit.Invoke();
        }
    }
}
