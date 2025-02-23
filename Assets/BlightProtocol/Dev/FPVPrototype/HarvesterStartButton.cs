using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterStartButton : FPVInteractable
{
    private bool isMoving = true;
    private float harvesterSpeed = 0f;

    void Start()
    {
        harvesterSpeed = ControlZoneManager.moveSpeed;
    }
    public override void Interact()
    {
        Debug.Log("Harvester Start Button Pressed");
        if (isMoving)
        {
            ControlZoneManager.moveSpeed = 0f;
            isMoving = false;
        }
        else
        {
            ControlZoneManager.moveSpeed = harvesterSpeed;
            isMoving = true;
        }
        Debug.Log(ControlZoneManager.moveSpeed);
    }
}
