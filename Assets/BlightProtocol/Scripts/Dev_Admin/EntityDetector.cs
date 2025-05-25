using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;

public class EntityDetector : MonoBehaviour
{
    public UnityEvent OnAgentEnter;
    public UnityEvent OnAgentExit;

    [Tooltip("When true, only GameObjects on the single `layerToLookFor` string are detected.")]
    public bool detectSingularAgent = true;

    [Tooltip("Name of the layer to look for when `detectSingularAgent` is true.")]
    public string layerToLookFor = "Player";

    [Tooltip("Mask of layers to look for when `detectSingularAgent` is false.")]
    public LayerMask layersToLookFor;

    private void Awake()
    {
        // Restrict this collider so it only fires triggers/collisions against the chosen layer
        var col = GetComponent<Collider>();
        int targetLayer = LayerMask.NameToLayer(layerToLookFor);
        col.includeLayers = 1 << targetLayer;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (CheckGameobjectLayer(collision.gameObject))
            OnAgentEnter.Invoke();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (CheckGameobjectLayer(collision.gameObject))
            OnAgentExit.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (CheckGameobjectLayer(other.gameObject))
            OnAgentEnter.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (CheckGameobjectLayer(other.gameObject))
            OnAgentExit.Invoke();
    }

    /// <summary>
    /// Returns true if the given GameObject should be considered "the agent"
    /// according to either the single-layer name (when detectSingularAgent is true)
    /// or the LayerMask (when detectSingularAgent is false).
    /// </summary>
    private bool CheckGameobjectLayer(GameObject go)
    {
        int targetLayer = LayerMask.NameToLayer(layerToLookFor);

        // if we're only looking for the single named layer
        if (detectSingularAgent)
        {
            return go.layer == targetLayer;
        }

        // otherwise check the LayerMask bitfield
        return ((layersToLookFor.value >> go.layer) & 1) == 1;
    }
}
