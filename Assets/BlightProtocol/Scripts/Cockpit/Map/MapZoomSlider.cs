using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZoomSlider : ACSlider
{
    public MapCamera mapCamera;

    protected override void Start()
    {
        base.Start();
        SetPositionNormalized(0.5f);
    }
    protected override void OnValueChanged(float normalizedValue) {
        mapCamera.SetHeight(normalizedValue);
    }
}
