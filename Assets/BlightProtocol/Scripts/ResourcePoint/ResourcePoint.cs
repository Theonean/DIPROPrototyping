using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public ResourceData resourceData;
    public float resourceAmount = 100f;

    public bool HarvestResource(float amount)
    {
        if (resourceAmount <= 0f)
            return false;

        ResourceHandler.Instance.Add(resourceData, amount);
        resourceAmount -= amount;

        return true;
    }
}
