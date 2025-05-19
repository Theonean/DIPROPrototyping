using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class ItemCollector : MonoBehaviour
{
    private class ActiveItemDisplay
    {
        public GameObject displayObject;
        public SOItem itemData;
        public int quantity = 1;
        public float bundleTimer;
        public TextMeshProUGUI text;
    }

    private List<ActiveItemDisplay> activeDisplays = new List<ActiveItemDisplay>();

    public float collectorRadius = 40f;
    private Collider[] colliderBuffer = new Collider[128];
    private List<CollectibleItem> itemsFlyingToPlayer = new List<CollectibleItem>();

    private float timeToFadeOutText = 1f;
    private float timeToBundleTogetherCollectedItems = 0.5f; //When another item 

    [SerializeField] private GameObject collectedItemTextPrefab;
    [SerializeField] private Sprite crystalSprite, componentSprite;
    [SerializeField] private Color crystalSpriteColor, componentSpriteColor;
    [SerializeField] private Canvas collectedItemCanvas;

    private Queue<(SOItem item, bool isFirstTime)> itemQueue = new();
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
        bool isFirstTime = false;

        switch (itemData.itemType)
        {
            case EItemTypes.Crystal:
                ItemManager.Instance.AddCrystal(1);
                break;

            case EItemTypes.Component:
                if (!ItemManager.Instance.HasComponent(itemData.itemName))
                {
                    isFirstTime = true;
                }

                ItemManager.Instance.AddComponent(itemData.itemName, 1);
                break;
        }

        itemQueue.Enqueue((itemData, isFirstTime));


        if (!isDisplayingItem)
            StartCoroutine(HandleCollectedItemsDisplay());
    }

    private IEnumerator HandleCollectedItemsDisplay()
    {
        while (itemQueue.Count > 0)
        {
            var (newItem, isFirstTime) = itemQueue.Dequeue();

            ActiveItemDisplay existing = activeDisplays.Find(d => d.itemData == newItem);
            if (existing != null)
            {
                existing.quantity++;
                existing.text.text = $"{newItem.itemName} x{existing.quantity}";
                existing.bundleTimer = timeToBundleTogetherCollectedItems;
            }
            else
            {
                GameObject prefabToUse = collectedItemTextPrefab;
                GameObject go = Instantiate(prefabToUse, collectedItemCanvas.transform);
                RectTransform rect = go.GetComponent<RectTransform>();

                TextMeshProUGUI text = go.GetComponentInChildren<TextMeshProUGUI>(false);
                Image image = go.GetComponentInChildren<Image>();
                text.text = newItem.itemName;

                switch (newItem.itemType)
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

                if (isFirstTime)
                {
                    Image firstHighlightImage = image.transform.GetChild(0).GetComponent<Image>();
                    firstHighlightImage.enabled = true;
                }

                ActiveItemDisplay newDisplay = new ActiveItemDisplay
                {
                    displayObject = go,
                    itemData = newItem,
                    quantity = 1,
                    bundleTimer = isFirstTime ? 2.5f : timeToBundleTogetherCollectedItems,
                    text = text
                };

                activeDisplays.Insert(0, newDisplay);

                float baseOffset = -13f;
                float verticalSpacing = 2f;
                for (int i = 0; i < activeDisplays.Count; i++)
                {
                    RectTransform r = activeDisplays[i].displayObject.GetComponent<RectTransform>();
                    r.localPosition = Vector3.up * (baseOffset + i * verticalSpacing);
                }

                StartCoroutine(FadeOutAndRemove(newDisplay, isFirstTime));
            }
        }

        yield return null;
    }

    private IEnumerator FadeOutAndRemove(ActiveItemDisplay display, bool isFirstTime)
    {
        float displayDuration = isFirstTime ? 3f : timeToFadeOutText;

        while (display.bundleTimer > 0f)
        {
            display.bundleTimer -= Time.deltaTime;
            yield return null;
        }

        float t = 0f;
        Vector3 startPos = display.displayObject.transform.localPosition;
        Vector3 endPos = startPos + Vector3.up * 20f;

        TextMeshProUGUI text = display.displayObject.GetComponentInChildren<TextMeshProUGUI>();

        while (t < displayDuration)
        {
            display.displayObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t / displayDuration);
            text.color = Color.Lerp(Color.white, Color.clear, t / displayDuration);
            t += Time.deltaTime;
            yield return null;
        }

        Destroy(display.displayObject);
        activeDisplays.Remove(display);
    }

}