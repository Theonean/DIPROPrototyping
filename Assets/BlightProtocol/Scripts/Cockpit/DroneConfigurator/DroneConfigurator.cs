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

    void Start()
    {
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
        switchModeButton.OnPressed.AddListener(SwitchMode);
    }

    void OnDisable()
    {
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
}
