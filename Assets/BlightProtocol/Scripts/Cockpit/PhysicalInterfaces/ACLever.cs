using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACLever : ACSlider
{
    public Transform leverPivot;
    public float leverMinAngle = 0f;
    public float leverMaxAngle = 180f;
    public bool isPulled = false;

    public override void OnStartInteract() {
        base.OnStartInteract();
    }

    public override void OnUpdateInteract()
    {
        if (!isPulled)
        {
            base.OnUpdateInteract();
        }
    }

    public override void OnEndInteract()
    {
        base.OnEndInteract();
        if (!isPulled)
        {
            ResetLever();
        }
    }

    public override void Drag() {
        if (!IsCurrentlyInteractable) return;
        progress = this.GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        float angle = GetAngle(progress);

        leverPivot.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
        OnValueChanged(progress);
    }
    protected override void OnValueChanged(float normalizedValue)
    {
        float angle = GetAngle(normalizedValue);
        leverPivot.localRotation = Quaternion.AngleAxis(angle, Vector3.right);
        OnPulled(normalizedValue);
    }

    protected override IEnumerator LerpPosition(float targetProgress, float duration) {
        IsCurrentlyInteractable = false;
        float elapsedTime = 0f;
        float startProgress = progress;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = adjustCurve.Evaluate(t);
            float newProgress = Mathf.Lerp(startProgress, targetProgress, curveValue);

            leverPivot.localRotation = Quaternion.AngleAxis(GetAngle(newProgress), Vector3.right);
            progress = newProgress;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        leverPivot.localRotation = Quaternion.AngleAxis(GetAngle(targetProgress), Vector3.right);
        progress = targetProgress;
        IsCurrentlyInteractable = true;

        OnPulled(targetProgress);
    }

    private float GetAngle(float progress)
    {
        return Mathf.Lerp(leverMinAngle, leverMaxAngle, progress);
    }

    public void ResetLever()
    {
        SetPositionNormalized(0f);
        isPulled = false;
        FPVInteractionHandler.Instance.AbortInteraction();
    }

    protected virtual void OnPulled(float normalizedValue) {
        if (normalizedValue >= 0.9f && !isPulled)
        {
            SetPositionNormalized(1f);
            isPulled = true;
        }
    }
}