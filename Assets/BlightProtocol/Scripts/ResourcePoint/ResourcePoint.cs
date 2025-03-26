using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public ResourceData resourceData;
    public float resourceAmount = 100f;
    private bool isAboveGround = false;
    public GameObject aboveGround;

    /// <summary>
    /// Interface to harvest resources from a node
    /// </summary>
    /// <param name="amount">amount of resources to deduct from this node</param>
    /// <returns>returns whether resources can still be harvested from this node</returns>
    /// 
    void Start()
    {
        isAboveGround = Random.value > 0.5f;
        aboveGround.SetActive(isAboveGround);     
    }
    public bool HarvestResource(float amount) {
        if(resourceAmount <= 0f)
            return false;

        ResourceHandler.Instance.CollectResource(resourceData, amount);
        resourceAmount -= amount;

        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        Logger.Log(other.name, LogLevel.INFO, LogType.HARVESTER);
        if (other.CompareTag("Harvester")) {
            Destroy(aboveGround);
        }       
    }
}
