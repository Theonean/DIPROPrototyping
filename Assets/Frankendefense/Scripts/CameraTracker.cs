using TMPro;
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
    public TextMeshProUGUI countdownUntilRespawnText;
    public float countdownUntilRespawnTime;
    private float m_RespawnTimer = 0f;
    private bool m_PlayerInRange = false;
    float m_MaxCameraDistance = 2f;
    private Vector3 cameraOffset;

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
                ControlZoneManager.Instance.Die();
                SetIsPlayerInRange(true);
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

        //Rotate arrow to face the control zone
        if (arrowRotator != null && controlZone != null)
        {
            arrowRotator.transform.LookAt(controlZone.transform.position);

            //When near the control zone, make the arrow invisible
            if (Vector3.Distance(arrowRotator.transform.position, controlZone.transform.position) < maxDistanceFromHarvester)
            {
                SetIsPlayerInRange(true);
            }
            else
            {
                SetIsPlayerInRange(false);
            }
        }
    }

    private void SetIsPlayerInRange(bool isInRange)
    {
        m_PlayerInRange = isInRange;

        if (m_PlayerInRange)
        {
            OutOfRangeUIGroup.SetActive(false);
            m_PlayerInRange = true;
            arrowSprite.enabled = false;
        }
        else
        {
            OutOfRangeUIGroup.SetActive(true);
            m_PlayerInRange = false;
            arrowSprite.enabled = true;
            m_RespawnTimer = countdownUntilRespawnTime;
        }
    }
}
