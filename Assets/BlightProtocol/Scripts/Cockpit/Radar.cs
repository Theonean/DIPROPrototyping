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
    public RadarPulseData radarData;
    private MapRevealer mapRevealer;
    public float pulseSeismoEmission = 5f;

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

    public void Pulse(float modifier)
    {
        if (ResourceHandler.Instance.CheckResource(radarData.pulseCostResource) > radarData.pulseCost)
        {
            ResourceHandler.Instance.ConsumeResource(radarData.pulseCostResource, radarData.pulseCost, false, 1f);
            StartCoroutine(PulseEffect(modifier));
            mapRevealer.Pulse(radarData.pulseStartRange * revealStartRangeFactor, radarData.pulseRange * revealRangeFactor * modifier, radarData.pulseSpeed * revealSpeedFactor, radarData.pulseDuration);

            Seismograph.Instance.SetOtherEmission("Radar Pulse", pulseSeismoEmission, 1f);
        }
        else
        {
            Debug.LogWarning("Not enough Resources!");
        }
    }

    private IEnumerator PulseEffect(float modifier)
    {
        float timer = 0f;
        float linearTimer = 0f;

        float range = radarData.pulseRange * modifier;

        while (timer < radarData.pulseDuration)
        {
            linearTimer += Time.deltaTime;

            float currentRadius = Mathf.Lerp(radarData.pulseStartRange, range, timer / radarData.pulseDuration);
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
            Map.Instance.SetEnergySignature(collisionPos, signature);
        }
    }
}
