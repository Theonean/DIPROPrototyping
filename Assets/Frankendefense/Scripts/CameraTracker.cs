using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public bool trackObjectWithCamera = false;
    public GameObject objectToTrack;
    public GameObject arrowRotator;
    public GameObject controlZone;
    public SpriteRenderer arrowSprite;
    public AnimationCurve cameraFollowCurve;
    public bool allowCameraScroll = false;
    float m_MaxDistance = 2f;
    private Vector3 cameraOffset;

    private void Start()
    {
        // Save the initial offset between camera and object
        cameraOffset = Camera.main.transform.position - objectToTrack.transform.position;
    }

    void Update()
    {
        if (trackObjectWithCamera)
        {
            Vector3 targetPosition = objectToTrack.transform.position + cameraOffset;
            float distance = Vector3.Distance(Camera.main.transform.position, targetPosition);
            float t = Mathf.Clamp(distance / m_MaxDistance, 0, 1);

            //Move camera towards object
            Camera.main.transform.position = Vector3.MoveTowards(
                Camera.main.transform.position,
                targetPosition,
                20f * Time.deltaTime * cameraFollowCurve.Evaluate(t));

            if (allowCameraScroll)
            {
                //Scroll wheel to zoom in and out
                if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    cameraOffset.y -= 0.3f;
                }
                else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
                {
                    cameraOffset.y += 0.3f;
                }
            }
        }

        //Rotate arrow to face the control zone
        if (arrowRotator != null && controlZone != null)
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
