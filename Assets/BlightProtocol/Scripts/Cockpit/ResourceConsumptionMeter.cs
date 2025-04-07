using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceCocnsumptionMeter : MonoBehaviour
{
    public GameObject needlePivot;
    public Vector2 minMaxAngle = new Vector2(0, 180);
    public float maxDelta = 10f;
    private float delta = 0f;
    private float lastDelta = 0f;
    private ResourceHandler resourceHandler;
    public float needleSpeed = 1f;
    private Coroutine currentCoroutine;

    void Start()
    {
        resourceHandler = ResourceHandler.Instance;
    }

    void Update()
    {
        delta = resourceHandler.GetDelta(resourceHandler.fuelResource);
        if (!Mathf.Approximately(delta, lastDelta))
        {
            UpdateDisplay();
            lastDelta = delta;
        }
    }

    void UpdateDisplay()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        
        // Normalize delta between -maxDelta and maxDelta to 0-1 range
        float normalizedValue = Mathf.Clamp01((delta + maxDelta) / (2 * maxDelta));
        float targetAngle = Mathf.Lerp(minMaxAngle.x, minMaxAngle.y, normalizedValue);
        
        currentCoroutine = StartCoroutine(LerpNeedle(targetAngle));
    }

    private IEnumerator LerpNeedle(float targetAngle)
    {
        float startAngle = needlePivot.transform.localEulerAngles.z;
        // Handle angle wrapping
        if (startAngle > 180) startAngle -= 360;
        
        float time = 0;
        float duration = Mathf.Abs(targetAngle - startAngle) / needleSpeed;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            float currentAngle = Mathf.Lerp(startAngle, targetAngle, t);
            needlePivot.transform.localRotation = Quaternion.Euler(0, 0, currentAngle);
            yield return null;
        }
        
        needlePivot.transform.localRotation = Quaternion.Euler(0, 0, targetAngle);
        currentCoroutine = null;
    }
}