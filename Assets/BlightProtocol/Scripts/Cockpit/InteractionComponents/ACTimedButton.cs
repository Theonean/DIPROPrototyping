using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ACTimedButton : ACButton
{
    public Slider slider;
    public RectTransform sweetSpotOverlay;
    public Vector2 sweetSpotMinMax = new Vector2(0.6f, 0.8f);
    public float chargeTime = 1f;
    public bool isCharging = false;
    public Color normalColor = Color.blue;
    public Color succeedColor = Color.yellow;
    public Color failColor = Color.red;

    private void Start()
    {
        UpdateSweetSpotVisual();
    }

    public override void OnStartInteract()
    {
        if (isCharging)
        {
            if (slider.value > sweetSpotMinMax.x && slider.value < sweetSpotMinMax.y)
            {
                OnChargeSucceeded();
            }
            else
            {
                OnChargeFailed();
            }
        }
        else
        {
            slider.value = 0;
            StartCoroutine(LerpSlider(1, chargeTime));
            isCharging = true;
        }
    }

    private IEnumerator LerpSlider(float target, float duration, bool isReset = false)
    {
        float elapsedTime = 0f;
        float startValue = slider.value;
        while (elapsedTime < duration)
        {
            slider.value = Mathf.Lerp(startValue, target, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        slider.value = target;
        if (!isReset)
        {
            OnChargeTimeElapsed();
        }
    }

    public virtual void OnChargeTimeElapsed()
    {
        isCharging = false;
        slider.fillRect.GetComponent<Image>().color = normalColor;
        StartCoroutine(LerpSlider(0, 0.75f, true));
    }

    public virtual void OnChargeSucceeded()
    {
        isCharging = false;
        slider.fillRect.GetComponent<Image>().color = succeedColor;
        StopAllCoroutines();
        StartCoroutine(LerpSlider(0, 1f, true));
    }

    public virtual void OnChargeFailed()
    {
        isCharging = false;
        slider.fillRect.GetComponent<Image>().color = failColor;
        StopAllCoroutines();
        StartCoroutine(LerpSlider(0, 0.5f, true));
    }

    private void UpdateSweetSpotVisual()
    {
        if (sweetSpotOverlay == null || slider == null) return;

        RectTransform sliderRect = slider.GetComponent<RectTransform>();
        float sliderWidth = sliderRect.rect.width;

        float startX = sweetSpotMinMax.x * sliderWidth;
        float width = (sweetSpotMinMax.y - sweetSpotMinMax.x) * sliderWidth;

        sweetSpotOverlay.anchorMin = new Vector2(0, 0);
        sweetSpotOverlay.anchorMax = new Vector2(0, 1); // Full vertical stretch
        sweetSpotOverlay.pivot = new Vector2(0, 0.5f);
        sweetSpotOverlay.sizeDelta = new Vector2(width, 0); // height handled by anchors
        sweetSpotOverlay.anchoredPosition = new Vector2(startX, 0);
    }
}
