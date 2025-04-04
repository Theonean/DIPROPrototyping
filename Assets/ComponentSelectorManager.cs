using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentSelectorManager : MonoBehaviour
{
    [SerializeField] private UISelectedRocketManager selectedRocketManager;
    [SerializeField] private RocketComponentDropdownLoader frontComponentDropdown;
    [SerializeField] private RocketComponentDropdownLoader bodyComponentDropdown;
    [SerializeField] private RocketComponentDropdownLoader propulsionComponentDropdown;

    void OnEnable()
    {
        frontComponentDropdown.SelectedComponentChanged.AddListener(ComponentSelectionChanged);
        bodyComponentDropdown.SelectedComponentChanged.AddListener(ComponentSelectionChanged);
        propulsionComponentDropdown.SelectedComponentChanged.AddListener(ComponentSelectionChanged);
    }

    void OnDisable()
    {
        frontComponentDropdown.SelectedComponentChanged.RemoveListener(ComponentSelectionChanged);
        bodyComponentDropdown.SelectedComponentChanged.RemoveListener(ComponentSelectionChanged);
        propulsionComponentDropdown.SelectedComponentChanged.RemoveListener(ComponentSelectionChanged);
    }

    private void ComponentSelectionChanged(RocketComponentType componentType, GameObject componentPrefab)
    {
        selectedRocketManager.ChangeComponent(componentType, componentPrefab);
    }
}
