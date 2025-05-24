using UnityEngine;
using UnityEngine.Events;

public class EntityDetector : MonoBehaviour
{
    public UnityEvent OnAgentEnter;
    public UnityEvent OnAgentExit;
    public string layerToLookFor = "Player";

    private void Awake()
    {
        Collider collider = GetComponent<Collider>(); 
        collider.includeLayers = 1 << LayerMask.NameToLayer(layerToLookFor);
        
        /*if (gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
        {
            Debug.LogWarning("Fixed wrong layer [" + gameObject.layer + "] for object using entity detector: " + transform.parent.name + "\\" + name);
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }*/
    }

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
