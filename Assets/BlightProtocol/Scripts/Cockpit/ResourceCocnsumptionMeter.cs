using UnityEngine;

public class ResourceConsumptionMeter : MonoBehaviour
{
    public GameObject needlePivot;
    public Vector2 minMaxAngle = new Vector2(0, 180);
    public float maxDelta = 10f;
    public float needleSpeed = 10f;
    public float responseLag = 0.1f; // Added parameter for lag amount
    
    private float delta = 0f;
    private ResourceHandler resourceHandler;
    private float currentAngle;
    private float targetAngle;
    private float velocity; // For smooth damping

    void Start()
    {
        resourceHandler = ResourceHandler.Instance;
    }

    void Update()
    {
        delta = resourceHandler.GetDelta(resourceHandler.fuelResource);
        
        // Normalize delta between -maxDelta and maxDelta to 0-1 range
        float normalizedValue = Mathf.Clamp01((delta + maxDelta) / (2 * maxDelta));
        targetAngle = Mathf.Lerp(minMaxAngle.x, minMaxAngle.y, normalizedValue);
        
        // Smoothly move toward target angle
        currentAngle = Mathf.SmoothDamp(currentAngle, targetAngle, ref velocity, responseLag, needleSpeed);
        
        needlePivot.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
    }
}