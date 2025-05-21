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
    private int[] researchCostCrystal = { 20, 30, 40, 50 };
    private int[] researchCostComponent = { 5, 10, 15, 20 };
    protected int maxComponentLevel = 5;
    public string DescriptiveName;
    public string componentDescription = "Description here please uwu";
    public string upgradeDescription;
    public UnityEvent<RocketComponentType, int> OnKilledEnemy = new UnityEvent<RocketComponentType, int>();

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
    }

    protected virtual void OnDisable()
    {
        onComponentLevelUp.RemoveListener(SetStatsToLevel);
    }

    protected abstract void SetStatsToLevel();
    public abstract string GetResearchDescription();

    public void LevelUpComponent()
    {
        if (componentLevel >= maxComponentLevel)
        {
            Debug.LogWarning($"Component {DescriptiveName} has reached its maximum level of {maxComponentLevel}.");
            return;
        }

        componentLevel++;
        onComponentLevelUp?.Invoke();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>returns the cost to upgrade at current level (CRYSTAL, COMPONENT)</returns>
    public (int,int) GetResearchCost()
    {
        return (researchCostCrystal[componentLevel], researchCostComponent[componentLevel]);
    }
}