using UnityEngine;
using UnityEngine.Events;

public class EntityDetector : MonoBehaviour
{
    public UnityEvent OnAgentEnter;
    public UnityEvent OnAgentExit;
    public string tagToLookFor;


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToLookFor))
        {
            OnAgentEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagToLookFor))
        {
            OnAgentExit.Invoke();
        }
    }
}
