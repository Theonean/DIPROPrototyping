using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private int crystals = 20;
    public ComponentEntry[] components = new ComponentEntry[0];
    public UnityEvent<int> crystalAmountChanged;
    public UnityEvent notEnoughCrystals;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public int GetCrystal()
    { return crystals; }

    public void AddCrystal(int amount)
    {
        crystals += amount;
        crystalAmountChanged.Invoke(amount);
    }

    public bool RemoveCrystal(int amount)
    {
        if(crystals >= amount)
        {
            crystals -= amount;
            crystalAmountChanged.Invoke(-amount);
            return true;
        }
        
        notEnoughCrystals.Invoke();
        return false;
    }

    public void AddComponent(string componentName, int amount)
    {
        ComponentEntry entry = components.Where(Component => Component.name == componentName).FirstOrDefault();
        if(entry == null)
        {
            entry = new ComponentEntry(componentName);
        }

        entry.amountHeld += amount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="componentName"></param>
    /// <param name="amount"></param>
    /// <returns>Whether removing was succesfull, false when not enough components left</returns>
    public bool RemoveComponent(string componentName, int amount)
    {
        ComponentEntry entry = components.Where(Component => Component.name == componentName).FirstOrDefault();
        if(entry == null)
        {
            Debug.LogError("Removing component that cannot be found error: " + componentName);
        }
        
        if(amount > entry.amountHeld)
        {
            return false;
        }
        else
        {
            entry.amountHeld -= amount;
            return true;
        }
    }
}

[System.Serializable]
public class ComponentEntry
{
    public string name;
    public int amountHeld;
    public int highestLevelUpgraded;

    public ComponentEntry(string name)
    {
        this.name = name;
        amountHeld = 0;
        highestLevelUpgraded = 1;
    }
}