using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : ACSlider
{
    private int speedStepCount;
    private float[] speedstepPositions;


    void Start()
    {
        speedStepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        speedstepPositions = new float[speedStepCount];
        InitializeStepPositions();
        SetPositionByIndex(0);
    }

    protected override void OnValueChanged(float normalizedValue)
    {
        int index = Mathf.FloorToInt(normalizedValue * speedStepCount);
        index = Mathf.Clamp(index, 0, speedStepCount - 1);
        HarvesterSpeedControl.Instance.SetSpeedStepIndex(index);
    }
    public void SetPositionByIndex(int index)
    {
        SetPositionNormalized(speedstepPositions[index]);
    }

    public override void OnEndInteract()
    {
        int index = Mathf.FloorToInt(progress * speedStepCount);
        SetPositionByIndex(index);
    }

    private void InitializeStepPositions()
    {
        float stepHeightNormalized = 1f / (speedStepCount - 1f);

        for (int i = 0; i < speedStepCount; i++)
        {
            speedstepPositions[i] = stepHeightNormalized * i;
        }
    }

    protected override Vector3 GetPosition(float progress)
    {
        CockpitScreenHandler.Instance.SetValue(ScreenType.SPEED, progress);
        return Vector3.Lerp(minPos.position, maxPos.position, progress);
    }
}
