using UnityEngine;

public class RadarPulseButton : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "E"; // Backing field for Inspector
    public float seismoEmission = 5f;

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public void OnInteract()
    {
        Radar.Instance.Pulse();
        Seismograph.Instance.SetOtherEmission("Radar Pulse", seismoEmission, 1f);
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
