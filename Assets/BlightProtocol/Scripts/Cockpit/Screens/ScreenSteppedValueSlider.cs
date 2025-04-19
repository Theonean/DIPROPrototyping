using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSteppedValueSlider : ScreenValueSlider
{
    [SerializeField] RectTransform stepContainer;
    public int stepCount = 0;

    protected virtual void Start() {
        CreateStepVisuals();
    }
    private void CreateStepVisuals()
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
