using UnityEngine;

public class FPVDroneGoggles : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E";
    public string interactText = "";
    public bool UpdateHover { get; set; } = false;
    public bool UpdateInteract { get; set; } = false;
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

    public void OnStartInteract()
    {
        PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
    }
    public void OnUpdateInteract() {}
    public void OnEndInteract() {}

    public void OnHover()
    {
        this.DefaultOnHover();
    }
}
