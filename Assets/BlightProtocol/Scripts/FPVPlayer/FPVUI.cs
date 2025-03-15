using TMPro;
using UnityEngine;

public class FPVUI : MonoBehaviour
{
    public static FPVUI Instance { get; private set; }
    public TextMeshProUGUI lookAtText;
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

    public void SetLookAtText(string text) {
        lookAtText.text = text;
    }

    public void ClearLookAtText() {
        lookAtText.text = "";
    }
}
