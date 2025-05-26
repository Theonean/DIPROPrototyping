using UnityEngine;
using UnityEngine.Events;

public class SpeedSlider : ACSlider
{
    [Header("Visualization")]
    public UnityEvent startDrag;
    public UnityEvent<float> valueChanged, endDrag;
    public float boundary;

    protected override void Start()
    {
        base.Start();
    }

    public override void OnStartInteract()
    {
        if (TutorialManager.Instance.IsTutorialOngoing() && 
            TutorialManager.Instance.progressState is not TutorialProgress.SETSPEED 
            and not TutorialProgress.SETSPEEDRESOURCEPOINT 
            and not TutorialProgress.DRIVETOCHECKPOINT)
        {
            return;
        }

        base.OnStartInteract();
        startDrag.Invoke();
    }

    public override void Drag()
    {
        progress = GetMouseProgressOnSlider(screenSpaceBounds[0], screenSpaceBounds[1], Input.mousePosition);
        progress = Mathf.Clamp(progress, 0, boundary);

        Vector3 newPos = GetPosition(progress);
        head.position = newPos;
        valueChanged.Invoke(progress);
    }

    public override void OnEndInteract()
    {
        endDrag.Invoke(progress);
    }
}