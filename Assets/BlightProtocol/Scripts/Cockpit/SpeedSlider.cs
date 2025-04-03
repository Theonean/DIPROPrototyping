using System.Collections;
using UnityEngine;

public class SpeedSlider : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E";
    public string interactText = "";
    public bool UpdateHover { get; set; } = false;
    public bool UpdateInteract { get; set; } = true;
    [SerializeField] private Transform _touchPoint;

    public Transform TouchPoint
    {
        get => _touchPoint;
        set => _touchPoint = value;
    }

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public string InteractText
    {
        get => interactText;
        set => interactText = value;
    }

    public Transform sliderHead;
    public Transform sliderMax;
    public Transform sliderMin;
    private float sliderProgress = 0f;
    private Vector2[] screenSpaceBounds;
    public AnimationCurve adjustCurve;

    private int speedStepCount;

    void Start()
    {
        speedStepCount = HarvesterSpeedControl.Instance.GetSpeedStepCount();
        SetSliderPosition(0);
    }

    public void OnStartInteract()
    {
        Cursor.visible = false;
        screenSpaceBounds = this.GetScreenSpaceBounds(sliderMin.position, sliderMax.position, FPVPlayerCam.Instance.GetComponent<Camera>());
    }
    public void OnUpdateInteract()
    {
        DragSlider();
    }
    public void OnEndInteract()
    {
        Cursor.visible = true;
    }

    public void OnHover()
    {
        this.DefaultOnHover();
    }

    private void DragSlider()
    {
        sliderProgress = this.GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        Vector3 newPos = GetSliderPosition(sliderProgress);

        sliderHead.position = newPos;

        float percentage = Vector3.Distance(sliderMin.position, newPos) / Vector3.Distance(sliderMin.position, sliderMax.position);
        int newIndex = Mathf.RoundToInt(percentage * (speedStepCount - 1));

        HarvesterSpeedControl.Instance.SetSpeedStepIndex(newIndex);
    }

    public void SetSliderPosition(float SpeedStepIndex)
    {
        // NOTE: Problem is state setting, harvester likely goes into idle state if speed is low
        Logger.Log("overriding slider position", LogLevel.INFO, LogType.LOGGER);
        float targetProgress = SpeedStepIndex / (speedStepCount - 1);
        StartCoroutine(LerpSliderPosition(targetProgress, 1f));
    }

    private IEnumerator LerpSliderPosition(float targetProgress, float duration)
    {
        IsCurrentlyInteractable = false;
        float elapsedTime = 0f;
        float startProgress = sliderProgress;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = adjustCurve.Evaluate(t);

            float newProgress = startProgress + curveValue * (targetProgress - startProgress);
            sliderHead.position = GetSliderPosition(newProgress);
            sliderProgress = newProgress;


            elapsedTime += Time.deltaTime;
            yield return null;
        }
        sliderHead.position = Vector3.Lerp(sliderMin.position, sliderMax.position, targetProgress);
        sliderProgress = targetProgress;
        IsCurrentlyInteractable = true;
    }

    private Vector3 GetSliderPosition(float progress)
    {
        return Vector3.Lerp(sliderMin.position, sliderMax.position, progress);
    }
}
