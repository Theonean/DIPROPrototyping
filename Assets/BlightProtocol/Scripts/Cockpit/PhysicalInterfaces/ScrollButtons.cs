using UnityEngine;
using UnityEngine.Events;

public class ScrollButtons : MonoBehaviour
{
    [SerializeField] Button up, down;

    public UnityEvent<int> onScrolled; 

    void OnEnable() {
        up.OnPressed.AddListener(OnButtonPressed);
        down.OnPressed.AddListener(OnButtonPressed);
    }
    void OnDisable() {
        up.OnPressed.RemoveListener(OnButtonPressed);
        down.OnPressed.RemoveListener(OnButtonPressed);
    }

    void OnButtonPressed(Button button) {
        if (button == up) onScrolled.Invoke(1);
        else if (button == down) onScrolled.Invoke(-1);
    }
}
