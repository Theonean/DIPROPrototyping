using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public bool trackObjectWithCamera = false;
    public float cameraHeight = 7f;
    public GameObject objectToTrack;
    public GameObject arrowRotator;
    public GameObject controlZone;
    public SpriteRenderer arrowSprite;

    void Update()
    {
        if (trackObjectWithCamera)
        {
            Camera.main.transform.position = objectToTrack.transform.position + Vector3.up * cameraHeight;

            //Scroll wheel to zoom in and out
            if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            {
                cameraHeight -= 0.3f;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
            {
                cameraHeight += 0.3f;
            }
        }

        //Rotate arrow to face the control zone
        if (arrowRotator != null)
        {
            arrowRotator.transform.LookAt(controlZone.transform.position);

            //When near the control zone, make the arrow invisible
            if (Vector3.Distance(arrowRotator.transform.position, controlZone.transform.position) < 1f)
            {
                arrowSprite.enabled = false;
            }
            else
            {
                arrowSprite.enabled = true;
            }
        }
    }
}
