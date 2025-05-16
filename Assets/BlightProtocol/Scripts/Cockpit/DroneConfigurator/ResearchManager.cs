using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResearchManager : MonoBehaviour
{
    [SerializeField] public RocketComponentType componentType;
    [SerializeField] Button researchButton;
    [SerializeField] TextMeshPro componentReq, crystalReq;
    [SerializeField] TextMeshPro description;
    [SerializeField] RocketComponentSelector componentSelector;

    public UnityEvent<RocketComponentType> OnResearched;

    [Header("Text")]
    [SerializeField] private TextMeshPro researchCostCrystalText;
    [SerializeField] private TextMeshPro researchCostComponentText;
    [SerializeField] private TextMeshPro researchResultDescriptionText;

    void OnEnable()
    {
        researchButton.OnPressed.AddListener(OnResearchPressed);
    }

    void OnDisable()
    {
        researchButton.OnPressed.RemoveListener(OnResearchPressed);
    }

    void OnResearchPressed(Button button) {
        // update text here

        OnResearched.Invoke(componentType);
    }

    public void SetText(string crystal, string component, string description) {
        researchCostCrystalText.text = crystal;
        researchCostComponentText.text = component;
        researchResultDescriptionText.text = description;
    }
}
