using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Linq;

public class RadioButton : MonoBehaviour
{
    public UnityEvent<int> OnSelected;
    [SerializeField] private List<Button> buttons;
    public int activeButton = 1;

    void Start()
    {
        if (buttons[activeButton] != null)
        {
            OnButtonPressed(buttons[activeButton]);
        }
        else Logger.Log("active button is null!", LogLevel.ERROR, LogType.COCKPIT);
    }

    void OnEnable()
    {
        foreach (var button in buttons)
        {
            button.OnPressed.AddListener(OnButtonPressed);
        }
    }

    void OnDisable()
    {
        foreach (var button in buttons)
        {
            button.OnPressed.RemoveListener(OnButtonPressed);
        }
    }

    void OnButtonPressed(Button pressedButton)
    {
        buttons.Where(button => button != pressedButton).ToList()
               .ForEach(button => button.SetPressed(false));
        activeButton = buttons.FindIndex(button => button == pressedButton);
        OnSelected.Invoke(activeButton);
    }

    public void SetIndex(int index) {
        Button buttonToPress = buttons[index];
        OnButtonPressed(buttonToPress);
    }
}
