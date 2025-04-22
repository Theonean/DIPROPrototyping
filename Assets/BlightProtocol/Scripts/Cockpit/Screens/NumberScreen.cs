using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberScreen : ACScreenValueDisplayer
{
    public int intValue;
    [SerializeField] private TextMeshProUGUI numberText;

    protected override void UpdateValue(float value) {
        base.UpdateValue(value);
        intValue = Mathf.RoundToInt(value);
        numberText.text = intValue.ToString();
    }
}
