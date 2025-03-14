using System.Collections;
using System.Collections.Generic;
using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Resource
{
    public ResourceData resourceData;
    public string devDescription;
    public float amount;
    public float maxCapacity = 10000;
    public Slider displaySlider;
}

public class ResourceHandler : MonoBehaviour
{
    public static ResourceHandler Instance { get; private set; }

    [SerializeField] private List<Resource> resources = new List<Resource>(); // Editable in Inspector
    private Dictionary<ResourceData, Resource> resourceDictionary = new Dictionary<ResourceData, Resource>();

    public ResourceData fuelResource;
    // Add other Resources here when new ones are created

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            InitializeResources();
        }
        Debug.LogError("Tie all usages of the fuel resource to the fuelResource variable to maintain SO flexibility");
    }

    void Start()
    {
        UpdateResourceDisplay();
    }
    private void InitializeResources()
    {
        resourceDictionary.Clear();
        foreach (var resource in resources)
        {
            if (resource.resourceData != null)
            {
                if (!resourceDictionary.ContainsKey(resource.resourceData))
                {
                    resourceDictionary.Add(resource.resourceData, resource);
                }
                else
                {
                    Debug.LogWarning("Duplicate resource detected: " + resource.resourceData.displayName);
                }
            }
        }
    }

    public float? CheckResource(ResourceData resourceData)
    {
        if (resourceDictionary.TryGetValue(resourceData, out Resource resource))
        {
            return resource.amount;
        }
        else
        {
            Debug.LogWarning("Resource not found: " + resourceData.displayName);
            return null;
        }
    }

    public void CollectResource(ResourceData resourceData, float amount)
    {
        if (resourceDictionary.TryGetValue(resourceData, out Resource resource))
        {
            if (resource.amount >= resource.maxCapacity)
            {
                Debug.LogWarning(resourceData.displayName + " is full!");
            }
            else
            {
                resource.amount += amount;
                UpdateResourceDisplay();
            }
        }
        else
        {
            Debug.LogWarning("Resource not found: " + resourceData.displayName);
        }
    }

    public void ConsumeResource(ResourceData resourceData, float amount, bool instant, float duration = 1f)
    {
        if (resourceDictionary.TryGetValue(resourceData, out Resource resource))
        {
            if (resource.amount >= amount)
            {
                if (instant)
                {
                    resource.amount -= amount;
                    UpdateResourceDisplay();
                }
                else
                {
                    StartCoroutine(ConsumeResourceOverTime(resource, amount, duration));
                }

            }
            else
            {
                Debug.LogWarning(resourceData.displayName + " is empty or insufficient!");
            }
        }
        else
        {
            Debug.LogWarning("Resource not found: " + resourceData.displayName);
        }
    }

    private IEnumerator ConsumeResourceOverTime(Resource resource, float amount, float duration)
    {
        float timer = 0f;
        float totalSubtracted = 0f;

        while (timer < duration)
        {
            float deltaAmount = amount / duration * Time.deltaTime;
            resource.amount -= deltaAmount;
            totalSubtracted += deltaAmount;

            UpdateResourceDisplay();
            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure exact subtraction
        float correction = amount - totalSubtracted;
        resource.amount -= correction;
        UpdateResourceDisplay();
    }


    public void UpdateResourceDisplay()
    {
        foreach (var resource in resourceDictionary.Values)
        {
            resource.displaySlider.value = resource.amount / resource.maxCapacity;
        }
    }
}
