using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set; }
    [Header("Rotating Radar")]
    public float rotationSpeed = 180f;
    public float radarDistance = 150f;
    [Header("Raycast")]
    public LayerMask layerMask;
    [Header("Ping")]
    public GameObject enemyPing;
    public GameObject resourcePointPing;

    [Header("Pulse")]
    public float pulseCost = 100f;
    public ResourceData pulseCostResource;
    private MapRevealer mapRevealer;

    // Collider & Sprite
    public Transform pulseTransform;
    private SpriteRenderer pulseSpriteRenderer;

    // Duration
    public float pulseDuration = 1.0f;

    [Header("Pulse Range")]
    // Range
    public float pulseRange = 500f;
    public float revealRangeFactor = 1f;
    public float pulseStartRange = 10f;
    public float revealStartRangeFactor = 1f;
    

    [Header("Pulse Strength")]
    // Strength
    public AnimationCurve pulseStrengthCurve;

    [Header("Pulse Speed")]
    // Speed
    public float pulseSpeed = 100f;
    public float revealSpeedFactor = 1f;
    public AnimationCurve pulseSpeedCurve;

    private List<Collider> colliderList = new List<Collider>();

    void Start()
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
        if (ResourceHandler.Instance.CheckResource(pulseCostResource) > pulseCost)
        {
            ResourceHandler.Instance.ConsumeResource(pulseCostResource, pulseCost, false, 1f);
            StartCoroutine(PulseEffect());
            mapRevealer.Pulse(pulseStartRange * revealStartRangeFactor, pulseRange * revealRangeFactor, pulseSpeed * revealSpeedFactor, pulseDuration);
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

        while (timer < pulseDuration)
        {
            linearTimer += Time.deltaTime;

            float currentRadius = Mathf.Lerp(pulseStartRange, pulseRange, timer / pulseDuration);
            float currentStrength = pulseStrengthCurve.Evaluate(linearTimer / pulseDuration);
            float currentSpeed = pulseSpeedCurve.Evaluate(linearTimer / pulseDuration) * pulseSpeed;

            pulseTransform.localScale = Vector3.one * currentRadius;
            pulseSpriteRenderer.color = new Color(pulseSpriteRenderer.color.r, pulseSpriteRenderer.color.g, pulseSpriteRenderer.color.b, currentStrength);

            if (Physics.SphereCast(new Ray(Vector3.up, transform.position), currentRadius, out RaycastHit hit, 0.1f, layerMask))
            {
                Instantiate(enemyPing, hit.point, Quaternion.Euler(90, 0, 0));
            }
            ;

            timer += Time.deltaTime * currentSpeed;
            yield return null;
        }
        pulseTransform.localScale = Vector3.one;
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 collisionPos = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        Debug.Log(other.tag);
        switch (other.tag)
        {
            case "Enemy":
                Instantiate(enemyPing, new Vector3(collisionPos.x, 0, collisionPos.z), Quaternion.Euler(90, 0, 0));
                break;

            case "ResourcePoint":
                Instantiate(resourcePointPing, new Vector3(collisionPos.x, 0, collisionPos.z), Quaternion.Euler(90, 0, 0));
                break;
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
                    Instantiate(enemyPing, hit.point, Quaternion.Euler(90, 0, 0));
                }
            }
        }
    }
}
