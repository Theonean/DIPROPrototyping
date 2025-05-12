using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance { get; private set; }

    private int crystals = 0;
    private Dictionary<string, Dictionary<int, int>> components = new();

    [Header("Debug View (Read-Only)")]
    [SerializeField] private List<CrystalEntry> crystalDebugView = new();
    [SerializeField] private List<ComponentEntry> componentDebugView = new();

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
    }

    public bool RemoveCrystal(int amount)
    {
        if(crystals > amount)
        {
            crystals -= amount;
            return true;
        }
        
        return false;
    }

    public void AddComponent(string componentName, int level, int amount)
    {
        if (level < 1 || level > 5)
        {
            Debug.LogWarning($"Component level {level} is out of allowed range (1-5).");
            return;
        }

        if (!components.ContainsKey(componentName))
            components[componentName] = new Dictionary<int, int>();

        if (!components[componentName].ContainsKey(level))
            components[componentName][level] = 0;

        components[componentName][level] += amount;
        UpdateComponentDebugView();
    }

    private void UpdateComponentDebugView()
    {
        componentDebugView.Clear();
        foreach (var comp in components)
        {
            var entry = new ComponentEntry { name = comp.Key };
            foreach (var level in comp.Value)
            {
                entry.levels.Add(new LevelAmount { level = level.Key, amount = level.Value });
            }
            componentDebugView.Add(entry);
        }
    }
}


//DEBUG CLASSES

[System.Serializable]
public class CrystalEntry
{
    public string name;
    public int amount;
}

[System.Serializable]
public class ComponentEntry
{
    public string name;
    public List<LevelAmount> levels = new List<LevelAmount>();
}

[System.Serializable]
public class LevelAmount
{
    public int level;
    public int amount;
}
