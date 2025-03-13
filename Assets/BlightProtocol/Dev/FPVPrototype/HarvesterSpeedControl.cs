using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HarvesterSpeedControl : MonoBehaviour
{
    public float currentSpeed = 0f;
    public float baseSpeed = 0f;
    public float maxSpeed = 50f;
    public Slider speedIndicator;
    public ResourceData fuelResource;
    public float overSpeedCost = 10f;
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        ControlZoneManager.Instance.SetMoveSpeed(currentSpeed);
        UpdateSpeedIndicator();
    }

    private void UpdateSpeedIndicator()
    {
        speedIndicator.value = currentSpeed / maxSpeed;
    }

    void Update()
    {
        if (currentSpeed > baseSpeed)
        {
            if (ResourceHandler.Instance.CheckResource(fuelResource) > overSpeedCost)
            {
                ResourceHandler.Instance.ConsumeResource(fuelResource, overSpeedCost, true);
            }
            else {
                currentSpeed = baseSpeed;
                ControlZoneManager.Instance.SetMoveSpeed(currentSpeed);
                UpdateSpeedIndicator();
            }
        }
    }
}
