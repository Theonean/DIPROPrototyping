using UnityEngine;

public class SpeedSlider : ACSlider
{
    private int speedStepCount;

    void Start()
    {
        speedStepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        SetSliderPositionIndex(0);
    }

    protected override void OnValueChanged(float normalizedValue)
    {
        int index = Mathf.RoundToInt(normalizedValue * (speedStepCount - 1));
        HarvesterSpeedControl.Instance.SetSpeedStepIndex(index);
    }

    public void SetSliderPositionIndex(int index)
    {
        float normalized = index / (float)(speedStepCount - 1);
        SetPositionNormalized(normalized);
    }
}
