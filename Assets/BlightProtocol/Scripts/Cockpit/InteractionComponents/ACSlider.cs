using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACSlider : MonoBehaviour, IFPVInteractable
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

    public Transform head;
    public Transform maxPos;
    public Transform minPos;
    public AnimationCurve adjustCurve;

    protected float progress = 0f; // Value between 0 and 1
    protected Vector2[] screenSpaceBounds;

    protected abstract void OnValueChanged(float normalizedValue);

    public virtual void OnStartInteract()
    {
        screenSpaceBounds = this.GetScreenSpaceBounds(minPos.position, maxPos.position, FPVPlayerCam.Instance.GetComponent<Camera>());
    }

    public virtual void OnUpdateInteract()
    {
        Drag();
    }

    public virtual void OnEndInteract()
    {
    }

    public virtual void OnHover()
    {
        this.DefaultOnHover();
    }

    public virtual void Drag()
    {
        progress = this.GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        progress = Mathf.Clamp01(progress);

        Vector3 newPos = GetPosition(progress);
        head.position = newPos;

        OnValueChanged(progress);
    }

    public void SetPositionNormalized(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        StartCoroutine(LerpPosition(normalizedValue, 1f));
    }

    protected virtual IEnumerator LerpPosition(float targetProgress, float duration)
    {
        IsCurrentlyInteractable = false;
        float elapsedTime = 0f;
        float startProgress = progress;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = adjustCurve.Evaluate(t);
            float newProgress = Mathf.Lerp(startProgress, targetProgress, curveValue);

            head.position = GetPosition(newProgress);
            progress = newProgress;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        head.position = GetPosition(targetProgress);
        progress = targetProgress;
        IsCurrentlyInteractable = true;

        OnValueChanged(targetProgress);
    }

    private Vector3 GetPosition(float progress)
    {
        return Vector3.Lerp(minPos.position, maxPos.position, progress);
    }
}

