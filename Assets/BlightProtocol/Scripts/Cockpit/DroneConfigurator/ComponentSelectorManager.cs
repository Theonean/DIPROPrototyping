using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class ComponentSelectorManager : MonoBehaviour
{
    public static ComponentSelectorManager Instance { get; private set; }
    [SerializeField] private DroneConfigurator droneConfigurator;
    [SerializeField] private RotatingSelectedRocketManager selectedRocketManager;
    [SerializeField] private Button applyToAllButton;
    [SerializeField] public RocketComponentSelector frontSelector, bodySelector, propSelector;
    [SerializeField] private ResearchManager[] researchManagers = new ResearchManager[3];
    [Header("Screens")]
    [SerializeField] private Button showDescriptionButton;
    [SerializeField] private ConfiguratorDummyRocket screenDummyRocket;
    [SerializeField] private ComponentDescriptionDisplayer descriptionDisplayer;
    private bool descriptionVisible = true;

    public UnityEvent OnComponentSelectionChanged = new UnityEvent();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        ToggleDescription();
    }

    void OnEnable()
    {
        
        ConnectSelector(frontSelector);
        ConnectSelector(bodySelector);
        ConnectSelector(propSelector);
        

        ResearchManager.OnResearched.AddListener(selectedRocketManager.LevelUpComponent);

        droneConfigurator.onModeSwitched.AddListener(OnModeSwitched);

        applyToAllButton.OnPressed.AddListener(ApplySelectionToAll);

        showDescriptionButton.OnPressed.AddListener(ToggleDescription);
    }

    void OnDisable()
    {

        DisconnectSelector(frontSelector);
        DisconnectSelector(bodySelector);
        DisconnectSelector(propSelector);

        ResearchManager.OnResearched.RemoveListener(selectedRocketManager.LevelUpComponent);

        droneConfigurator.onModeSwitched.RemoveListener(OnModeSwitched);

        applyToAllButton.OnPressed.RemoveListener(ApplySelectionToAll);

        showDescriptionButton.OnPressed.RemoveListener(ToggleDescription);
    }

    private void ConnectSelector(RocketComponentSelector selector)
    {
        selector.SelectedComponentChanged.AddListener(ComponentSelectionChanged);
        selectedRocketManager.rocketSelected.AddListener(selector.LoadActiveComponent);
    }

    private void DisconnectSelector(RocketComponentSelector selector)
    {
        selector.SelectedComponentChanged.RemoveListener(ComponentSelectionChanged);
        selectedRocketManager.rocketSelected.RemoveListener(selector.LoadActiveComponent);
    }

    private void ComponentSelectionChanged(RocketComponentType componentType, GameObject componentPrefab, bool unlocked)
    {
        OnComponentSelectionChanged?.Invoke();
        selectedRocketManager.ChangeActiveRocketSelectedComponent(componentType, componentPrefab, unlocked);
    }

    private void ApplySelectionToAll(Button button)
    {
        for (int i = 0; i < selectedRocketManager.rockets.Length; i++)
        {
            GameObject frontPrefab = frontSelector.GetCurrentSelectionPrefab(out bool frontUnlocked);
            if (frontUnlocked)
            {
                selectedRocketManager.ChangeComponent(i, RocketComponentType.FRONT, frontPrefab, true);
            }
            GameObject bodyPrefab = bodySelector.GetCurrentSelectionPrefab(out bool bodyUnlocked);
            if (bodyUnlocked)
            {
                selectedRocketManager.ChangeComponent(i, RocketComponentType.BODY, bodyPrefab, true);
            }
            GameObject propPrefab = propSelector.GetCurrentSelectionPrefab(out bool propUnlocked);
            if (propUnlocked)
            {
                selectedRocketManager.ChangeComponent(i, RocketComponentType.PROPULSION, propPrefab, true);
            }
        }
    }

    public ResearchManager GetResearchManager(RocketComponentType type)
    {
        return researchManagers.First(researchManager => researchManager.componentType == type);
    }

    private void OnModeSwitched()
    {
        if (droneConfigurator.configurationMode == ConfigurationMode.RESEARCH)
        {
            selectedRocketManager.LoadResearchFieldsOfActiveRocket();
        }
    }

    private void ToggleDescription(Button button = null)
    {
        descriptionDisplayer.ShowText(!descriptionVisible);
        screenDummyRocket.gameObject.SetActive(descriptionVisible);
        descriptionVisible = !descriptionVisible;
    }
}
