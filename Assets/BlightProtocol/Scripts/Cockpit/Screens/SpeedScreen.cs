public class SpeedScreen : ScreenSteppedValueSlider
{
    protected override void Start()
    {
        stepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        base.Start();
    }

    private void OnInputDenied()
    {
        StartCoroutine(FlashFeedback(0.5f));
    }
}
