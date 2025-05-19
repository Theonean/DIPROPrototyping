using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraTracker : MonoBehaviour
{
    public static CameraTracker Instance { get; private set; }
    public GameObject objectToTrack;
    public AnimationCurve cameraFollowCurve;
    public float cameraMoveSpeed = 20f;
    float m_MaxCameraDistance = 2f;
    private Vector3 cameraOffset;

    private Camera topDownCamera;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        topDownCamera = GetComponent<Camera>();
        cameraOffset = topDownCamera.transform.position - objectToTrack.transform.position;
    }

    void FixedUpdate()
    {
        if (objectToTrack != null)
        {
            Vector3 targetPosition = objectToTrack.transform.position + cameraOffset;

            float distance = Vector3.Distance(topDownCamera.transform.position, targetPosition);
            float t = Mathf.Clamp(distance / m_MaxCameraDistance, 0, 1);

            //Move camera towards object
            topDownCamera.transform.position = Vector3.MoveTowards(
                topDownCamera.transform.position,
                targetPosition,
                cameraMoveSpeed * Time.fixedDeltaTime * cameraFollowCurve.Evaluate(t));

            if(Vector3.Distance(targetPosition, transform.position) > m_MaxCameraDistance * 2)
            {
                transform.position = targetPosition;
            }
        }
    }
}
