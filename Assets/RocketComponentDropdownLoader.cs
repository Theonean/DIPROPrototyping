using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum RocketComponentType
{
    PROPULSION,
    BODY,
    FRONT
}

public class RocketComponentDropdownLoader : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    public GameObject selectedComponentHolder;
    public TextMeshProUGUI typeTextTitle;
    public TextMeshProUGUI componentSwitchInputKeyText;
    [SerializeField] private RocketComponentType componentType;
    public UnityEvent<RocketComponentType, GameObject> SelectedComponentChanged;
    private GameObject selectedComponentGameobject;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(GetComponentOptions());
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);

        typeTextTitle.text = componentType.ToString();
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                componentSwitchInputKeyText.text = "U";
                break;
            case RocketComponentType.BODY:
                componentSwitchInputKeyText.text = "I";
                break;
            case RocketComponentType.PROPULSION:
                componentSwitchInputKeyText.text = "O";
                break;
        }
    }

    private void Update()
    {
        switch (componentType)
        {
            case RocketComponentType.FRONT:
                if (Input.GetKeyDown("u"))
                {
                    // Cycle through options for FRONT component type
                    dropdown.value = (dropdown.value + 1) % dropdown.options.Count;
                }
                break;
            case RocketComponentType.BODY:
                if (Input.GetKeyDown("i"))
                {
                    // Cycle through options for BODY component type
                    dropdown.value = (dropdown.value + 1) % dropdown.options.Count;
                }
                break;
            case RocketComponentType.PROPULSION:
                if (Input.GetKeyDown("o"))
                {
                    // Cycle through options for PROPULSION component type
                    dropdown.value = (dropdown.value + 1) % dropdown.options.Count;
                }
                break;
        }
    }

    private List<string> GetComponentOptions()
    {
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                return PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketPropulsions
                    .Select(x => x.name).ToList();
            case RocketComponentType.BODY:
                return PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketBodies
                    .Select(x => x.name).ToList();
            case RocketComponentType.FRONT:
                return PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketFronts
                    .Select(x => x.name).ToList();
            default:
                return new List<string>();
        }
    }

    private void OnDropdownValueChanged(int selectedPropertyIndex)
    {
        string componentName = dropdown.options[selectedPropertyIndex].text;

        GameObject selectedComponentPrefab = null;
        switch (componentType)
        {
            case RocketComponentType.PROPULSION:
                selectedComponentPrefab = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketPropulsions
                    .FirstOrDefault(x => componentName.Contains(x.name));
                break;
            case RocketComponentType.BODY:
                selectedComponentPrefab = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketBodies
                    .FirstOrDefault(x => componentName.Contains(x.name));
                break;
            case RocketComponentType.FRONT:
                selectedComponentPrefab = PlayerCore.Instance.GetComponentInChildren<RocketAimController>().rocketFronts
                    .FirstOrDefault(x => componentName.Contains(x.name));
                break;
        }


        if (selectedComponentPrefab != null)
        {
            if(selectedComponentGameobject != null)
            {
                Destroy(selectedComponentGameobject);
            }
            selectedComponentGameobject = Instantiate(selectedComponentPrefab, Vector3.zero, Quaternion.identity, selectedComponentHolder.transform);
            SelectedComponentChanged?.Invoke(componentType, selectedComponentPrefab);
        }
        else
        {
            Debug.LogError($"No component found for name: {componentName}");
        }
        Debug.Log($"Selected {componentType} component: {selectedComponentPrefab?.name}");

    }

}
