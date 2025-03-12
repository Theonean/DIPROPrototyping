using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radar : MonoBehaviour
{
    public static Radar Instance { get; private set;}
    public float rotationSpeed = 180f;
    public float radarDistance = 150f;
    public LayerMask layerMask;
    public GameObject enemyPing;
    public GameObject terrainPing;

    [Header("Pulse")]
    public Transform pulseTransform;
    private SpriteRenderer pulseSpriteRenderer;
    public float pulseStartRadius = 10f;
    public float pulseRangeFactor = 1f;
    public AnimationCurve pulseStrengthCurve;
    public float pulseSpeedFactor = 1f;
    public AnimationCurve pulseSpeedCurve;   
    public float pulseDuration = 1.0f;

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
    }

    // Update is called once per frame
    void Update()
    {
        /*float previousRotation = (transform.eulerAngles.y % 360) - 180;
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
                }
                else if (!colliderList.Contains(hit.collider))
                {
                    colliderList.Add(hit.collider);
                    Instantiate(enemyPing, hit.point, Quaternion.Euler(90, 0, 0));
                }
            }
        }*/
    }

    public void Pulse(float range, float speed) {
        StartCoroutine(PulseEffect(range, speed));
    }

    private IEnumerator PulseEffect(float range, float speed)
    {

        float timer = 0f;
        float linearTimer = 0f;

        while (timer < pulseDuration)
        {
            linearTimer += Time.deltaTime;

            float currentRadius = Mathf.Lerp(pulseStartRadius, range*pulseRangeFactor, timer / pulseDuration);
            float currentStrength = pulseStrengthCurve.Evaluate(linearTimer/pulseDuration);
            float currentSpeed = pulseSpeedCurve.Evaluate(linearTimer/pulseDuration) * speed * pulseSpeedFactor;

            pulseTransform.localScale = Vector3.one * currentRadius;
            pulseSpriteRenderer.color = new Color(pulseSpriteRenderer.color.r, pulseSpriteRenderer.color.g, pulseSpriteRenderer.color.b, currentStrength);

            if (Physics.SphereCast(new Ray(Vector3.up, transform.position), currentRadius, out RaycastHit hit, 0.1f, layerMask)) {
                Instantiate(enemyPing, hit.point, Quaternion.Euler(90, 0, 0));
            };

            timer += Time.deltaTime * currentSpeed;
            yield return null;
        }
        pulseTransform.localScale = Vector3.one;
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 collisionPos = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
        Instantiate(enemyPing, new Vector3(collisionPos.x, 0, collisionPos.z), Quaternion.Euler(90, 0, 0));
    }
}
