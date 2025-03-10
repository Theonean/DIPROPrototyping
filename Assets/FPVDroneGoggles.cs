using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVDroneGoggles : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "[E] Enter Drone Mode"; // Backing field for Inspector

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public void OnInteract()
    {
        CameraTracker.Instance.SetTopDownPerspective();
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
