using TMPro;
using UnityEngine;

public class PlayerOutOfRangeHandler : MonoBehaviour
{
    public SpriteRenderer arrowSprite;
    public GameObject OutOfRangeUIGroup;
    public CanvasGroup outOfRangeUIGroupCanvas;
    public TextMeshProUGUI countdownUntilRespawnText;
    public float countdownUntilRespawnTime;
    public float maxDistanceFromHarvester;

    private float m_RespawnTimer = 0f;
    private bool m_PlayerInRange = true;

    private const float fadeStartDistance = 20f;
    private DroneMovement droneMovement;
    private Harvester harvester;
    private TutorialManager tutorialManager;

    void Start()
    {
        droneMovement = GetComponentInChildren<DroneMovement>();
        harvester = Harvester.Instance;
        tutorialManager = TutorialManager.Instance;
    }

    private void Update()
    {
        if (!m_PlayerInRange && !harvester.GetZoneState().Equals(HarvesterState.DIED))
        {
            m_RespawnTimer -= Time.deltaTime;
            countdownUntilRespawnText.text = m_RespawnTimer.ToString("F2");

            if (m_RespawnTimer <= 0f)
            {
                SetIsPlayerInRange(true);
                PlayerCore.Instance.ModifyHealth(-100);
            }
        }
    }

    private void FixedUpdate()
    {
        if (harvester.GetZoneState() == HarvesterState.DIED) return;
        if (tutorialManager.IsTutorialOngoing()) return;

        float distanceToControlZone = Vector3.Distance(droneMovement.transform.position, harvester.transform.position);

        if (distanceToControlZone < maxDistanceFromHarvester - fadeStartDistance)
        {
            SetIsPlayerInRange(true);
        }
        else
        {
            SetIsPlayerInRange(false);
        }

        HandleUIFade(distanceToControlZone);
    }

    private void SetIsPlayerInRange(bool isInRange)
    {
        if (m_PlayerInRange != isInRange)
        {
            if (isInRange)
            {
                OutOfRangeUIGroup.SetActive(false);
                arrowSprite.enabled = false;
                m_PlayerInRange = true;
            }
            else
            {
                OutOfRangeUIGroup.SetActive(true);
                arrowSprite.enabled = true;
                m_PlayerInRange = false;
                m_RespawnTimer = countdownUntilRespawnTime;
            }
        }
    }

    private void HandleUIFade(float distanceToControlZone)
    {
        float fadeStart = maxDistanceFromHarvester - fadeStartDistance;

        if (distanceToControlZone >= fadeStart && distanceToControlZone <= maxDistanceFromHarvester)
        {
            float fadeFactor = (distanceToControlZone - fadeStart) / (maxDistanceFromHarvester - fadeStart);
            outOfRangeUIGroupCanvas.alpha = Mathf.Clamp01(fadeFactor);
        }
        else if (distanceToControlZone > maxDistanceFromHarvester)
        {
            outOfRangeUIGroupCanvas.alpha = 1f;
        }
        else
        {
            outOfRangeUIGroupCanvas.alpha = 0f;
        }
    }
}
