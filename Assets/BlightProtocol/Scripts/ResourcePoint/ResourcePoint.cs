using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public ResourceData resourceData;
    public float resourceAmount = 100f;
    
    /// <summary>
    /// Interface to harvest resources from a node
    /// </summary>
    /// <param name="amount">amount of resources to deduct from this node</param>
    /// <returns>returns whether resources can still be harvested from this node</returns>
    public bool HarvestResource(float amount) {
        if(resourceAmount <= 0f)
            return false;

        ResourceHandler.Instance.CollectResource(resourceData, amount);
        resourceAmount -= amount;

        return true;
    }
}
