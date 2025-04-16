using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ImplosionBody : ACRocketBody
{
    public AnimationCurve moveToCenterCurve;
    public float maxPullSpeed;

    protected override void Explode()
    {
        // Create explosion effect
        GameObject explosionEffect = Instantiate(explosionPrefab, rocketTransform.position, Quaternion.identity);
        explosionEffect.GetComponentInChildren<LegExplosionHandler>().SetExplosionRadius(explosionRadius / 10);

        if (debugExplosionSphere)
        {
            GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gizmo.transform.position = rocketTransform.position;
            gizmo.transform.localScale = new Vector3(explosionRadius * 2f, explosionRadius * 2f, explosionRadius * 2f);
            gizmo.GetComponent<Renderer>().material = explosionMaterial;
            gizmo.GetComponent<SphereCollider>().enabled = false;
            Destroy(gizmo, 5f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(rocketTransform.position, explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            int layer = hitCollider.gameObject.layer;
            if (layer == LayerMask.NameToLayer("PL_IsEnemy"))
            {
                Debug.Log("Pulling enemy");
                hitCollider.gameObject.transform.parent.position = rocketTransform.position;
            }
            else if (layer == LayerMask.NameToLayer("PL_IsRocket"))
            {
                Rocket rocket = hitCollider.GetComponent<Rocket>();
                if (parentRocket.Equals(rocket))
                    return;

                PullRocketTowardsCenter(hitCollider);
            }
        }
    }

    private void PullRocketTowardsCenter(Collider hitCollider)
    {
        Rocket rocket = hitCollider.GetComponent<Rocket>();
        if (rocket == null)
            return;

        if (rocket.state == RocketState.ATTACHED || rocket.state == RocketState.REGROWING)
            return;

        GameObject rocketObject = hitCollider.gameObject;
        StartCoroutine(PullRocketCoroutine(rocketObject));
    }

    private IEnumerator PullRocketCoroutine(GameObject rocketObject)
    {
        float duration = 1.5f;
        float t = 0f;

        Vector3 startPosition = rocketObject.transform.position;
        Vector3 targetPosition = rocketTransform.position;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / duration);

            rocketObject.transform.position = Vector3.Lerp(
                startPosition,
                targetPosition,
                moveToCenterCurve.Evaluate(normalizedTime)
            );

            yield return null;
        }

        // Final snap to ensure accuracy
        rocketObject.transform.position = targetPosition;
    }
}
