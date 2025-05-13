using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

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
    [SerializeField] private Sprite crystalSprite, componentSprite;
    [SerializeField] private Color crystalSpriteColor, componentSpriteColor;
    [SerializeField] private Canvas collectedItemCanvas;

    private SOItem lastItemData;
    private GameObject lastItemCollected;

    private Queue<SOItem> itemQueue = new Queue<SOItem>();
    private bool isDisplayingItem = false;


    void Update()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.DRONE)
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
                ItemManager.Instance.AddCrystal(1);
                break;
            case EItemTypes.Component:
                ItemManager.Instance.AddComponent(itemData.itemName, 1);
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
            rect.position = transform.position + Vector3.up * 10f;

            TextMeshProUGUI text = lastItemCollected.GetComponentInChildren<TextMeshProUGUI>();
            Image image = lastItemCollected.GetComponentInChildren<Image>();
            text.text = currentItem.itemName;
            switch (currentItem.itemType)
            {
                case EItemTypes.Crystal:
                    image.sprite = crystalSprite;
                    image.color = crystalSpriteColor;
                break;
                case EItemTypes.Component:
                    image.sprite = componentSprite;
                    image.color = componentSpriteColor;
                    break;
            }


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
                    text.text = $"{currentItem.itemName}x {quantity}";
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
        Vector3 startPos = gameObject.transform.localPosition;

        Vector3 endPos = startPos + Vector3.up * 20;

        TextMeshProUGUI collectedItemText = gameObject.GetComponentInChildren<TextMeshProUGUI>();

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