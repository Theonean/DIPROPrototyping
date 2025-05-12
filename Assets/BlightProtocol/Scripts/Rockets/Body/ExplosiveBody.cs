using System.Collections;
using UnityEngine;

public class ExplosiveBody : ACRocketBody
{

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
        int enemiesKilled = 0;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("PL_IsEnemy"))
            {
                Vector3 directionToEnemy = hitCollider.transform.position - rocketTransform.position;
                //Debug raycast to check if the rocket is in line of sight to the enemy
                Debug.DrawRay(rocketTransform.position, directionToEnemy.normalized * directionToEnemy.magnitude, Color.red, 50f);

                RaycastHit hit;
                if (Physics.Raycast(rocketTransform.position, directionToEnemy.normalized, out hit, directionToEnemy.magnitude))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        hitCollider.gameObject.GetComponentInParent<EnemyDamageHandler>().DestroyEnemy();
                        enemiesKilled++;
                    }
                }
            }
            else if (hitCollider.gameObject.CompareTag("Rocket"))
            {
                Rocket rocket = hitCollider.gameObject.GetComponentInParent<Rocket>();
                if (rocket.CanExplode())
                {
                    StartCoroutine(DaisyChainExplosion(rocket));
                }
            }
        }

        OnKilledEnemy.Invoke(RocketComponentType.BODY, enemiesKilled);
    }

    private IEnumerator DaisyChainExplosion(Rocket rocket)
    {
        yield return new WaitForSeconds(explosionChainDelay);
        rocket.Explode();
    }
}