using System.Collections;
using UnityEngine;

public class ChargerEnemyMovement : ACEnemyMovementBehaviour
{
    [SerializeField] private float chargeDistance = 10f;
    [SerializeField] private float chargeSpeed = 20f;
    [SerializeField] private float chargeWindupTime = 1f;
    [SerializeField] private AnimationCurve chargeSpeedCurve;
    private bool charging = false;

    void Start()
    {
        navMeshAgent.acceleration = chargeSpeed*5f;
    }

    protected override void Update()
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
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Color startColor = Color.white;
        Color endColor = Color.red;

        while (elapsedTime < chargeWindupTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeWindupTime;

            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = Color.Lerp(startColor, endColor, t);
            }

            yield return null;
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
