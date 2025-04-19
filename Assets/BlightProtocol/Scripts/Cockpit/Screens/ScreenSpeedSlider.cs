using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ScreenSpeedSlider : ScreenSteppedValueSlider
{
    [SerializeField] private Image feedbackImage;
    [SerializeField] private AnimationCurve flashCurve;
    [SerializeField] private float flashDuration = 0.5f;
    protected override void Start()
    {
        stepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        HarvesterSpeedControl.Instance.inputDenied.AddListener(OnInputDenied);
        base.Start();
    }

    private void OnInputDenied()
    {
        Debug.Log("denied");
        StartCoroutine(FlashFeedback(0.5f));
    }

    private IEnumerator FlashFeedback(float duration)
    {
        {
            float startAlpha = feedbackImage.color.a;
            float newAlpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                newAlpha = flashCurve.Evaluate(elapsedTime / duration);
                feedbackImage.color = new Color(feedbackImage.color.r, feedbackImage.color.g, feedbackImage.color.b, newAlpha);
                yield return null;
            }
        }
    }
}
