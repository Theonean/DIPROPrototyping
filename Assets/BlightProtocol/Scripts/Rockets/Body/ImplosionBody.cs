using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ImplosionBody : ACRocketBody
{
    public AnimationCurve moveToCenterCurve;
    protected override void Explode()
    {
        //Create explosion effect
        GameObject explosionEffect = Instantiate(explosionPrefab, rocketTransform.position, Quaternion.identity);
        explosionEffect.GetComponentInChildren<LegExplosionHandler>().SetExplosionRadius(explosionRadius / 10);

        if (debugExplosionSphere)
        {
            //Create gizmo with explosionradius, for debugging
            GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gizmo.transform.position = rocketTransform.position;
            gizmo.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f);
            gizmo.GetComponent<Renderer>().material = explosionMaterial;
            gizmo.GetComponent<SphereCollider>().enabled = false;
            //Destroy after 5 seconds
            Destroy(gizmo, 5f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(rocketTransform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
                hitCollider.gameObject.layer == LayerMask.NameToLayer("Rocket"))
            {
                StartCoroutine(TryPullObjectTowardsCenter(hitCollider));
            }
        }
    }

    private IEnumerator TryPullObjectTowardsCenter(Collider hitCollider)
    {
        float duration = 1.5f;
        float t = 0f;
        Vector3 startPosition = hitCollider.transform.position;
        Vector3 targetPosition = rocketTransform.position;
        GameObject objectToMove = hitCollider.gameObject;

        // Handle enemy movement stop
        if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ACEnemyMovementBehaviour enemyMovement = hitCollider.GetComponent<ACEnemyMovementBehaviour>();
            if (enemyMovement != null)
            {
                enemyMovement.StopMovement();
            }
            objectToMove = hitCollider.transform.parent.gameObject;
        }
        else if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Rocket"))
        {
            Rocket rocket = hitCollider.GetComponent<Rocket>();
            if (rocket.state == RocketState.ATTACHED || rocket.state == RocketState.REGROWING)
                yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / duration);

            objectToMove.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                moveToCenterCurve.Evaluate(normalizedTime)
            );
            yield return null;
        }

        // Final snap to target to ensure precision
        objectToMove.transform.position = targetPosition;

        // Resume enemy movement
        if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            ACEnemyMovementBehaviour enemyMovement = hitCollider.GetComponent<ACEnemyMovementBehaviour>();
            if (enemyMovement != null)
            {
                enemyMovement.ResumeMovement();
            }
        }
    }
}