using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set; }
    [Header("Rotating Radar")]
    public float rotationSpeed = 170f;
    public float radarDistance = 150f;
    [Header("Raycast")]
    public LayerMask layerMask;

    [Header("Pulse")]
    public RadarPulseData radarData;
    private MapRevealer mapRevealer;
    public float pulseSeismoEmission = 5f;

    // Collider & Sprite
    public Transform pulseSpriteTransform;
    private SpriteRenderer pulseSpriteRenderer;

    public float signatureDelay = 0.01f;

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

        pulseSpriteRenderer = pulseSpriteTransform.GetComponent<SpriteRenderer>();
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

            Collider[] hitColliders = new Collider[100];
            int numColliders = Physics.OverlapSphereNonAlloc(
                transform.position,
                radarData.pulseRange * modifier,
                hitColliders,
                layerMask
            );

            if (numColliders > 0)
            {
                var sortedColliders = hitColliders
                    .Take(numColliders)
                    .OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                    .ToArray();

                StartCoroutine(DisplayPulseMarkers(sortedColliders, modifier));
                StartCoroutine(DisplayPulseRing(modifier));
            }

            mapRevealer.Pulse(
                radarData.pulseStartRange * revealStartRangeFactor,
                radarData.pulseRange * revealRangeFactor * modifier,
                radarData.pulseSpeed * revealSpeedFactor,
                radarData.pulseDuration
            );

            Seismograph.Instance.SetOtherEmission("Radar Pulse", pulseSeismoEmission, 1f);
        }
        else
        {
            Debug.LogWarning("Not enough Resources!");
        }
    }

    private IEnumerator DisplayPulseMarkers(Collider[] colliders, float modifier)
    {
        float maxDistance = radarData.pulseRange * modifier;
        float pulseDuration = radarData.pulseDuration;
        float startTime = Time.time;

        foreach (var collider in colliders)
        {
            if (collider == null) continue;

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            float normalizedDistance = distance / maxDistance;
            float delay = normalizedDistance * pulseDuration;

            // Wait until the pulse wave would reach this distance
            while (Time.time - startTime < delay)
            {
                yield return null;
            }

            if (collider.TryGetComponent<EnergySignature>(out EnergySignature signature))
            {
                Map.Instance.SetEnergySignature(new Vector3(collider.transform.position.x, 0, collider.transform.position.z), signature);
            }


            yield return new WaitForSeconds(signatureDelay);
        }
    }
    private IEnumerator DisplayPulseRing(float modifier) {
        pulseSpriteRenderer.enabled = true;
        float elapsedTime = 0f;
        while (elapsedTime < radarData.pulseDuration) {
            float normalizedTime = elapsedTime / radarData.pulseDuration;
            float currentSpeed = pulseSpeedCurve.Evaluate(normalizedTime) * radarData.pulseSpeed;
            float currentStrength = pulseStrengthCurve.Evaluate(normalizedTime / radarData.pulseDuration);

            pulseSpriteTransform.localScale = Vector3.one * Mathf.Lerp(radarData.pulseStartRange, radarData.pulseRange * modifier, normalizedTime); 
            pulseSpriteRenderer.color = new Color(pulseSpriteRenderer.color.r, pulseSpriteRenderer.color.g, pulseSpriteRenderer.color.b, currentStrength);
            
            elapsedTime += Time.deltaTime * currentSpeed;
            yield return null;
        }
    }
}
