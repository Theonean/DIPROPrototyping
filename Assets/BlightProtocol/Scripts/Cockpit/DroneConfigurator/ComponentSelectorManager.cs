using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentSelectorManager : MonoBehaviour
{
    [SerializeField] private DroneConfigurator droneConfigurator;
    [SerializeField] private SelectedRocketManager selectedRocketManager;
    [SerializeField] private RocketComponentSelector[] componentSelectors = new RocketComponentSelector[3];
    [SerializeField] private ResearchManager researchManager;
    [SerializeField] private RocketComponentSelector researchComponentSelector;

    void OnEnable()
    {
        foreach (var componentSelector in componentSelectors)
        {
            ConnectSelector(componentSelector);
        }

        // for research
        ConnectSelector(researchComponentSelector);
        researchManager.OnResearched.AddListener(researchComponentSelector.ApplyCurrentSelection);
        researchComponentSelector.SelectedComponentApplied.AddListener(selectedRocketManager.LevelUpComponent);
    }

    void OnDisable()
    {
        foreach (var componentSelector in componentSelectors)
        {
            DisconnectSelector(componentSelector);
        }

        // for research
        DisconnectSelector(researchComponentSelector);
        researchManager.OnResearched.RemoveListener(researchComponentSelector.ApplyCurrentSelection);
        researchComponentSelector.SelectedComponentApplied.RemoveListener(selectedRocketManager.LevelUpComponent);
    }

    private void ConnectSelector(RocketComponentSelector selector)
    {
        selector.SelectedComponentChanged.AddListener(ComponentSelectionChanged);
        selectedRocketManager.onRocketsLoaded.AddListener(selector.LoadActiveComponents);
        droneConfigurator.onModeSwitched.AddListener(selector.LoadActiveComponents);
    }

    private void DisconnectSelector(RocketComponentSelector selector)
    {
        selector.SelectedComponentChanged.RemoveListener(ComponentSelectionChanged);
        selectedRocketManager.onRocketsLoaded.RemoveListener(selector.LoadActiveComponents);
        droneConfigurator.onModeSwitched.RemoveListener(selector.LoadActiveComponents);
    }

    private void ComponentSelectionChanged(RocketComponentType componentType, GameObject componentPrefab)
    {
        selectedRocketManager.ChangeComponent(componentType, componentPrefab);
    }
}
