using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BouncingFront : ACRocketFront
{
    [Header("Bouncing Front")]
    public float flightDistanceAfterBounce;

    protected override void OnActivateAbility(Collider collider)
    {
        Vector3 rayDirection = (collider.transform.position - rocketTransform.position).normalized;
        Vector3 hitNormal = Vector3.zero;
        Vector3 hitPoint = Vector3.zero;

        bool hitSomething = false;
        float raycastDistance = (collider.transform.position - rocketTransform.position).magnitude;

        // Visualize the raycast toward the collider
        Debug.DrawRay(rocketTransform.position, rayDirection * raycastDistance, Color.cyan, 20f); // forward ray

        if (Physics.Raycast(rocketTransform.position, rayDirection, out RaycastHit hit, raycastDistance))
        {
            hitNormal = hit.normal;
            hitPoint = hit.point;
            hitSomething = true;
        }

        if (hitSomething)
        {
            Logger.Log("Bounce Angle: " + Vector3.Angle(hit.normal, Vector3.up), LogLevel.INFO, LogType.ROCKETS);
            // Check if the hit normal is nearly directly upward (within 10 degrees)
            if (Vector3.Angle(hit.normal, Vector3.up) <= 60f)
            {
                // Use parent's shootingDirection instead of a new reflection calculation
                Vector3 shootingDirection = parentRocket.shootingDirection.normalized;
                Vector3 newDirection = shootingDirection * flightDistanceAfterBounce;

                // Debug ray for parent's shooting direction
                Debug.DrawRay(rocketTransform.position, shootingDirection * 20f, Color.magenta, 20f);

                parentRocket.SetState(RocketState.IDLE);

                Vector3 newTarget = new Vector3(rocketTransform.position.x + newDirection.x,
                                                parentRocket.initialTransform.position.y,
                                                rocketTransform.position.z + newDirection.z);

                parentRocket.Shoot(newTarget);
            }
            else
            {
                // Flatten the hitNormal to the XZ plane for reflection calculation
                hitNormal.y = 0;
                hitNormal.Normalize();

                Vector3 reflectedDirection = Vector3.Reflect(rayDirection, hitNormal).normalized;
                Vector3 newDirection = reflectedDirection * flightDistanceAfterBounce;

                // Reflection debug rays
                Debug.DrawRay(hitPoint, hitNormal * 20f, Color.green, 20f);         // Surface normal
                Debug.DrawRay(hitPoint, reflectedDirection * 20f, Color.red, 20f);    // Reflected direction

                parentRocket.SetState(RocketState.IDLE);

                Vector3 newTarget = new Vector3(rocketTransform.position.x + newDirection.x,
                                                parentRocket.initialTransform.position.y,
                                                rocketTransform.position.z + newDirection.z);
                parentRocket.Shoot(newTarget);
            }
        }
        else
        {
            Debug.LogWarning("Bounce failed: No surface detected.");
        }
    }
}
