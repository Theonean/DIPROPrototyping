using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedScreen : ScreenSteppedValueSlider
{
    private HarvesterSpeedControl speedControl;
    private GameObject[] steps;

    protected override void Awake()
    {
        speedControl = HarvesterSpeedControl.Instance;
        stepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        steps = new GameObject[stepCount];
        base.Awake();
    }

    protected void OnEnable()
    {
        speedControl.overrodePosition.AddListener(OnPositionOverride);
    }

    protected void OnDisable()
    {
        speedControl.overrodePosition.RemoveListener(OnPositionOverride);
    }

    private void OnPositionOverride()
    {
        Flash();
    }

    public void SetActiveStep(int index)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            Image image = steps[i].GetComponentInChildren<Image>();
            TextMeshProUGUI text = steps[i].GetComponentInChildren<TextMeshProUGUI>();
            if (i == index)
            {
                image.enabled = true;
                text.color = Color.black;

            }
            else
            {
                image.enabled = false;
                text.color = HarvesterSpeedControl.Instance.speedSteps[i].textColor;
            }
        }
    }

    protected override void CreateStepVisuals()
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
            steps[i] = stepObj;
            TextMeshProUGUI text = stepObj.GetComponentInChildren<TextMeshProUGUI>();
            Image image = stepObj.GetComponentInChildren<Image>();

            RectTransform rt = stepObj.GetComponent<RectTransform>();

            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, stepHeight);
            rt.anchoredPosition = new Vector2(0, i * stepHeight);

            text.text = i.ToString();
            text.color = HarvesterSpeedControl.Instance.speedSteps[i].textColor;

            image.color = HarvesterSpeedControl.Instance.speedSteps[i].activeBackgroundColor;
        }
    }
}
