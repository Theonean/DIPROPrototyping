using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private float yPos;

    void Start()
    {
        yPos = transform.position.y;
    }

    void Update()
    {
        transform.position = new Vector3(ControlZoneManager.Instance.transform.position.x, yPos, ControlZoneManager.Instance.transform.position.z);
    }
}
