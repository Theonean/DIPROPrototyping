using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTracker : MonoBehaviour
{
    public static CameraTracker Instance { get; private set; }
    public bool trackObjectWithCamera = false;
    public GameObject objectToTrack;
    public GameObject player;
    public GameObject harvester;
    public GameObject harvesterCameraPos;
    public GameObject arrowRotator;
    public GameObject controlZone;
    public SpriteRenderer arrowSprite;
    public AnimationCurve cameraFollowCurve;
    public float maxDistanceFromHarvester;
    public float cameraMoveSpeed = 20f;
    public GameObject OutOfRangeUIGroup;
    public CanvasGroup outOfRangeUIGroupCanvas; // CanvasGroup for controlling the alpha of the UI
    public TextMeshProUGUI countdownUntilRespawnText;
    public float countdownUntilRespawnTime;
    private float m_RespawnTimer = 0f;
    private bool m_PlayerInRange = true;
    float m_MaxCameraDistance = 2f;
    private Vector3 cameraOffset;
    private PlayerCore playerCore;

    private const float fadeStartDistance = 20f; // Start fading when player is this close to the max distance

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
    }

    private void Start()
    {
        // Save the initial offset between camera and object
        cameraOffset = Camera.main.transform.position - player.transform.position;
        playerCore = GetComponentInChildren<PlayerCore>();

    }

    private void Update()
    {
        if (!m_PlayerInRange && !ControlZoneManager.Instance.GetZoneState().Equals(ZoneState.DIED))
        {
            m_RespawnTimer -= Time.deltaTime;
            countdownUntilRespawnText.text = m_RespawnTimer.ToString("F2");
            if (m_RespawnTimer <= 0f)
            {
                SetIsPlayerInRange(true);
                //Make the player die
                playerCore.ModifyHealth(-100);
            }
        }
        /*

        if (Input.GetKey(KeyCode.F))
        {
            objectToTrack = harvester;
        }
        if (Input.GetKey(KeyCode.E))
        {
            objectToTrack = player;
        }*/
    }

    void FixedUpdate()
    {
        if (trackObjectWithCamera && objectToTrack != null)
        {
            Vector3 targetPosition;
            if (objectToTrack == harvester)
            {
                targetPosition = harvesterCameraPos.transform.position;
                Camera.main.transform.rotation = harvesterCameraPos.transform.rotation;
            }
            else
            {
                targetPosition = player.transform.position + cameraOffset;
                Camera.main.transform.LookAt(player.transform.position);
            }

            float distance = Vector3.Distance(Camera.main.transform.position, targetPosition);
            float t = Mathf.Clamp(distance / m_MaxCameraDistance, 0, 1);

            //Move camera towards object
            Camera.main.transform.position = Vector3.MoveTowards(
                Camera.main.transform.position,
                targetPosition,
                cameraMoveSpeed * Time.fixedDeltaTime * cameraFollowCurve.Evaluate(t));
        }

        // Rotate arrow to face the control zone, only when the harvester is not dead
        if (!ControlZoneManager.Instance.GetZoneState().Equals(ZoneState.DIED))
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
        //Disable logic when control zone died
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
