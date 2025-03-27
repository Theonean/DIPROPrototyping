using UnityEngine;

public class BouncingFront : ACRocketFront
{
    public float flightDistanceAfterBounce;
    public override void ActivateAbility(Collider collider)
    {
        Vector3 impactDirection = collider.transform.position - parentRocket.positionWhenShot;

        // Reflect the impact direction on the XZ plane
        Vector3 normal = collider.transform.up; // Assuming the collider's up vector is the surface normal
        Vector3 reflectedDirection = Vector3.Reflect(impactDirection, normal);

        Debug.Log("Bouncee");

        // Normalize the reflected direction and scale it by flightDistanceAfterBounce
        Vector3 newDirection = new Vector3(reflectedDirection.x, 0, reflectedDirection.z).normalized * flightDistanceAfterBounce;

        parentRocket.SetState(RocketState.IDLE);
        parentRocket.Shoot(rocketTransform.position + newDirection);
    }
}
