using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public ResourceData resourceData;
    public float resourceAmount = 100f;
    
    public void HarvestResource(float amount) {
        ResourceHandler.Instance.CollectResource(resourceData, amount);
        resourceAmount -= amount;
    }
}
