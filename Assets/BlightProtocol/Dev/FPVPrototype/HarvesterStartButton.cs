using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterStartButton : FPVInteractable
{

    public float setSpeed = 0f;

    public override void Interact()
    {
        ControlZoneManager.moveSpeed = setSpeed;
    }
}
