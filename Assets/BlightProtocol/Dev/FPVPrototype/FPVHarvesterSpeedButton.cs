using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterStartButton : MonoBehaviour, IFPVInteractable
{
    public float setSpeed = 0f;
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "E"; // Backing field for Inspector
    public HarvesterSpeedControl harvesterSpeedControl;

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public void OnInteract()
    {
        harvesterSpeedControl.SetSpeed(setSpeed);
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
