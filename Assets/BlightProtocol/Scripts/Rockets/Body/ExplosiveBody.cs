using System.Collections;
using UnityEngine;

public class ExplosiveBody : ACRocketBody
{

    protected override void Explode()
    {
        //Create explosion effect
        GameObject explosionEffect = Instantiate(explosionPrefab, rocketTransform.position, Quaternion.identity);
        explosionEffect.GetComponentInChildren<LegExplosionHandler>().SetExplosionRadius(ParentRocket.settings.explosionRadius / 10);

        if (debugExplosionSphere)
        {
            //Create gizmo with explosionradius, for debugging
            GameObject gizmo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gizmo.transform.position = rocketTransform.position;
            gizmo.transform.localScale = new Vector3(ParentRocket.settings.explosionRadius * 2f, ParentRocket.settings.explosionRadius * 2f, ParentRocket.settings.explosionRadius * 2f);
            gizmo.GetComponent<Renderer>().material = explosionMaterial;
            gizmo.GetComponent<SphereCollider>().enabled = false;
            //Destroy after 5 seconds
            Destroy(gizmo, 5f);
        }

        Collider[] hitColliders = Physics.OverlapSphere(rocketTransform.position, ParentRocket.settings.explosionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("Enemy"))
            {
                hitCollider.gameObject.GetComponent<EnemyDamageHandler>().DestroyEnemy();
            }
            else if (hitCollider.gameObject.CompareTag("Rocket"))
            {
                Rocket rocket = hitCollider.gameObject.GetComponent<Rocket>();
                if (rocket.CanExplode())
                {
                    StartCoroutine(DaisyChainExplosion(rocket));
                }
            }
        }
    }

    private IEnumerator DaisyChainExplosion(Rocket rocket)
    {
        yield return new WaitForSeconds(ParentRocket.settings.explosionChainDelay);
        rocket.Explode();
    }
}