using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    public int crystals = 40;
    public int totalCrystalsCollected = 0;
    public int gas = 0;
    public List<ComponentEntry> components = new List<ComponentEntry>();
    public UnityEvent<int> crystalAmountChanged;
    public UnityEvent<int> gasAmountChanged;
    public UnityEvent notEnoughCrystals;
    public UnityEvent notEnoughGas;
    public UnityEvent<string> onNewComponentUnlocked;
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
        }

        components.Add(new ComponentEntry("BouncingFront", true));
        components.Add(new ComponentEntry("ExplosiveBody", true));
        components.Add(new ComponentEntry("DirectlinePropulsion", true));

        components.Add(new ComponentEntry("ShrapnelshotFront"));
        components.Add(new ComponentEntry("ImplosionBody"));
        components.Add(new ComponentEntry("ArcingPropulsion"));

        components.Add(new ComponentEntry("PenetrativeFront"));
        components.Add(new ComponentEntry("ElectricityBody"));
        components.Add(new ComponentEntry("BoomerangPropulsion"));
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.I))
        {
            FREEMONEYMODEENGAGED = !FREEMONEYMODEENGAGED;
        }
    }

    #region Crystal Management
    public int GetCrystal()
    { return crystals; }

    public void AddCrystal(int amount)
    {
        crystals += amount;
        totalCrystalsCollected += amount;
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
    #endregion

    #region Gas Management
    public int GetGas()
    {
        return gas;
    }

    public void AddGas(int amount) 
    { 
        gas += amount; 
        gasAmountChanged.Invoke(amount);
    }

    public bool RemoveGas(int amount)
    {
        if (gas >= amount || FREEMONEYMODEENGAGED)
        {
            gas -= amount;
            gasAmountChanged.Invoke(-amount);
            return true;
        }

        notEnoughGas.Invoke();
        return false;
    }
    #endregion

    #region Component Management
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

    public bool HasComponent(string componentName)
    {
        ComponentEntry entry = components.Where(Component => Component.name == componentName).FirstOrDefault();
        return entry != null;
    }

    private ComponentEntry GetComponentEntry(string componentName)
    {
        ComponentEntry entry = components.Where(Component => Component.name == componentName).FirstOrDefault();
        if (entry == null)
        {
            entry = new ComponentEntry(componentName);
            onNewComponentUnlocked.Invoke(componentName);
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
    #endregion
}

[System.Serializable]
public class ComponentEntry
{
    public string name;
    public int amountHeld;
    public int highestLevelUpgraded;
    public bool isUnlocked;

    public ComponentEntry(string name, bool unlockedFromStart = false)
    {
        this.name = name;
        amountHeld = 0;
        highestLevelUpgraded = 0;
        isUnlocked = unlockedFromStart;
    }
}