using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : ACSlider
{
    private int speedStepCount;

    [Header("Step Visuals")]
    public RectTransform stepContainer;

    void Start()
    {
        speedStepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        SetSliderPositionIndex(0);
        CreateStepVisuals();
    }

    protected override void OnValueChanged(float normalizedValue)
    {
        int index = Mathf.FloorToInt(normalizedValue * speedStepCount);
        index = Mathf.Clamp(index, 0, speedStepCount - 1);
        HarvesterSpeedControl.Instance.SetSpeedStepIndex(index);
    }

    public void SetSliderPositionIndex(int index)
    {
        float normalized = Mathf.Clamp01(index / (float)speedStepCount);
        SetPositionNormalized(normalized);
    }

    private void CreateStepVisuals()
{
    if (stepContainer == null) return;
    foreach (Transform child in stepContainer)
    {
        Destroy(child.gameObject);
    }

    float totalHeight = stepContainer.rect.height;
    float stepHeight = totalHeight / speedStepCount;

    for (int i = 0; i < speedStepCount; i++)
    {
        GameObject stepObj = new GameObject("StepImage", typeof(RectTransform), typeof(Image));
        stepObj.transform.SetParent(stepContainer, false);

        RectTransform rt = stepObj.GetComponent<RectTransform>();
        Image img = stepObj.GetComponent<Image>();

        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.sizeDelta = new Vector2(0, stepHeight);
        rt.anchoredPosition = new Vector2(0, i * stepHeight);

        if (img != null)
        {
            img.color = HarvesterSpeedControl.Instance.speedSteps[i].displayColor;
        }
    }
}

}
