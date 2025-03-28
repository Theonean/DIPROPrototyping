using UnityEngine;

public class FPVDroneGoggles : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E";
    public string interactText = "";

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

    public void OnInteract()
    {
        PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
    }

    public void OnHover()
    {
        this.DefaultOnHover();
    }
}
