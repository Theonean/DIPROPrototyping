using System;
using UnityEngine;
using UnityEngine.Events;

public enum RocketComponentType {
    PROPULSION,
    BODY,
    FRONT
}

public abstract class ACRocketComponent : MonoBehaviour
{
    [HideInInspector] public Rocket parentRocket;
    public Rocket ParentRocket
    {
        get
        {
            if (parentRocket == null)
            {
                parentRocket = GetComponentInParent<Rocket>();
            }
            return parentRocket;
        }
    }
    public int componentLevel { get; private set; } = 0;

    [SerializeField] private int unlockCostCrystal = 50;
    [SerializeField] private int[] researchCostCrystal = { 20, 30, 40, 50 };
    public int maxComponentLevel { get; private set; } = 5;
    public string DescriptiveName;
    public string componentDescription = "Description here please uwu";
    public string upgradeDescription;
    public UnityEvent<RocketComponentType, int> OnKilledEnemy = new UnityEvent<RocketComponentType, int>();
    public static UnityEvent<Type, int> OnAnyComponentLevelUp = new UnityEvent<Type, int>();

    protected Vector3 rocketOriginalScale;

    protected Transform rocketTransform
    {
        get { return ParentRocket.transform; }
    }

    protected UnityEvent onComponentLevelUp = new UnityEvent();

    private void Awake()
    {
        if (ParentRocket == null)
        {
            return;
        }
        rocketOriginalScale = ParentRocket.transform.localScale;
    }

    protected virtual void Start()
    {
        componentLevel = ItemManager.Instance.GetItemLevel(DescriptiveName);
        SetStatsToLevel();
    }

    protected virtual void OnEnable()
    {
        onComponentLevelUp.AddListener(SetStatsToLevel);
        OnAnyComponentLevelUp.AddListener(HandleGlobalLevelUp);
    }

    protected virtual void OnDisable()
    {
        onComponentLevelUp.RemoveListener(SetStatsToLevel);
        OnAnyComponentLevelUp.RemoveListener(HandleGlobalLevelUp);
    }

    protected abstract void SetStatsToLevel();
    public abstract string GetResearchDescription();
    public abstract string GetResearchDescription(int customLevel);

    public void LevelUpComponent()
    {
        if (componentLevel >= maxComponentLevel)
        {
            Debug.LogWarning($"Component {DescriptiveName} has reached its maximum level of {maxComponentLevel}.");
            return;
        }

        componentLevel++;
        
        // Fire the global event so *all* ACRocketComponent subclasses hear it
        OnAnyComponentLevelUp.Invoke(GetType(), componentLevel);

        onComponentLevelUp?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>returns the cost to upgrade at current level (CRYSTAL, COMPONENT)</returns>
    public int GetResearchCost()
    {
        if(componentLevel == 0) return unlockCostCrystal;
        else return researchCostCrystal[componentLevel - 1];
    }
    public int GetResearchCost(int customLevel)
    {
        if (customLevel == 0) return unlockCostCrystal;
        else if (customLevel <= maxComponentLevel) return researchCostCrystal[customLevel - 1];
        else return -1;
    }

    /// <summary>
    /// Every component hears all level-up broadcasts,
    /// but only reacts if the Type matches its own concrete subclass.
    /// </summary>
    private void HandleGlobalLevelUp(Type type, int newLevel)
    {
        // ignore upgrades for other component classes
        if (type != GetType())
            return;

        // sync up to the new level
        componentLevel = newLevel;
        SetStatsToLevel();
    }

}