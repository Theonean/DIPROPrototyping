using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraTracker : MonoBehaviour
{
    public static CameraTracker Instance { get; private set; }
    public GameObject objectToTrack;
    public AnimationCurve cameraFollowCurve;
    public float cameraMoveSpeed = 20f;
    float m_MaxCameraDistance = 20f;
    public Vector3 cameraOffset;

    private Camera topDownCamera;
    [Header("Cinematic overrides")]
    public bool doMapFlyOver = false;
    public float flyUpTime = 4f;
    public AnimationCurve flyUpCurve;
    public float flyOverTime = 40f;
    public AnimationCurve flyOverCurve;
    public Vector3 flyOverDestination = new Vector3 (0, 0, 3000);
    public float cameraOffsetMultiplier = 2f;

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

        if (doMapFlyOver)
            StartCoroutine(FlyOverMap());
    }

    void FixedUpdate()
    {
        if (objectToTrack != null && !doMapFlyOver)
        {
            Vector3 targetPosition = objectToTrack.transform.position + cameraOffset;

            float distance = Vector3.Distance(topDownCamera.transform.position, targetPosition);
            float t = Mathf.Clamp(distance / m_MaxCameraDistance, 0, 1);

            //Move camera towards object
            topDownCamera.transform.position = Vector3.MoveTowards(
                topDownCamera.transform.position,
                targetPosition,
                cameraMoveSpeed * Time.fixedDeltaTime * cameraFollowCurve.Evaluate(t));

            if(Vector3.Distance(targetPosition, transform.position) > m_MaxCameraDistance * 4)
            {
                transform.position = targetPosition;
            }
        }
    }
    private IEnumerator FlyOverMap()
    {
        Vector3 flyUpStartPos = transform.position;
        Vector3 flyUpEndPos = cameraOffset * cameraOffsetMultiplier;

        topDownCamera.farClipPlane = 3000;

        float t = 0f;

        while (t < flyUpTime)
        {
            float progress = t / flyUpTime;
            float eased = flyUpCurve.Evaluate(progress);

            // Animate position with easing
            transform.position = Vector3.Lerp(flyUpStartPos, flyUpEndPos, eased);

            // Animate near clip plane
            topDownCamera.nearClipPlane = Mathf.Lerp(50, 300, eased);

            // Keep camera facing the object
            transform.LookAt(objectToTrack.transform);

            t += Time.deltaTime;
            yield return null;
        }

        // Smooth transition: now from flyUpEndPos to flyOverEndPos
        Vector3 flyOverStartPos = flyUpEndPos;
        Vector3 flyOverEndPos = flyOverStartPos + flyOverDestination;

        t = 0f;
        while (t < flyOverTime)
        {
            float progress = t / flyOverTime;

            // Optional: use same or a new curve for continuity
            float eased = flyOverCurve.Evaluate(progress);

            transform.position = Vector3.Lerp(flyOverStartPos, flyOverEndPos, eased);

            t += Time.deltaTime;
            yield return null;
        }
    }


}
