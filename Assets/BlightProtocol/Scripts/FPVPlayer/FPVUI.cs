using TMPro;
using UnityEngine;

public class FPVUI : MonoBehaviour
{
    public static FPVUI Instance { get; private set; }
    public TextMeshProUGUI lookAtText;
    public TextMeshProUGUI interactText;
    public GameObject crosshair;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    // Look at Text
    public void SetLookAtText(string text) {
        lookAtText.text = text;
    }

    public void ClearLookAtText() {
        lookAtText.text = "";
    }

    public void ToggleLookAtText(bool toggle) {
        lookAtText.gameObject.SetActive(toggle);
    }

    // Interact Text
    public void SetInteractText(string text) {
        interactText.text = text;
    }

    public void ClearInteractText() {
        interactText.text = "";
    }

    public void ToggleInteractText(bool toggle) {
        interactText.gameObject.SetActive(toggle);
    }

    public void ToggleFPVCrosshair(bool toggle) {
        crosshair.SetActive(toggle);
    }
}
