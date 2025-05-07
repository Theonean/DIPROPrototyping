using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPeriscopeSlider : ACSlider
{
    private PersicopeCamera persicopeCamera;
    protected override void Start()
    {
        base.Start();
        persicopeCamera = PersicopeCamera.Instance;
        UpdateInteract = true;
    }

    protected override void OnValueChanged(float normalizedValue)
    {
        persicopeCamera.RotateCam(normalizedValue);
    }

}
