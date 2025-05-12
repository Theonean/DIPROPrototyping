using UnityEngine;
using UnityEngine.Events;

public abstract class ACInteractable : MonoBehaviour
{
    public bool IsCurrentlyInteractable = true;

    [HideInInspector] public bool UpdateHover;
    [HideInInspector] public bool UpdateInteract;
    [SerializeField] public Transform TouchPoint;
    protected Outline outline;

    public string LookAtText { get; set; }
    protected virtual void Start()
    {
        IsCurrentlyInteractable = true;
        outline = GetComponentInChildren<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
    public virtual void OnStartInteract() { }
    public virtual void OnUpdateInteract() { }
    public virtual void OnEndInteract() { }
    public virtual void OnStartHover()
    {
        if (!string.IsNullOrEmpty(LookAtText))
        {
            FPVUI.Instance.SetLookAtText(LookAtText);
        }
        if (outline != null)
        {
            outline.enabled = true;
        }
    }
    public virtual void OnUpdateHover(Vector2 mousePos) { }
    public virtual void OnEndHover()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

}