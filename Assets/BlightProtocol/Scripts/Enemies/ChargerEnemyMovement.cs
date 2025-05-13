using System.Collections;
using UnityEngine;

public class ChargerEnemyMovement : ACEnemyMovementBehaviour
{
    [SerializeField] private float chargeDistance = 10f;
    [SerializeField] private float chargeSpeed = 20f;
    [SerializeField] private float chargeWindupTime = 1f;
    [SerializeField] private AnimationCurve chargeSpeedCurve;
    private bool charging = false;
    [SerializeField] private Color chargeStartColor;
    [ColorUsage(true, true)]
    [SerializeField] private Color chargeEndColor;
    [SerializeField] private Renderer batteryRenderer;

    void Start()
    {
        navMeshAgent.acceleration = chargeSpeed * 5f;
    }

    protected override void CustomMovementUpdate()
    {
        if (charging)
            return;

        if (CanMove())
        {
            SetDestination(harvester.transform.position);

            float distanceToHarvester = Vector3.Distance(transform.position, harvester.transform.position);
            if (distanceToHarvester < chargeDistance)
            {
                charging = true;
                StopMovement();
                StartCoroutine(ChargeWindup());
            }
        }
    }

    private IEnumerator ChargeWindup()
    {
        float elapsedTime = 0f;
        float blinkTime = 1f;

        while (elapsedTime < chargeWindupTime - blinkTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeWindupTime;
            batteryRenderer.material.color = Color.Lerp(chargeStartColor, chargeEndColor, t);

            yield return null;
        }
        StartCoroutine(Blink(blinkTime));
    }

    private IEnumerator Blink(float timeLeft)
    {
        float elapsedTime = 0f;

        while (elapsedTime < timeLeft)
        {
            batteryRenderer.material.color = chargeStartColor;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;

            batteryRenderer.material.color = chargeEndColor;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        ResumeMovement();
        StartCoroutine(SetChargeSpeed());
    }

    private IEnumerator SetChargeSpeed()
    {
        Vector3 targetPosition = harvester.transform.position;
        Vector3 startingPosition = transform.position;
        float distanceToTarget = Vector3.Distance(startingPosition, targetPosition);
        float timeToCharge = distanceToTarget / chargeSpeed;

        for (float t = 0; t < timeToCharge; t += Time.deltaTime)
        {
            float normalizedTime = t / timeToCharge;
            float speed = chargeSpeed * chargeSpeedCurve.Evaluate(normalizedTime);
            SetSpeed(speed);
            yield return null;
        }
    }
}
