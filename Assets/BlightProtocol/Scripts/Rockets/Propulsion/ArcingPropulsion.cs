using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ArcingPropulsion : ACRocketPropulsion
{
    public Vector2 minMaxArcHeight; // y value of the arc's peak, 
    public float maxDistance; // distance from the rocket to the TargetPosition
    [Header("Explosion Settings")]
    public Material explosionMaterial; //Material for the explosion effect when the rocket explodes.

    public GameObject explosionPrefab; //The explosion prefab to spawn when the rocket explodes.

    public float landingExplosionRadius = 10f; // Radius of the explosion when the rocket lands
    public override IEnumerator FlyToTargetPosition()
    {
        Logger.Log("Arcing propulsion activated", LogLevel.INFO, LogType.ROCKETS);

        Vector3 start = rocketTransform.position;
        Vector3 midPoint = (start + TargetPosition) * 0.5f;
        TargetPosition = new Vector3(TargetPosition.x, parentRocket.initialTransform.position.y, TargetPosition.z);

        float distance = Vector3.Distance(start, TargetPosition);
        midPoint.y += Mathf.Lerp(minMaxArcHeight.x, minMaxArcHeight.y, Mathf.Clamp01(distance / maxDistance));

        float progress = 0f;

        while (progress < 1f)
        {
            progress += (ParentRocket.settings.flySpeed / Vector3.Distance(start, TargetPosition)) * Time.deltaTime;
            float t = Mathf.Clamp01(progress);

            // Quadratic Bézier interpolation: (1 - t)^2 * A + 2(1 - t)t * B + t^2 * C
            Vector3 position =
                Mathf.Pow(1 - t, 2) * start +
                2 * (1 - t) * t * midPoint +
                Mathf.Pow(t, 2) * TargetPosition;

            rocketTransform.LookAt(position);
            rocketTransform.position = position;

            // Optional scale effect during flight
            rocketTransform.localScale = Vector3.Lerp(
                rocketTransform.localScale,
                rocketOriginalScale * ParentRocket.settings.flyScaleMultiplier,
                0.1f * Time.deltaTime * ParentRocket.settings.flySpeed
            );

            yield return null;
        }

        rocketTransform.position = TargetPosition;

        // Trigger explosion
        GameObject explosionEffect = Instantiate(explosionPrefab, rocketTransform.position, Quaternion.identity);
        explosionEffect.GetComponentInChildren<LegExplosionHandler>().SetExplosionRadius(landingExplosionRadius / 10f);

        Collider[] hitColliders = Physics.OverlapSphere(rocketTransform.position, landingExplosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("PL_IsEnemy") && hitCollider.CompareTag("Enemy"))
            {
                Vector3 directionToEnemy = hitCollider.transform.position - rocketTransform.position;
                //Debug raycast to check if the rocket is in line of sight to the enemy
                Debug.DrawRay(rocketTransform.position, directionToEnemy.normalized * directionToEnemy.magnitude, Color.red, 50f);
                if (Physics.Raycast(rocketTransform.position, directionToEnemy.normalized, directionToEnemy.magnitude))
                {
                    hitCollider.gameObject.GetComponent<EnemyDamageHandler>().DestroyEnemy();
                }
            }
            else if (hitCollider.gameObject.CompareTag("PL_IsRocket"))
            {
                Rocket rocket = hitCollider.gameObject.GetComponentInParent<Rocket>();
                if (rocket.CanExplode())
                {
                    StartCoroutine(DaisyChainExplosion(rocket));
                }
            }
        }

        Logger.Log("Rocket reached TargetPosition", LogLevel.INFO, LogType.ROCKETS);
        ParentRocket.SetState(RocketState.IDLE);
    }

    private IEnumerator DaisyChainExplosion(Rocket rocket)
    {
        yield return new WaitForSeconds(ParentRocket.settings.explosionChainDelay);
        rocket.Explode();
    }
}