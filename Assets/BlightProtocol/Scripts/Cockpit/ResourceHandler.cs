using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Resource
{
    public ResourceData data;
    public ACScreenValueDisplayer displayer;
    public float amount;
    public float maxCapacity = 10000;
    [HideInInspector] public float lastAmount;
    public float delta;
}

public class ResourceHandler : MonoBehaviour
{
    public static ResourceHandler Instance { get; private set; }
    [SerializeField] private Resource[] resources;
    [SerializeField] public Resource fuel;
    public ResourceData fuelResource;

    [Header("Visualization")]
    public UnityEvent<ResourceData, float, float> OnFuelConsumptionChanged;
    [SerializeField] private float consumptionEventInterval = 2f;
    private float timeSinceLastEvent;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        Initialize();
        FindResource(fuelResource, out fuel);
    }

    void Initialize()
    {
        // update displayers
        foreach (var r in resources)
        {
            if (r.displayer != null) {
                r.displayer.SetMaxValue(r.maxCapacity);
                r.displayer.SetValue(r.amount);
            }
            r.lastAmount = r.amount;
        }
    }

    void Update()
    {
        CalculateDeltas();
        // flash display if fuel is being consumed
        HandleFuelEvents();
    }

    void HandleFuelEvents()
    {
        timeSinceLastEvent += Time.deltaTime;
        
        if (timeSinceLastEvent >= consumptionEventInterval)
        {
            if (Mathf.Abs(fuel.delta) > 0.0001f)
            {
                OnFuelConsumptionChanged.Invoke(
                    fuel.data, 
                    fuel.amount, 
                    fuel.delta
                );
            }
            timeSinceLastEvent = 0f;
        }
    }

    void CalculateDeltas()
    {
        foreach (var r in resources)
        {
            r.delta = r.amount - r.lastAmount;
            r.lastAmount = r.amount;
        }
    }

    public float GetAmount(ResourceData data) => 
        FindResource(data, out var r) ? r.amount : 0f;

    public float GetDelta(ResourceData data) => 
        FindResource(data, out var r) ? r.delta : 0f;

    public void Add(ResourceData data, float amount)
    {
        if (!FindResource(data, out var r)) return;
        
        r.amount = Mathf.Min(r.amount + amount, r.maxCapacity);
        UpdateDisplay(r);
    }

    // scripts that call this should always check if there are enough resources
    public void Consume(ResourceData data, float amount, bool displayFlash = false, float duration = 0f)
    {
        if (!FindResource(data, out var r)) return;
        if (displayFlash) {
            OnFuelConsumptionChanged.Invoke(
                    fuel.data, 
                    fuel.amount, 
                    fuel.delta
                );
        }
        if (r.amount <= 0) {
            Logger.Log("Not enough Resources of type: " + r.data.ToString(), LogLevel.WARNING, LogType.RESOURCE);
        }

        if (duration > 0f) 
        {
            StartCoroutine(ConsumeOverTime(r, amount, duration));
        }
        else
        {
            float consumedAmount = Mathf.Min(amount, r.amount);
            r.amount -= consumedAmount;
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
        UpdateDisplay(r);
    }

    void UpdateDisplay(Resource r) => 
        r.displayer.SetValue(r.amount);

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
        
        resource = null;
        return false;
    }
}