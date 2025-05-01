using UnityEngine;
using System.Collections.Generic;

public class ItemCollector : MonoBehaviour
{
    public float collectorRadius = 40f;
    private Collider[] colliderBuffer = new Collider[128];
    private List<CollectibleItem> itemsFlyingToPlayer = new List<CollectibleItem>();
    public List<SOItem> collectedItems = new List<SOItem>();

    void Update()
    {
        if(PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.DRONE)
        {
            int foundColliders = Physics.OverlapSphereNonAlloc(transform.position, collectorRadius, colliderBuffer);
            if (foundColliders <= 0) return;

            for (int i = 0; i < foundColliders; i++)
            {
                Collider other = colliderBuffer[i];
                CollectibleItem possiblyCollectableItem = other.GetComponentInParent<CollectibleItem>();
                if (possiblyCollectableItem && !itemsFlyingToPlayer.Contains(possiblyCollectableItem))
                {
                    itemsFlyingToPlayer.Add(possiblyCollectableItem);
                    possiblyCollectableItem.arrivedAtPlayer.AddListener(ItemArrived);
                    possiblyCollectableItem.StartflyingToPlayer();
                }
            }
        }
    }

    public void ItemArrived(CollectibleItem itemCollider)
    {
        CollectibleItem item = itemCollider.GetComponentInParent<CollectibleItem>();
        collectedItems.Add(item.itemData);
        itemsFlyingToPlayer.Remove(itemCollider);

        SOItem itemData = item.itemData;

        switch (itemData.itemType)
        {
            case EItemTypes.Crystal:
                ItemManager.Instance.AddCrystal(itemData.prefab.gameObject.name, 1);
                break;
            case EItemTypes.Component:
                ItemManager.Instance.AddComponent(itemData.prefab.gameObject.name, 1, 1);
                break;
        }
    }

}