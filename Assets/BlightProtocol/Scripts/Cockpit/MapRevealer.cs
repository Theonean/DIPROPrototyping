using System.Collections;
using UnityEngine;

public class MapRevealer : MonoBehaviour
{
    private Vector2 coords;
    public MapMask mapMask;
    private RaycastHit hit;
    public LayerMask layerMask;
    public float edgeSharpness = 5f;

    [Header("Continuous Reveal")]
    public float revealRadius = 50f;
    public float revealStrength = 1f;

    [Header("Pulse")]
    public float pulseStrength = 1f;
    public AnimationCurve pulseStrengthCurve;
    public AnimationCurve pulseSpeedCurve;

    void Awake()
    {
        if (mapMask != null)
        {
            mapMask.edgeSharpness = 5f;
        }
    }

    void Update()
    {
        GetMapCoordinates();
        if (coords != null)
        {
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

    public void Pulse(float startRange, float range, float speed, float duration)
    {
        GetMapCoordinates();
        StartCoroutine(PulseEffect(coords, startRange, range, speed, duration));
    }

    private IEnumerator PulseEffect(Vector2 coords, float startRange, float range, float speed, float duration)
    {
        //yield return new WaitForSeconds(0.5f);
        float timer = 0f;
        float linearTimer = 0f;

        while (timer < duration)
        {
            linearTimer += Time.deltaTime;

            float currentRadius = Mathf.Lerp(startRange, range, timer / duration);
            float currentStrength = pulseStrengthCurve.Evaluate(linearTimer / duration) * pulseStrength;
            float currentSpeed = pulseSpeedCurve.Evaluate(linearTimer / duration) * speed;

            mapMask.PaintOnMask(coords, currentRadius, currentStrength);

            timer += Time.deltaTime * currentSpeed;
            yield return null;
        }
    }

}
