// ScreenPulseSlider.cs - Pure visualization component
using UnityEngine;
using UnityEngine.UI;

public class PulseScreen : ScreenValueSlider
{
    [Header("Pulse Settings")]
    public RectTransform sweetSpotOverlay;
    public Color normalColor = Color.blue;
    public Color succeedColor = Color.yellow;
    public Color failColor = Color.red;

    public void UpdateSweetSpotVisual(Vector2 minMax)
    {
        if (sweetSpotOverlay == null || slider == null) return;

        float sliderHeight = slider.GetComponent<RectTransform>().rect.height;
        float sliderWidth = slider.GetComponent<RectTransform>().rect.width;
        float startY = minMax.x * sliderHeight;
        float height = (minMax.y - minMax.x) * sliderHeight;

        sweetSpotOverlay.anchoredPosition = new Vector2(0, startY);
        sweetSpotOverlay.sizeDelta = new Vector2(0, height);
    }
}