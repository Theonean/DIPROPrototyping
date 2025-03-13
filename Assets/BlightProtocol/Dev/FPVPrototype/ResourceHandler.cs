using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Resource
{
    public ResourceData resourceData;
    public float amount;
    public float maxCapacity = 10000;
    public Slider displaySlider;
}

public class ResourceHandler : MonoBehaviour
{
    public static ResourceHandler Instance { get; private set; }

    [SerializeField] private List<Resource> resources = new List<Resource>(); // Editable in Inspector
    private Dictionary<ResourceData, Resource> resourceDictionary = new Dictionary<ResourceData, Resource>();

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

    public void ConsumeResource(ResourceData resourceData, float amount)
    {
        if (resourceDictionary.TryGetValue(resourceData, out Resource resource))
        {
            if (resource.amount >= amount)
            {
                resource.amount -= amount;
                UpdateResourceDisplay();
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

    public void UpdateResourceDisplay()
    {
        foreach (var resource in resourceDictionary.Values)
        {
            resource.displaySlider.value = resource.amount / resource.maxCapacity;
        }
    }
}
