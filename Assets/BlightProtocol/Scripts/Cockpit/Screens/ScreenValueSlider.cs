using UnityEngine;
using UnityEngine.UI;

public class ScreenValueSlider : ACScreenValueDisplayer
{
    public Slider slider;

    protected virtual void Awake()
    {
        useMaxValue = true;
    }

    protected override void UpdateValue(float targetValue) {
        Value = Mathf.Min(targetValue, maxValue);
        slider.value = Value / maxValue;
    }

}
