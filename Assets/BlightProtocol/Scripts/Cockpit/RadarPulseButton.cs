using UnityEngine;

public class RadarPulseButton : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "E"; // Backing field for Inspector

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public void OnInteract()
    {
        Radar.Instance.Pulse();
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
