using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class ArcingPropulsion : ACRocketPropulsion
{
    public Vector2 minMaxArcHeight; // y value of the arc's peak, 
    public float maxDistance; // distance from the rocket to the target
    [Header("Explosion Settings")]
    public Material explosionMaterial; //Material for the explosion effect when the rocket explodes.

    public GameObject explosionPrefab; //The explosion prefab to spawn when the rocket explodes.

    public float landingExplosionRadius = 10f; // Radius of the explosion when the rocket lands
    public override IEnumerator FlyToTargetPosition(Vector3 target)
    {
        Logger.Log("Arcing propulsion activated", LogLevel.INFO, LogType.ROCKETS);

        Vector3 start = rocketTransform.position;
        Vector3 midPoint = (start + target) * 0.5f;
        target = new Vector3(target.x, parentRocket.initialTransform.position.y, target.z);

        float distance = Vector3.Distance(start, target);
        midPoint.y += Mathf.Lerp(minMaxArcHeight.x, minMaxArcHeight.y, Mathf.Clamp01(distance / maxDistance));

        float progress = 0f;

        while (progress < 1f)
        {
            progress += (ParentRocket.settings.flySpeed / Vector3.Distance(start, target)) * Time.deltaTime;
            float t = Mathf.Clamp01(progress);

            // Quadratic Bézier interpolation: (1 - t)^2 * A + 2(1 - t)t * B + t^2 * C
            Vector3 position =
                Mathf.Pow(1 - t, 2) * start +
                2 * (1 - t) * t * midPoint +
                Mathf.Pow(t, 2) * target;

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

        rocketTransform.position = target;

        // Trigger explosion
        GameObject explosionEffect = Instantiate(explosionPrefab, rocketTransform.position, Quaternion.identity);
        explosionEffect.GetComponentInChildren<LegExplosionHandler>().SetExplosionRadius(landingExplosionRadius / 10f);

        Logger.Log("Rocket reached target", LogLevel.INFO, LogType.ROCKETS);
        ParentRocket.SetState(RocketState.IDLE);
    }
}