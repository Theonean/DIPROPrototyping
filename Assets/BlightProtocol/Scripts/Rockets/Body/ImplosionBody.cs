using System.Collections;
using UnityEngine;

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
        float t = 0;
        Vector3 startPosition = hitCollider.transform.position;
        Vector3 targetPosition = rocketTransform.position;

        float minMoveBeforeAbort = 0.2f;

        while (t < 1f)
        {
            t += Time.deltaTime / ParentRocket.settings.explosionChainDelay;
            hitCollider.transform.position = Vector3.Lerp(startPosition, targetPosition, moveToCenterCurve.Evaluate(t));

            if (Vector3.Distance(hitCollider.transform.position, targetPosition) < minMoveBeforeAbort)
            {
                // If the object is close enough to the target or is not moving anymore stop moving it
                break;
            }

            yield return null;
        }
    }
}