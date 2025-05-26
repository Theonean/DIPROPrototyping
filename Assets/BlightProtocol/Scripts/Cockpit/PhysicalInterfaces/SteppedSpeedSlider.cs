using UnityEngine;
using UnityEngine.Events;

public class SteppedSpeedSlider : ACSlider
{
    public UnityEvent startDrag, endDrag;
    public UnityEvent<float> valueChanged;

    private float[] stepPositions;
    private float[] boundaries;
    public float currentBoundary = 1f;

    public override void OnStartInteract()
    {
        base.OnStartInteract();
        startDrag.Invoke();
    }
    public override void OnEndInteract()
    {
        base.OnEndInteract();
        endDrag.Invoke();
    }

    public override void Drag()
    {
        progress = GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        progress = Mathf.Clamp01(progress);

        if (progress > currentBoundary)
        {
            progress = currentBoundary;
        }

        Vector3 newPos = GetPosition(progress);
        head.position = newPos;
        OnValueChanged(progress);
    }
    protected override void OnValueChanged(float normalizedValue)
    {
        valueChanged.Invoke(normalizedValue);
    }

    public void InitializeStepPositions(int count)
    {
        stepPositions = new float[count];
        boundaries = new float[count];

        float stepHeightNormalized = 1f / count;

        for (int i = 0; i < count; i++)
        {
            stepPositions[i] = stepHeightNormalized * (i + 1) - stepHeightNormalized / 2;
            boundaries[i] = stepHeightNormalized * (i + 1) - 0.05f;
        }
        currentBoundary = boundaries[^1];
    }

    public void SetBoundary(int index)
    {
        currentBoundary = boundaries[index];
    }

    public void SetPositionByIndex(int index)
    {
        SetPositionNormalized(stepPositions[index]);
    }
}
