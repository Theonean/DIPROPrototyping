using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class Gas
{
    public GasData data;
    public UnityEvent<int> amountChanged;
    public float amount;
}

public class GasManager : MonoBehaviour
{
    public static GasManager Instance { get; private set; }
    public Gas[] gasTypes;

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
        // update displayers
        foreach (var gas in gasTypes)
        {
            gas.amountChanged.Invoke(0);
        }
    }
    public float GetGas(GasData data) => 
        FindGas(data, out var gas)? gas.amount : -1f;
    public void AddGas(GasData data, float amount)
    {
        if (!FindGas(data, out var gas)) return;
        
        gas.amount += amount;
        gas.amountChanged.Invoke(Mathf.RoundToInt(amount));
    }

    public bool FindGas(GasData _data, out Gas gas)
    {
        gas = gasTypes.First(x => x.data.Equals(_data));
        if (gas != null) return true;
        else return false;
    }
}