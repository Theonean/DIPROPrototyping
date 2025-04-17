using System.Collections;
using UnityEngine;

[System.Serializable]
public class Resource
{
    public ResourceData data;
    public ScreenType screenType;
    public float amount;
    public float maxCapacity = 10000;
    [HideInInspector] public float lastAmount;
    [HideInInspector] public float delta;
    [HideInInspector] public bool wasFullLastCheck;
    [HideInInspector] public bool wasEmptyLastCheck;
}

public class ResourceHandler : MonoBehaviour
{
    public static ResourceHandler Instance { get; private set; }

    [SerializeField] private Resource[] resources;
    public ResourceData fuelResource;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        Initialize();
    }

    void Initialize()
    {
        foreach (var r in resources)
        {
            CockpitScreenHandler.Instance.SetMaxValue(r.screenType, r.maxCapacity);
            CockpitScreenHandler.Instance.SetValue(r.screenType, r.amount);
            r.lastAmount = r.amount;
            r.wasFullLastCheck = (r.amount >= r.maxCapacity);
            r.wasEmptyLastCheck = (r.amount <= 0);
        }
    }

    void Update() => CalculateDeltas();

    void CalculateDeltas()
    {
        foreach (var r in resources)
        {
            r.delta = r.amount - r.lastAmount;
            r.lastAmount = r.amount;

            // Check capacity warnings
            if (r.amount >= r.maxCapacity && !r.wasFullLastCheck)
            {
                Logger.Log($"{r.data.displayName} reached maximum capacity!", 
                          LogLevel.WARNING, 
                          LogType.RESOURCE);
                r.wasFullLastCheck = true;
            }
            else if (r.amount < r.maxCapacity)
            {
                r.wasFullLastCheck = false;
            }

            // Check empty warnings (only for fuel)
            if (r.data == fuelResource && r.amount <= 0 && !r.wasEmptyLastCheck)
            {
                Logger.Log("Fuel depleted!", 
                          LogLevel.WARNING, 
                          LogType.RESOURCE);
                r.wasEmptyLastCheck = true;
            }
            else if (r.amount > 0)
            {
                r.wasEmptyLastCheck = false;
            }
        }
    }

    public float GetAmount(ResourceData data) => 
        FindResource(data, out var r) ? r.amount : 0f;

    public float GetDelta(ResourceData data) => 
        FindResource(data, out var r) ? r.delta : 0f;

    public void Add(ResourceData data, float amount)
    {
        if (!FindResource(data, out var r)) return;
        
        float newAmount = Mathf.Min(r.amount + amount, r.maxCapacity);
        if (newAmount == r.maxCapacity && r.amount != r.maxCapacity)
        {
            Logger.Log($"{data.displayName} reached maximum capacity!", 
                      LogLevel.WARNING, 
                      LogType.RESOURCE);
        }
        r.amount = newAmount;
        UpdateDisplay(r);
    }

    public void Consume(ResourceData data, float amount, float duration = 0f)
    {
        if (!FindResource(data, out var r)) return;
        
        if (r.amount <= 0)
        {
            if (data == fuelResource)
            {
                Logger.Log("Attempted to consume fuel when empty!", 
                          LogLevel.WARNING, 
                          LogType.RESOURCE);
            }
            return;
        }

        if (duration > 0f) 
        {
            StartCoroutine(ConsumeOverTime(r, amount, duration));
        }
        else
        {
            r.amount = Mathf.Max(0, r.amount - amount);
            if (r.amount <= 0 && data == fuelResource)
            {
                Logger.Log("Fuel depleted!", 
                          LogLevel.WARNING, 
                          LogType.RESOURCE);
            }
            UpdateDisplay(r);
        }
    }

    IEnumerator ConsumeOverTime(Resource r, float amount, float duration)
    {
        float startAmount = r.amount;
        float endAmount = Mathf.Max(0, startAmount - amount);
        float timer = 0f;

        while (timer < duration)
        {
            r.amount = Mathf.Lerp(startAmount, endAmount, timer/duration);
            UpdateDisplay(r);
            timer += Time.deltaTime;
            yield return null;
        }

        r.amount = endAmount;
        if (r.amount <= 0 && r.data == fuelResource)
        {
            Logger.Log("Fuel depleted during gradual consumption!", 
                      LogLevel.WARNING, 
                      LogType.RESOURCE);
        }
        UpdateDisplay(r);
    }

    void UpdateDisplay(Resource r) => 
        CockpitScreenHandler.Instance.SetValue(r.screenType, r.amount);

    bool FindResource(ResourceData data, out Resource resource)
    {
        foreach (var r in resources)
        {
            if (r.data == data)
            {
                resource = r;
                return true;
            }
        }
        
        Logger.Log($"Resource not found: {data.displayName}", 
                  LogLevel.WARNING, 
                  LogType.RESOURCE);
        resource = null;
        return false;
    }

    public Resource GetFuelResource()
    {
        if (!FindResource(fuelResource, out var r))
        {
            Logger.Log("Fuel resource reference is missing!", 
                      LogLevel.ERROR, 
                      LogType.RESOURCE);
        }
        return r;
    }
}