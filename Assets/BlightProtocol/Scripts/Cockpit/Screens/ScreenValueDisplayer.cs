using UnityEngine;
using System.Collections;

public abstract class ScreenValueDisplayer : MonoBehaviour
{
    public float Value { get; protected set; } = 0;
    protected bool useMaxValue = false;
    protected float maxValue = 1f;
    public ScreenType screenType;

    private void OnDestroy()
    {
        CockpitScreenHandler.Instance.UnregisterDisplayer(screenType);
    }

    public void SetValue(float targetValue, float duration = -1)
    {
        if (duration > 0)
        {
            StartCoroutine(SetValueOverTime(targetValue, duration));
        }
        else
        {
            UpdateValue(targetValue);
        }
    }

    public void SetMaxValue(float maxValue)
    {
        this.maxValue = maxValue;
        useMaxValue = true;
    }

    private IEnumerator SetValueOverTime(float targetValue, float duration)
    {
        float startValue = Value;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float interpolatedValue = startValue + (targetValue - startValue) * t;
            UpdateValue(interpolatedValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        UpdateValue(targetValue);
    }

    protected virtual void UpdateValue(float targetValue)
    {
        Value = useMaxValue ? Mathf.Min(targetValue, maxValue) : targetValue;
    }
}