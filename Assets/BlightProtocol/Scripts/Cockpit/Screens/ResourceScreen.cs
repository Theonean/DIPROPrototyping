
public class ResourceScreen : ScreenValueSlider
{
    protected void Start()
    {
        ResourceHandler.Instance.OnFuelConsumptionChanged.AddListener(OnFuelFeedback);
    }

    protected void OnEnable() {
        if (ResourceHandler.Instance != null) {
            ResourceHandler.Instance.OnFuelConsumptionChanged.RemoveListener(OnFuelFeedback);
            ResourceHandler.Instance.OnFuelConsumptionChanged.AddListener(OnFuelFeedback);
        }
    }
    protected void OnDisable()
    {
        ResourceHandler.Instance.OnFuelConsumptionChanged.RemoveListener(OnFuelFeedback);
    }

    private void OnFuelFeedback(ResourceData resource, float amount, float delta)
    {
        // maybe do something with the parameters here
        OnFeedback();
    }
}
