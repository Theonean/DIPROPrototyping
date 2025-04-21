using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnPeriscopeButton : ACButton
{
    [SerializeField] private Vector3 direction;
    private PersicopeCamera persicopeCamera;

    void Start()
    {
        persicopeCamera = PersicopeCamera.Instance;
        UpdateInteract = true;
    }

    public override void OnUpdateInteract()
    {
        persicopeCamera.RotateCam(direction);
    }
}
