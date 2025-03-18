using UnityEngine;

public class FPVDroneGoggles : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "[E] Enter Drone Mode"; // Backing field for Inspector

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
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
