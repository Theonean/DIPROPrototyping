using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVPlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
    }
    private void LateUpdate()
    {
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.localRotation = Quaternion.Euler(0, yRotation, 0);
    }
}
