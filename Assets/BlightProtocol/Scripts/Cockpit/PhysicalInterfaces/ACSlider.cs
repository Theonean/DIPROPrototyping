using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ACSlider : ACInteractable
{

    public Transform head;
    public Transform maxPos;
    public Transform minPos;
    public AnimationCurve adjustCurve;
    [SerializeField] private float adjustSpeed = 1f;

    protected float progress = 0f; // Value between 0 and 1
    protected Vector2[] screenSpaceBounds;

    protected override void Start() {
        base.Start();
        UpdateInteract = true;
    }

    protected abstract void OnValueChanged(float normalizedValue);

    public override void OnStartInteract()
    {
        screenSpaceBounds = GetScreenSpaceBounds(minPos.position, maxPos.position, FPVInputManager.Instance.fpvPlayerCam);
    }

    public override void OnUpdateInteract()
    {
        Drag();
    }

    public virtual void Drag()
    {
        progress = GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        progress = Mathf.Clamp01(progress);

        Vector3 newPos = GetPosition(progress);
        head.position = newPos;
        OnValueChanged(progress);
    }

    public virtual void SetPositionNormalized(float normalizedValue)
    {
        normalizedValue = Mathf.Clamp01(normalizedValue);
        StartCoroutine(LerpPosition(normalizedValue, adjustSpeed));
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

    protected virtual Vector3 GetPosition(float progress)
    {
        return Vector3.Lerp(minPos.position, maxPos.position, progress);
    }

    public static Vector2[] GetScreenSpaceBounds(Vector3 worldPosMin, Vector3 worldPosMax, Camera camera)
    {
        Vector3 screenMin = camera.WorldToScreenPoint(worldPosMin);
        Vector3 screenMax = camera.WorldToScreenPoint(worldPosMax);
        return new Vector2[2] { screenMin, screenMax };
    }

    public static float GetMouseProgressOnSlider(Vector2 screenSpaceMin, Vector2 screenSpaceMax, Vector2 MousePos)
    {
        // Direction vector of the bounds (A)
        Vector2 boundsVector = screenSpaceMax - screenSpaceMin;

        // Vector from min to MousePos (AP)
        Vector2 AP = MousePos - screenSpaceMin;

        // Projection of AP onto A: t = (AP • A) / (A • A)
        float t = Vector2.Dot(AP, boundsVector) / Vector2.Dot(boundsVector, boundsVector);

        // Clamp t to ensure C stays within the bounds segment
        t = Mathf.Clamp01(t);

        // Compute C as min + t * boundsVector
        Vector2 C = screenSpaceMin + t * boundsVector;

        return t;
    }
}

