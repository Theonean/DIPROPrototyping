public class SpeedScreen : ScreenSteppedValueSlider
{
    private HarvesterSpeedControl speedControl;

    protected override void Awake() {
        base.Awake();
        speedControl = HarvesterSpeedControl.Instance;
    }

    protected void OnEnable() {
        speedControl.overrodePosition.AddListener(OnPositionOverride);
    }

    protected void OnDisable() {
        speedControl.overrodePosition.RemoveListener(OnPositionOverride);
    }
    protected override void Start()
    {
        stepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        base.Start();
    }

    private void OnPositionOverride()
    {
        Flash();
    }
}
