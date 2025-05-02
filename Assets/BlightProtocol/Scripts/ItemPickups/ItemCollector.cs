using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class ItemCollector : MonoBehaviour
{
    public float collectorRadius = 40f;
    private Collider[] colliderBuffer = new Collider[128];
    private List<CollectibleItem> itemsFlyingToPlayer = new List<CollectibleItem>();

    private float timeToFadeOutText = 1f;
    private float timeToBundleTogetherCollectedItems = 0.5f; //When another item 
    private float itemRecentlyCollectedCounter = 0f;
    private int sameItemsCollectedInRow = 0;

    [SerializeField] private GameObject collectedItemTextPrefab;
    [SerializeField] private Canvas collectedItemCanvas;

    private SOItem lastItemData;
    private GameObject lastItemCollected;

    private Queue<SOItem> itemQueue = new Queue<SOItem>();
    private bool isDisplayingItem = false;


    void Update()
    {
        if(PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.DRONE)
        {
            collectedItemCanvas.transform.LookAt(Camera.main.transform);

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

        if(itemRecentlyCollectedCounter > 0f)
        {
            itemRecentlyCollectedCounter -= Time.deltaTime;
        }
    }

    public void ItemArrived(CollectibleItem itemCollider)
    {
        CollectibleItem item = itemCollider.GetComponentInParent<CollectibleItem>();
        itemsFlyingToPlayer.Remove(itemCollider);

        SOItem itemData = item.itemData;

        // Add item to inventory
        switch (itemData.itemType)
        {
            case EItemTypes.Crystal:
                ItemManager.Instance.AddCrystal(itemData.prefab.gameObject.name, 1);
                break;
            case EItemTypes.Component:
                ItemManager.Instance.AddComponent(itemData.prefab.gameObject.name, 1, 1);
                break;
        }

        itemQueue.Enqueue(itemData);

        if (!isDisplayingItem)
            StartCoroutine(HandleCollectedItemsDisplay());
    }

private IEnumerator HandleCollectedItemsDisplay()
{
    isDisplayingItem = true;

    while (itemQueue.Count > 0)
    {
        SOItem currentItem = itemQueue.Dequeue();
        int quantity = 1;

        // 🧠 Phase 1: show the item text immediately
        lastItemCollected = Instantiate(collectedItemTextPrefab, collectedItemCanvas.transform);
        RectTransform rect = lastItemCollected.GetComponent<RectTransform>();
        rect.position = transform.position + Vector3.up * 2.5f;

        TextMeshProUGUI text = lastItemCollected.GetComponent<TextMeshProUGUI>();
        text.text = currentItem.name;

        // 🕒 Start bundling window
        itemRecentlyCollectedCounter = timeToBundleTogetherCollectedItems;
        yield return null;

        while (itemRecentlyCollectedCounter > 0f)
        {
            itemRecentlyCollectedCounter -= Time.deltaTime;

            // 👥 Stack more of the same item
            if (itemQueue.Count > 0 && itemQueue.Peek() == currentItem)
            {
                itemQueue.Dequeue();
                quantity++;
                text.text = $"{currentItem.name} x{quantity}";
                itemRecentlyCollectedCounter = timeToBundleTogetherCollectedItems;
            }

            yield return null;
        }

        // 🧠 Phase 2: now start flying up and fading out
        yield return StartCoroutine(FadeOutCollectedItemText(lastItemCollected));
    }

    isDisplayingItem = false;
}



    private IEnumerator FadeOutCollectedItemText(GameObject gameObject)
    {
        float t = 0;
        Vector3 startPos = new Vector3(0, 0, 0);
        Vector3 endPos = new Vector3(0, 20f, 0);

        TextMeshProUGUI collectedItemText = gameObject.GetComponent<TextMeshProUGUI>();

        while (t < timeToFadeOutText)
        {
            gameObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t / timeToFadeOutText);
            collectedItemText.color = Color.Lerp(Color.white, Color.clear, t / timeToFadeOutText);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Clean up
    }

}