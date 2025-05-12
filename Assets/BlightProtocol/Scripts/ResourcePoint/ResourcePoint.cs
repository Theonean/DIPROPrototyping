using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public ResourceData resourceData;
    public float resourceAmount = 100f;
    public GameObject aboveGround;

    public bool HarvestResource(float amount)
    {
        if (resourceAmount <= 0f)
            return false;

        ResourceHandler.Instance.Add(resourceData, amount);
        resourceAmount -= amount;

        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PL_IsHarvester"))
        {
            if (aboveGround != null)
            {
                aboveGround.GetComponent<ItemDropper>().DropItems();
            }
        }
    }
}
