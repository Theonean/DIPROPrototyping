using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public int crystals = 40;
    public List<ComponentEntry> components = new List<ComponentEntry>();
    public UnityEvent<int> crystalAmountChanged;
    public UnityEvent notEnoughCrystals;
    private bool FREEMONEYMODEENGAGED = false;

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

        components.Add(new ComponentEntry("PenetrativeFront"));
        components.Add(new ComponentEntry("ExplosiveBody"));
        components.Add(new ComponentEntry("DirectlinePropulsion"));
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.I))
        {
            FREEMONEYMODEENGAGED = !FREEMONEYMODEENGAGED;
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
        if(crystals >= amount || FREEMONEYMODEENGAGED)
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
        ComponentEntry entry = GetComponentEntry(componentName);

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
        
        if(entry.amountHeld > crystals || FREEMONEYMODEENGAGED)
        {
            entry.amountHeld -= amount;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void IncreaseItemLevel(string componentName)
    {
        ComponentEntry entry = GetComponentEntry(componentName);

        entry.highestLevelUpgraded++;
        Debug.Log("Upgraded component " + componentName);
    }

    /// <summary>
    /// Get highest reached upgrade level for a specific component
    /// </summary>
    /// <param name="componentName"></param>
    /// <returns>Item level of associated component, -1 when no component found </returns>
    public int GetItemLevel(string componentName)
    {
        ComponentEntry entry = GetComponentEntry(componentName);
        return entry.highestLevelUpgraded;
    }

    public int GetComponentAmount(string componentName)
    {
        ComponentEntry entry = GetComponentEntry(componentName);
        return entry.amountHeld;
    }

    private ComponentEntry GetComponentEntry(string componentName)
    {
        ComponentEntry entry = components.Where(Component => Component.name == componentName).FirstOrDefault();
        if (entry == null)
        {
            entry = new ComponentEntry(componentName);
            components.Add(entry);
        }
        return entry;
    }

    public List<ACRocketComponent> GetAvailableComponents(RocketComponentType componentType)
    {
        List<ACRocketComponent> allComponentsOfType= new List<ACRocketComponent>();
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                allComponentsOfType = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketPropulsions
                    .Select(x => x.GetComponent<ACRocketComponent>()).ToList();
                break;
            case RocketComponentType.BODY:
                allComponentsOfType = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketBodies
                    .Select(x => x.GetComponent<ACRocketComponent>()).ToList();
                break;
            case RocketComponentType.FRONT:
                allComponentsOfType = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketFronts
                    .Select(x => x.GetComponent<ACRocketComponent>()).ToList();
                break;
        }

        string[] ownedComponentNames = components.Select( component => component.name).ToArray();
        List<ACRocketComponent> ownedComponents = allComponentsOfType.Where(component => ownedComponentNames.Contains(component.DescriptiveName)).ToList();

        return ownedComponents;
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
        highestLevelUpgraded = 0;
    }
}