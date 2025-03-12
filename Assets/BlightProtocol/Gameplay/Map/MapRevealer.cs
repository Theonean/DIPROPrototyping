using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapRevealer : MonoBehaviour
{
    public static MapRevealer Instance {get; private set;}
    private Vector2 coords;
    public MapMask mapMask;

    [Header("Raycast")]
    private RaycastHit hit;
    public LayerMask layerMask;

    [Header("Continuous Reveal")]
    public float revealRadius = 50f;
    public float revealStrength = 1f;

    [Header("Pulse")]
    public float pulseStartRadius = 10f;
    public float pulseRange = 500f;
    public float pulseStrength = 1f;
    public AnimationCurve pulseStrengthCurve;
    public float pulseSpeed = 100f;
    public AnimationCurve pulseSpeedCurve;   
    public float pulseDuration = 1.0f;

    void Start() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Update()
    {
        GetMapCoordinates();
        if (coords != null) {
            mapMask.PaintOnMask(coords, revealRadius, revealStrength);
        }
    }

    void GetMapCoordinates()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
        {
            coords = new Vector2(hit.textureCoord.x, hit.textureCoord.y);
        }
    }

    public void Pulse()
    {
        GetMapCoordinates();
        Radar.Instance.Pulse(pulseRange, pulseSpeed);
        StartCoroutine(PulseEffect(coords));
    }

    private IEnumerator PulseEffect(Vector2 coords)
    {

        float timer = 0f;
        float linearTimer = 0f;

        while (timer < pulseDuration)
        {
            linearTimer += Time.deltaTime;

            float currentRadius = Mathf.Lerp(pulseStartRadius, pulseRange, timer / pulseDuration);
            float currentStrength = pulseStrengthCurve.Evaluate(linearTimer/pulseDuration) * pulseStrength;
            float currentSpeed = pulseSpeedCurve.Evaluate(linearTimer/pulseDuration) * pulseSpeed;

            mapMask.PaintOnMask(coords, currentRadius, currentStrength);

            timer += Time.deltaTime * currentSpeed;
            yield return null;
        }
    }

}
