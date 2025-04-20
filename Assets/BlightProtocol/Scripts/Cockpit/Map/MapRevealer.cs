using System.Collections;
using UnityEngine;

public class MapRevealer : MonoBehaviour
{
    public MapMask mapMask;
    private RaycastHit hit;
    public LayerMask layerMask;

    [Header("Continuous Reveal")]
    public float revealRadius = 50f;
    public float revealStrength = 1f;

    [Header("Pulse")]
    public float pulseStrength = 1f;
    public AnimationCurve pulseStrengthCurve;
    public AnimationCurve pulseSpeedCurve;

    void Update()
    {
        mapMask.Paint(transform.position, revealRadius, revealStrength);
    }

    public void Pulse(float startRange, float range, float speed, float duration)
    {
        StartCoroutine(PulseEffect(startRange, range, speed, duration));
    }

    private IEnumerator PulseEffect(float startRange, float range, float speed, float duration)
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

            mapMask.Paint(transform.position, currentRadius, currentStrength);

            timer += Time.deltaTime * currentSpeed;
            yield return null;
        }
    }

}
