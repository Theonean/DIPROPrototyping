using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResearchManager : MonoBehaviour
{
    [SerializeField] Button researchButton;
    [SerializeField] TextMeshPro componentReq, crystalReq;
    [SerializeField] TextMeshPro description;
    [SerializeField] RocketComponentSelector componentSelector;

    public UnityEvent OnResearched;

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

        OnResearched.Invoke();
    }
}
