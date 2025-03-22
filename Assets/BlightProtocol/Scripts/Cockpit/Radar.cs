using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set; }
    [Header("Rotating Radar")]
    public float rotationSpeed = 170f;
    public float radarDistance = 150f;
    [Header("Raycast")]
    public LayerMask layerMask;
    [Header("Ping")]
    public GameObject ping;

    [Header("Pulse")]
    public RadarData radarData;
    private MapRevealer mapRevealer;

    // Collider & Sprite
    public Transform pulseTransform;
    private SpriteRenderer pulseSpriteRenderer;

    // Duration

    [Header("Pulse Range")]
    public float revealRangeFactor = 1f;
    public float revealStartRangeFactor = 1f;
    

    [Header("Pulse Strength")]
    // Strength
    public AnimationCurve pulseStrengthCurve;

    [Header("Pulse Speed")]
    // Speed
    public float revealSpeedFactor = 1f;
    public AnimationCurve pulseSpeedCurve;

    private List<Collider> colliderList = new List<Collider>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        pulseSpriteRenderer = pulseTransform.GetComponent<SpriteRenderer>();
        mapRevealer = GetComponentInChildren<MapRevealer>();
        InitiateMapRevealer();
    }

    private void InitiateMapRevealer()
    {
        mapRevealer.pulseStrengthCurve = pulseStrengthCurve;
        mapRevealer.pulseSpeedCurve = pulseSpeedCurve;
    }

    public void Pulse()
    {
        if (ResourceHandler.Instance.CheckResource(radarData.pulseCostResource) > radarData.pulseCost)
        {
            ResourceHandler.Instance.ConsumeResource(radarData.pulseCostResource, radarData.pulseCost, false, 1f);
            StartCoroutine(PulseEffect());
            mapRevealer.Pulse(radarData.pulseStartRange * revealStartRangeFactor, radarData.pulseRange * revealRangeFactor, radarData.pulseSpeed * revealSpeedFactor, radarData.pulseDuration);
        }
        else
        {
            Debug.LogWarning("Not enough Resources!");
        }
    }

    private IEnumerator PulseEffect()
    {
        float timer = 0f;
        float linearTimer = 0f;

        while (timer < radarData.pulseDuration)
        {
            linearTimer += Time.deltaTime;

            float currentRadius = Mathf.Lerp(radarData.pulseStartRange, radarData.pulseRange, timer / radarData.pulseDuration);
            float currentStrength = pulseStrengthCurve.Evaluate(linearTimer / radarData.pulseDuration);
            float currentSpeed = pulseSpeedCurve.Evaluate(linearTimer / radarData.pulseDuration) * radarData.pulseSpeed;

            pulseTransform.localScale = Vector3.one * currentRadius;
            pulseSpriteRenderer.color = new Color(pulseSpriteRenderer.color.r, pulseSpriteRenderer.color.g, pulseSpriteRenderer.color.b, currentStrength);

            timer += Time.deltaTime * currentSpeed;
            yield return null;
        }
        pulseTransform.localScale = Vector3.one;
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 collisionPos = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        if (other.TryGetComponent<EnergySignature>(out EnergySignature signature)) {
            GameObject instantiatedPing = Instantiate(ping, new Vector3(collisionPos.x, 0, collisionPos.z), Quaternion.Euler(90, 0, 0));
            instantiatedPing.GetComponent<EnergySignatureDisplayer>().DisplaySignature(signature);
        }
    }

    public void Rotate()
    {
        float previousRotation = (transform.eulerAngles.y % 360) - 180;
        transform.eulerAngles -= new Vector3(0, rotationSpeed * Time.deltaTime, 0);
        float currentRotation = (transform.eulerAngles.y % 360) - 180;

        if (previousRotation < 0 && currentRotation > 0)
        {
            // half rotation
            colliderList.Clear();
        }

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, radarDistance, layerMask))
        {
            if (hit.collider != null)
            {
                /*if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
                {
                    Instantiate(terrainPing, hit.point, Quaternion.Euler(90, 0, 0));
                }*/
                if (!colliderList.Contains(hit.collider))
                {
                    colliderList.Add(hit.collider);
                    //Instantiate(enemyPing, hit.point, Quaternion.Euler(90, 0, 0));
                }
            }
        }
    }
}
