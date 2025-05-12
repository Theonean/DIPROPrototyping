using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadarPulseButton : ACTimedButton
{
    public float chargeSuccededModifier = 1.5f;
    public float chargeFailedModifier = 0.8f;
    private RadarPulseData radarData;
    [SerializeField] private PulseScreen pulseSlider;

    private Image _fillImage;

    protected override void Start()
    {
        base.Start();
        if (pulseSlider != null)
        {
            _fillImage = pulseSlider.slider.fillRect.GetComponent<Image>();
            pulseSlider.UpdateSweetSpotVisual(sweetSpotMinMax);
        }
        radarData = Radar.Instance.radarData;
    }

    public override void OnStartInteract()
    {
        if (isCharging)
        {
            base.OnStartInteract();
        }
        else
        {
            if (ItemManager.Instance.RemoveCrystal(radarData.pulseCost))
            {
                base.OnStartInteract();
            }
            else
            {
                pulseSlider.OnFeedback();
            }
        }
    }

    protected override void UpdateChargeProgress(float progress)
    {
        pulseSlider.slider.value = progress;
    }

    protected override void HandleChargeResult(int result)
    {
        if (_fillImage == null) return;

        StopAllCoroutines();
        IsCurrentlyInteractable = false;

        switch (result)
        {
            case 0:
                // time ran out: normal
                _fillImage.color = pulseSlider.normalColor;
                Radar.Instance.Pulse(1);
                break;
            case 1:
                // succeeded
                _fillImage.color = pulseSlider.succeedColor;
                Radar.Instance.Pulse(chargeSuccededModifier);
                break;
            case 2:
                // failed
                _fillImage.color = pulseSlider.failColor;
                Radar.Instance.Pulse(chargeFailedModifier);
                break;
        }

        StartCoroutine(ResetSlider());
    }

    private IEnumerator ResetSlider()
    {
        float elapsedTime = 0f;
        float startValue = pulseSlider.slider.value;
        while (elapsedTime < 1f)
        {
            pulseSlider.slider.value = Mathf.Lerp(startValue, 0f, elapsedTime / 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        pulseSlider.slider.value = 0;
        _fillImage.color = pulseSlider.normalColor;
        IsCurrentlyInteractable = true;
    }

    protected override float GetCurrentProgress()
    {
        return pulseSlider?.slider.value ?? 0f;
    }
}