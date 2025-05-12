using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public GasData resourceData;
    public float resourceAmount = 100f;

    public bool HarvestResource(float amount)
    {
        if (resourceAmount <= 0f)
            return false;

        GasManager.Instance.AddGas(resourceData, amount);
        resourceAmount -= amount;

        return true;
    }
}
