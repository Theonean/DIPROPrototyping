using UnityEngine;

public class ResourcePoint : MonoBehaviour
{
    public ResourceData resourceData;
    public float resourceAmount = 100f;
    private bool isAboveGround = false;
    public GameObject aboveGround;
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
        if (other.CompareTag("Harvester")) {
            Destroy(aboveGround);
        }       
    }
}
