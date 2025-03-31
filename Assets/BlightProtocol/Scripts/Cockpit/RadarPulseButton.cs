using UnityEngine;

public class RadarPulseButton : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "E";
    public string interactText = "";
    public bool UpdateHover { get; set; } = false;

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
        Radar.Instance.Pulse();
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
