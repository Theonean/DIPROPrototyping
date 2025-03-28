using Unity.VisualScripting;
using UnityEngine;

public class BouncingFront : ACRocketFront
{
    public float flightDistanceAfterBounce;

    public override void ActivateAbility(Collider collider)
    {
        if (collider == parentRocket.GetComponent<Collider>()) return;

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
            // Flatten to XZ
            hitNormal.y = 0;
            hitNormal.Normalize();

            Vector3 reflectedDirection = Vector3.Reflect(rayDirection, hitNormal).normalized;
            Vector3 newDirection = reflectedDirection * flightDistanceAfterBounce;

            // Reflection debug rays
            Debug.DrawRay(hitPoint, hitNormal * 20f, Color.green, 20f);           // Surface normal
            Debug.DrawRay(hitPoint, reflectedDirection * 20f, Color.red, 20f);    // Reflected direction

            parentRocket.SetState(RocketState.IDLE);

            Vector3 newTarget = new Vector3(rocketTransform.position.x + newDirection.x, parentRocket.initialTransform.position.y, rocketTransform.position.z + newDirection.z);
            parentRocket.Shoot(newTarget);
        }
        else
        {
            Debug.LogWarning("Bounce failed: No surface detected.");
        }
    }
}
