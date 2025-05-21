using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResearchManager : MonoBehaviour
{
    [SerializeField] public RocketComponentType componentType;
    [SerializeField] Button researchButton;

    public static UnityEvent<RocketComponentType> OnResearched = new UnityEvent<RocketComponentType>();

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI researchCostCrystalText;
    [SerializeField] private TextMeshProUGUI researchCostComponentText;
    [SerializeField] private TextMeshProUGUI researchResultDescriptionText;

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
