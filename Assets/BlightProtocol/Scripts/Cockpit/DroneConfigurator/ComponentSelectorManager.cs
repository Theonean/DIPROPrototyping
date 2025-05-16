using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;

public class ComponentSelectorManager : MonoBehaviour
{
    public static ComponentSelectorManager Instance { get; private set;}
    [SerializeField] private DroneConfigurator droneConfigurator;
    [SerializeField] private RotatingSelectedRocketManager selectedRocketManager;
    [SerializeField] private Button applyToAllButton;
    [SerializeField] private RocketComponentSelector[] componentSelectors = new RocketComponentSelector[3];
    [SerializeField] private ResearchManager[] researchManagers = new ResearchManager[3];

    void Awake()
    {
        if (Instance != null && Instance != this) {
            Destroy(this);
        }       
        else {
            Instance = this;
        }
    }

    void OnEnable()
    {
        foreach (var componentSelector in componentSelectors)
        {
            ConnectSelector(componentSelector);
        }
        foreach (var researchManager in researchManagers)
        {
            researchManager.OnResearched.AddListener(selectedRocketManager.LevelUpComponent);
        }
        applyToAllButton.OnPressed.AddListener(selectedRocketManager.ApplySelectionToAll);
    }

    void OnDisable()
    {
        foreach (var componentSelector in componentSelectors)
        {
            DisconnectSelector(componentSelector);
        }

        foreach (var researchManager in researchManagers)
        {
            researchManager.OnResearched.RemoveListener(selectedRocketManager.LevelUpComponent);
        }
        applyToAllButton.OnPressed.RemoveListener(selectedRocketManager.ApplySelectionToAll);
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
        selectedRocketManager.ChangeActiveRocketComponent(componentType, componentPrefab);
    }

    public ResearchManager GetResearchManager(RocketComponentType type) {
        return researchManagers.First(researchManager => researchManager.componentType == type);
    }
}
