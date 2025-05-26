using UnityEngine;
using UnityEngine.UI;

public class ScreenSteppedValueSlider : ScreenValueSlider
{
    [SerializeField] protected RectTransform stepContainer;
    [SerializeField] protected GameObject StepPrefab;
    public int stepCount = 0;

    protected override void Awake() {
        base.Awake();
        CreateStepVisuals();
    }
    protected virtual void CreateStepVisuals()
    {
        if (stepContainer == null) return;
        foreach (Transform child in stepContainer)
        {
            Destroy(child.gameObject);
        }

        float totalHeight = stepContainer.rect.height;
        float stepHeight = totalHeight / stepCount;
        float stepHeightNormalized = 1f / stepCount;

        for (int i = 0; i < stepCount; i++)
        {
            GameObject stepObj = Instantiate(StepPrefab, stepContainer, false);

            RectTransform rt = stepObj.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, stepHeight);
            rt.anchoredPosition = new Vector2(0, i * stepHeight);
        }
    }
}
