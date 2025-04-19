using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenValueSlider : ScreenValueDisplayer
{
    [SerializeField] private Slider slider;

    protected void Awake()
    {
        useMaxValue = true;
    }

    protected override void UpdateValue(float targetValue) {
        Value = Mathf.Min(targetValue, maxValue);
        slider.value = Value / maxValue;
    }

}
