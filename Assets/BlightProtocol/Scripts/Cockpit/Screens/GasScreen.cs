using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GasScreen : ACScreenValueDisplayer
{
    private ItemManager itemManager;

    protected void Awake()
    {
        itemManager = ItemManager.Instance;
    }
    protected void OnEnable()
    {
        itemManager.gasAmountChanged.AddListener(OnGasAmountChanged);

        SetValue(itemManager.GetGas());
    }

    protected void OnDisable()
    {
        itemManager.gasAmountChanged.RemoveListener(OnGasAmountChanged);
    }

    protected void OnGasAmountChanged(int delta)
    {
        SetValue(itemManager.GetGas());
        if (delta < 0) Flash();
    }
}
