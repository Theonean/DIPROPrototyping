using UnityEngine;

public class HarvesterStartButton : MonoBehaviour, IFPVInteractable
{
    public bool increaseSpeed = true;
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "E"; // Backing field for Inspector

    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public void OnInteract()
    {
        HarvesterSpeedControl.Instance.AdjustSpeed(increaseSpeed);
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
