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
            SetDestination(target.transform.position);

            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);
            if (distanceToTarget < chargeDistance)
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
        bool targetInRange = true;

        while (elapsedTime < timeLeft)
        {
            targetInRange = IsTargetInRange();
            if (!targetInRange) break;

            batteryRenderer.material.color = chargeStartColor;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;

            batteryRenderer.material.color = chargeEndColor;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        ResumeMovement();

        if (!targetInRange) 
            charging = false;
        else
            StartCoroutine(SetChargeSpeed());
    }

    private IEnumerator SetChargeSpeed()
    {
        Vector3 targetPosition = target.transform.position;
        Vector3 startingPosition = transform.position;
        float distanceToTarget = Vector3.Distance(startingPosition, targetPosition);
        float timeToCharge = distanceToTarget / chargeSpeed;

        float t = 0;
        while (IsTargetInRange())
        {
            t += Time.deltaTime;
            float normalizedProgress = 1f - Vector3.Distance(transform.position, target.transform.position) / chargeDistance;
            float speed = chargeSpeed * chargeSpeedCurve.Evaluate(normalizedProgress);
            SetSpeed(speed);
            SetDestination(target.transform.position);
            yield return null;
        }

        SetSpeed(moveSpeed);
        charging = false;
    }

    private bool IsTargetInRange()
    {
        return Vector3.Distance(target.transform.position, transform.position) < chargeDistance;
    }
}
