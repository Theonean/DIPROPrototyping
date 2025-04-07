using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapZoomSlider : ACSlider
{
    public MapCamera mapCamera;
    public float minHeight = 100f;
    public float maxHeight = 300;

    void Start()
    {
        SetPositionNormalized(mapCamera.yPos / maxHeight - minHeight);
    }
    protected override void OnValueChanged(float normalizedValue) {
        mapCamera.SetHeight(Mathf.Lerp(minHeight, maxHeight, normalizedValue));
    }
}
