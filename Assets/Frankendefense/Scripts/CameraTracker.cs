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
    public AnimationCurve cameraFollowCurve;
    public bool allowCameraScroll = false;
    float m_MaxDistance = 2f;

    private void Start()
    {
        //Set camera position
        Camera.main.transform.position = objectToTrack.transform.position + Vector3.up * cameraHeight;
    }

    void Update()
    {
        if (trackObjectWithCamera)
        {
            float distance = Vector3.Distance(Camera.main.transform.position, objectToTrack.transform.position + Vector3.up * cameraHeight);
            float t = Mathf.Clamp(distance / m_MaxDistance, 0, 1);

            //Move camera towards object
            Camera.main.transform.position = Vector3.MoveTowards(
                Camera.main.transform.position,
                objectToTrack.transform.position + Vector3.up * cameraHeight,
                20f * Time.deltaTime * cameraFollowCurve.Evaluate(t));




            if (allowCameraScroll)
            {
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
        }

        //Rotate arrow to face the control zone
        if (arrowRotator != null)
        {
            arrowRotator.transform.LookAt(controlZone.transform.position);

            //When near the control zone, make the arrow invisible
            if (Vector3.Distance(arrowRotator.transform.position, controlZone.transform.position) < 30f)
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
