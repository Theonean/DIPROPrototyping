using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public abstract class ACScreenValueDisplayer : MonoBehaviour
{
    public float Value { get; protected set; } = 0;
    protected bool useMaxValue = false;
    protected float maxValue = 1f;

    [Header("Feedback")]
    [SerializeField] private Image feedbackImage;
    [SerializeField] private AnimationCurve flashCurve;
    [SerializeField] private float flashDuration = 0.5f;

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

    public virtual void OnFeedback() {
        StartCoroutine(FlashFeedback(flashDuration));
    }

    protected virtual IEnumerator FlashFeedback(float duration)
    {
        {
            float startAlpha = feedbackImage.color.a;
            float newAlpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                newAlpha = flashCurve.Evaluate(elapsedTime / duration);
                feedbackImage.color = new Color(feedbackImage.color.r, feedbackImage.color.g, feedbackImage.color.b, newAlpha);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}