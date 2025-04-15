using UnityEngine;
using UnityEngine.Events;

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
    private int maxComponentLevel = 5;
    public string DescriptiveName;

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

    void Start()
    {
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
}