using UnityEngine;

public class HarvesterStartButton : MonoBehaviour, IFPVInteractable
{
    public bool increaseSpeed = true;
    public bool IsCurrentlyInteractable {get; set;} = true;
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
        HarvesterSpeedControl.Instance.AdjustSpeed(increaseSpeed);
    }

    public void OnHover() {
        this.DefaultOnHover();
    }
}
