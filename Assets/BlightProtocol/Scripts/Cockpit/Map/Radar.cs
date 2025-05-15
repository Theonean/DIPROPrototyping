using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set; }
    [Header("Raycast")]
    public LayerMask layerMask;

    [Header("Pulse")]
    public RadarPulseData radarData;
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
    }

    public void Pulse(float modifier)
    {
        Collider[] hitColliders = Physics.OverlapSphere(
            transform.position,
            radarData.pulseRange * modifier,
            layerMask
        );
        if (hitColliders.Length > 0)
        {
            var sortedColliders = hitColliders
                .Take(hitColliders.Length)
                .OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                .ToArray();

            StartCoroutine(DisplayPulseMarkers(sortedColliders, modifier));
            StartCoroutine(DisplayPulseRing(modifier));
        }

        Seismograph.Instance.SetOtherEmission("Radar Pulse", pulseSeismoEmission, 1f);
    }

    private IEnumerator DisplayPulseMarkers(Collider[] colliders, float modifier)
    {
        float maxDistance = radarData.pulseRange * modifier;
        float pulseDuration = radarData.pulseDuration;
        float startTime = Time.time;

        foreach (var collider in colliders)
        {
            if (collider == null) continue;

            EnergySignature signature = collider.GetComponent<EnergySignature>();
            if (signature == null) continue;

            float distance = Vector3.Distance(transform.position, collider.transform.position);
            float normalizedDistance = distance / maxDistance;
            float delay = normalizedDistance * pulseDuration;

            // Wait until the pulse wave would reach this distance
            while (Time.time - startTime < delay)
            {
                yield return null;
            }

            Map.Instance.SetEnergySignature(new Vector3(collider.transform.position.x, 0.1f, collider.transform.position.z), signature);


            yield return new WaitForSeconds(signatureDelay);
        }
    }
    private IEnumerator DisplayPulseRing(float modifier)
    {
        pulseSpriteRenderer.enabled = true;
        float elapsedTime = 0f;
        while (elapsedTime < radarData.pulseDuration)
        {
            float normalizedTime = elapsedTime / radarData.pulseDuration;
            float currentSpeed = pulseSpeedCurve.Evaluate(normalizedTime) * radarData.pulseSpeed;
            float currentStrength = pulseStrengthCurve.Evaluate(normalizedTime / radarData.pulseDuration);

            pulseSpriteTransform.localScale = Vector3.one * Mathf.Lerp(radarData.pulseStartRange, radarData.pulseRange * modifier, normalizedTime);
            pulseSpriteRenderer.color = new Color(pulseSpriteRenderer.color.r, pulseSpriteRenderer.color.g, pulseSpriteRenderer.color.b, currentStrength);

            elapsedTime += Time.deltaTime * currentSpeed;
            yield return null;
        }
        pulseSpriteRenderer.color = new Color(pulseSpriteRenderer.color.r, pulseSpriteRenderer.color.g, pulseSpriteRenderer.color.b, 0f);
    }
}
