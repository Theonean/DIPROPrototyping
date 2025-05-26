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
    [SerializeField] private RocketComponentSelector[] componentSelectors = new RocketComponentSelector[3];
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
        foreach (var componentSelector in componentSelectors)
        {
            ConnectSelector(componentSelector);
        }

        ResearchManager.OnResearched.AddListener(selectedRocketManager.LevelUpComponent);

        droneConfigurator.onModeSwitched.AddListener(OnModeSwitched);

        applyToAllButton.OnPressed.AddListener(selectedRocketManager.ApplySelectionToAll);

        showDescriptionButton.OnPressed.AddListener(ToggleDescription);
    }

    void OnDisable()
    {
        foreach (var componentSelector in componentSelectors)
        {
            DisconnectSelector(componentSelector);
        }

        ResearchManager.OnResearched.RemoveListener(selectedRocketManager.LevelUpComponent);

        droneConfigurator.onModeSwitched.RemoveListener(OnModeSwitched);

        applyToAllButton.OnPressed.RemoveListener(selectedRocketManager.ApplySelectionToAll);

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

    private void ComponentSelectionChanged(RocketComponentType componentType, GameObject componentPrefab)
    {
        OnComponentSelectionChanged?.Invoke();
        selectedRocketManager.ChangeActiveRocketComponent(componentType, componentPrefab);
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
        descriptionDisplayer.gameObject.SetActive(!descriptionVisible);
        screenDummyRocket.gameObject.SetActive(descriptionVisible);
        descriptionVisible = !descriptionVisible;
    }
}
