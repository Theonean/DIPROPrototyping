using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[System.Serializable]
public enum ConfigurationMode
{
    SELECTION,
    RESEARCH
}

public class DroneConfigurator : MonoBehaviour
{
    public ConfigurationMode configurationMode = ConfigurationMode.SELECTION;
    public UnityEvent onModeSwitched;

    [Header("Mode Switching")]
    [SerializeField] private Button switchModeButton;
    [SerializeField] private ConfiguratorRotator rotator;

    [Header("Selector")]
    [SerializeField] private GameObject selectorParent;
    private ACInteractable[] selectorInteractables;

    [Header("Research")]
    [SerializeField] private GameObject researchParent;
    private ACInteractable[] researchInteractables;
    [SerializeField] private RadioButton researchComponentTypeSelector;
    [SerializeField] private RocketComponentSelector researchComponentSelector;

    void Start()
    {
        SetResearchComponentType(researchComponentTypeSelector.activeButton);

        selectorInteractables = selectorParent.GetComponentsInChildren<ACInteractable>();
        researchInteractables = researchParent.GetComponentsInChildren<ACInteractable>();

        configurationMode = ConfigurationMode.SELECTION;
        foreach (var interactable in selectorInteractables)
        {
            interactable.enabled = true;
        }
        foreach (var interactable in researchInteractables)
        {
            interactable.enabled = false;
        }
    }

    void OnEnable()
    {
        researchComponentTypeSelector.OnSelected.AddListener(SetResearchComponentType);
        switchModeButton.OnPressed.AddListener(SwitchMode);
    }

    void OnDisable()
    {
        researchComponentTypeSelector.OnSelected.RemoveListener(SetResearchComponentType);
        switchModeButton.OnPressed.AddListener(SwitchMode);
    }

    private void SwitchMode(Button button = null)
    {
        switchModeButton.IsCurrentlyInteractable = false;
        Invoke(nameof(ResetSwitchModeButton), rotator.rotationTime);
        switch (configurationMode)
        {
            case ConfigurationMode.SELECTION:
                configurationMode = ConfigurationMode.RESEARCH;
                foreach (var interactable in selectorInteractables)
                {
                    interactable.enabled = false;
                }
                foreach (var interactable in researchInteractables)
                {
                    interactable.enabled = true;
                }

                SetResearchComponentType(0);
                researchComponentTypeSelector.SetIndex(0);
                break;

            case ConfigurationMode.RESEARCH:
                configurationMode = ConfigurationMode.SELECTION;
                foreach (var interactable in selectorInteractables)
                {
                    interactable.enabled = true;
                }
                foreach (var interactable in researchInteractables)
                {
                    interactable.enabled = false;
                }
                break;
        }
        configurationMode = configurationMode == ConfigurationMode.SELECTION ? ConfigurationMode.RESEARCH : ConfigurationMode.SELECTION;

        onModeSwitched.Invoke();
        rotator.StartRotation();
    }

    private void ResetSwitchModeButton() {
        switchModeButton.IsCurrentlyInteractable = true;
    }

    private void SetResearchComponentType(int selected)
    {
        switch (selected)
        {
            case 0:
                researchComponentSelector.componentType = RocketComponentType.PROPULSION;
                break;
            case 1:
                researchComponentSelector.componentType = RocketComponentType.BODY;
                break;
            case 2:
                researchComponentSelector.componentType = RocketComponentType.FRONT;
                break;
        }
        researchComponentSelector.LoadActiveComponents();
    }

}
