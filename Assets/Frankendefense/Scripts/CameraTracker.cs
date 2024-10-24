using TMPro;
using Unity.VisualScripting;
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
    public float maxDistanceFromHarvester;
    public GameObject OutOfRangeUIGroup;
    public CanvasGroup outOfRangeUIGroupCanvas; // CanvasGroup for controlling the alpha of the UI
    public TextMeshProUGUI countdownUntilRespawnText;
    public float countdownUntilRespawnTime;
    private float m_RespawnTimer = 0f;
    private bool m_PlayerInRange = true;
    float m_MaxCameraDistance = 2f;
    private Vector3 cameraOffset;

    private const float fadeStartDistance = 20f; // Start fading when player is this close to the max distance

    private void Start()
    {
        // Save the initial offset between camera and object
        cameraOffset = Camera.main.transform.position - objectToTrack.transform.position;
    }

    private void Update()
    {
        if (!m_PlayerInRange)
        {
            m_RespawnTimer -= Time.deltaTime;
            countdownUntilRespawnText.text = m_RespawnTimer.ToString("F2");
            if (m_RespawnTimer <= 0f)
            {
                SetIsPlayerInRange(true);
                ControlZoneManager.Instance.Die();
            }
        }
    }

    void FixedUpdate()
    {
        if (trackObjectWithCamera)
        {
            Vector3 targetPosition = objectToTrack.transform.position + cameraOffset;
            float distance = Vector3.Distance(Camera.main.transform.position, targetPosition);
            float t = Mathf.Clamp(distance / m_MaxCameraDistance, 0, 1);

            //Move camera towards object
            Camera.main.transform.position = Vector3.MoveTowards(
                Camera.main.transform.position,
                targetPosition,
                20f * Time.fixedDeltaTime * cameraFollowCurve.Evaluate(t));

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

        // Rotate arrow to face the control zone
        if (arrowRotator != null && controlZone != null)
        {
            arrowRotator.transform.LookAt(controlZone.transform.position);

            float distanceToControlZone = Vector3.Distance(arrowRotator.transform.position, controlZone.transform.position);

            // When near the control zone, make the arrow invisible and handle UI fade
            if (distanceToControlZone < maxDistanceFromHarvester - fadeStartDistance)
            {
                SetIsPlayerInRange(true);
            }
            else
            {
                SetIsPlayerInRange(false);
            }

            // Fade the UI based on the player's distance from the control zone
            HandleUIFade(distanceToControlZone);
        }
    }

    private void SetIsPlayerInRange(bool isInRange)
    {
        // Only run the logic when the player's range status changes
        if (m_PlayerInRange != isInRange)
        {
            if (isInRange)
            {
                // Player is now in range
                OutOfRangeUIGroup.SetActive(false);
                arrowSprite.enabled = false;
                m_PlayerInRange = true;
            }
            else
            {
                // Player is out of range
                OutOfRangeUIGroup.SetActive(true);
                arrowSprite.enabled = true;
                m_PlayerInRange = false;

                // Start the respawn timer when the player goes out of range
                m_RespawnTimer = countdownUntilRespawnTime;
            }
        }
    }

    private void HandleUIFade(float distanceToControlZone)
    {
        // Calculate the starting distance for the fade (when to start fading in the UI)
        float fadeStart = maxDistanceFromHarvester - fadeStartDistance;

        // If the player is within the fade range, start fading in the UI
        if (distanceToControlZone >= fadeStart && distanceToControlZone <= maxDistanceFromHarvester)
        {
            // Calculate the fade factor (0 when just entering fade range, 1 when out of range)
            float fadeFactor = (distanceToControlZone - fadeStart) / (maxDistanceFromHarvester - fadeStart);
            outOfRangeUIGroupCanvas.alpha = Mathf.Clamp01(fadeFactor); // Fade UI based on distance
        }
        else if (distanceToControlZone > maxDistanceFromHarvester)
        {
            // Ensure UI is fully visible when the player is out of range
            outOfRangeUIGroupCanvas.alpha = 1f;
        }
        else
        {
            // Ensure UI is hidden when the player is close to the control zone
            outOfRangeUIGroupCanvas.alpha = 0f;
        }
    }
}
