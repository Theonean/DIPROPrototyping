using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPeriscopeButton : ACInteractable
{
    [SerializeField] private Vector3 direction;
    private PersicopeCamera persicopeCamera;

    protected override void Start()
    {
        base.Start();
        persicopeCamera = PersicopeCamera.Instance;
        UpdateInteract = true;
    }

    public override void OnUpdateInteract()
    {
        persicopeCamera.RotateCam(direction);
    }
}
