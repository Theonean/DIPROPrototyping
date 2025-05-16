using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public int resourceAmount = 100;
    private float gasHarvested = 0f;

    public bool HarvestResource(float amount)
    {
        if (resourceAmount <= 0f)
            return false;

        gasHarvested += amount;

        if(gasHarvested > 1f)
        {
            int gasInt = (int)gasHarvested;
            gasHarvested -= gasInt;

            ItemManager.Instance.AddGas(gasInt);
            resourceAmount -= gasInt;
        }

        return true;
    }
}