using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;

public class RocketComponentSelector : MonoBehaviour
{
    public RocketComponentType componentType;
    private List<ACRocketComponent> components;
    public int index = 0;
    public int maxValue;
    private ScrollButtons scrollButtons;

    [Header("Display")]
    [SerializeField] private Image componentIcon;
    [SerializeField] private TextMeshProUGUI componentNameText;

    public UnityEvent<RocketComponentType, GameObject> SelectedComponentChanged;
    public UnityEvent<RocketComponentType> SelectedComponentApplied;

    void Start()
    {
        scrollButtons = GetComponent<ScrollButtons>();
        scrollButtons.onScrolled.AddListener(Scroll);
    }

    public void LoadActiveComponent(Rocket rocket)
    {
        components = ItemManager.Instance.GetAvailableComponents(componentType);

        maxValue = components.Count - 1;
        int componentIndex;
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                componentIndex = GetComponentIndex(rocket.propulsionComponent.GetComponent<ACRocketComponent>());
                index = componentIndex;
                UpdateDisplay(componentIndex);
                break;

            case RocketComponentType.BODY:
                componentIndex = GetComponentIndex(rocket.bodyComponent.GetComponent<ACRocketComponent>());
                index = componentIndex;
                UpdateDisplay(componentIndex);
                break;

            case RocketComponentType.FRONT:
                componentIndex = GetComponentIndex(rocket.frontComponent.GetComponent<ACRocketComponent>());
                index = componentIndex;
                UpdateDisplay(componentIndex);
                break;

        }
    }

    public void Scroll(int direction)
    {
        index += direction;
        Debug.Log(index + " " + direction);
        if (index > maxValue) index = 0;
        else if (index < 0) index = maxValue;

        OnValueChanged(index);
        UpdateDisplay(index);
    }

    private void OnValueChanged(int selectedIndex)
    {
        ACRocketComponent component = components[selectedIndex];
        Debug.Log(component);

        GameObject selectedComponentPrefab = null;
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                selectedComponentPrefab = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketPropulsions
                    .FirstOrDefault(x => x.GetComponent<ACRocketComponent>() == component);
                break;
            case RocketComponentType.BODY:
                selectedComponentPrefab = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketBodies
                    .FirstOrDefault(x => x.GetComponent<ACRocketComponent>() == component);
                break;
            case RocketComponentType.FRONT:
                selectedComponentPrefab = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketFronts
                    .FirstOrDefault(x => x.GetComponent<ACRocketComponent>() == component);
                break;
        }


        if (selectedComponentPrefab != null)
        {
            SelectedComponentChanged?.Invoke(componentType, selectedComponentPrefab);
        }
        else
        {
            Debug.LogError($"No component found for name: {component}");
        }
        Debug.Log($"Selected {componentType} component: {selectedComponentPrefab?.name}");
    }

    public void ApplyCurrentSelection()
    {
        OnValueChanged(index);
        SelectedComponentApplied.Invoke(componentType);
    }

    private void UpdateDisplay(int value)
    {
        // Get the selected component's name and icon to display
        string stringToReplace = "";
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                stringToReplace = "Propulsion";
                break;
            case RocketComponentType.BODY:
                stringToReplace = "Body";
                break;
            case RocketComponentType.FRONT:
                stringToReplace = "Front";
                break;
        }
        //componentNameText.text = components[value].name.Replace(stringToReplace, "");
    }

    public int GetComponentIndex(ACRocketComponent component)
    {
        if (component == null || components == null)
            return -1;

        // Strip "(Clone)" from instantiated prefab names
        string componentName = component.name.Replace("(Clone)", "").Trim();

        return components.FindIndex(c =>
            c != null &&
            c.GetType() == component.GetType() &&
            c.name == componentName
        );
    }
}
