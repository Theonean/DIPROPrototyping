using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GasScreen : ACScreenValueDisplayer
{
    private GasManager gasManager;
    [SerializeField] GasData gasData;
    private Gas gas;
    protected void Awake()
    {
        gasManager = GasManager.Instance;
    }
    protected void OnEnable()
    {
        if (gas == null)
        {
            if (!gasManager.FindGas(gasData, out gas))
            {
                return;
            }
        }

        gas.amountChanged.AddListener(OnGasAmountChanged);

        SetValue(gas.amount);
    }

    protected void OnDisable()
    {
        if (gas != null)
        {
            gas.amountChanged.RemoveListener(OnGasAmountChanged);
        }
    }

    protected void OnGasAmountChanged(int delta)
    {
        {
            SetValue(gas.amount);
            if (delta < 0) Flash();
        }

    }
}
