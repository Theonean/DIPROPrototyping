using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACButton : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable {get; set;} = true;
    public string lookAtText = "E";
    public string interactText = "";
    public virtual bool UpdateHover { get; set; } = false;
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

    public virtual void OnStartInteract() {}
    public virtual void OnUpdateInteract() {}
    public virtual void OnEndInteract() {}

    public virtual void OnHover() {
        this.DefaultOnHover();
    }
}
