using System.Collections;
using UnityEngine;

public class StraightLinePropulsion : ACRocketPropulsion
{
    public override IEnumerator FlyToTargetPosition(Vector3 target)
    {
        Logger.Log("Straight line propulsion activated", LogLevel.INFO, LogType.ROCKETS);
        Vector3 startingPosition = rocketTransform.position;

        // Fly towards the target
        while (Vector3.Distance(rocketTransform.position, target) > 10f)
        {
            float totalDistance = Vector3.Distance(startingPosition, target);
            float distanceCovered = Vector3.Distance(startingPosition, rocketTransform.position);
            float t = distanceCovered / totalDistance;

            rocketTransform.position = Vector3.MoveTowards(
                rocketTransform.position,
                target,
                ParentRocket.settings.flySpeedCurve.Evaluate(t) * Time.deltaTime * ParentRocket.settings.flySpeed
            );

            rocketTransform.localScale = Vector3.Lerp(
                rocketTransform.localScale,
                rocketOriginalScale * ParentRocket.settings.flyScaleMultiplier,
                0.1f * Time.deltaTime * ParentRocket.settings.flySpeed
            );

            yield return null;
        }

        Logger.Log("Rocket reached target", LogLevel.INFO, LogType.ROCKETS);
        // Raycast to find ground at the target's XZ position
        RaycastHit hit;
        int groundLayerMask = LayerMask.GetMask("Ground");

        // Cast straight down from above the target position
        Vector3 rayOrigin = target + Vector3.up * 50f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 100f, groundLayerMask))
        {
            // Keep target X and Z, use ground Y
            Vector3 groundPoint = new Vector3(target.x, hit.point.y, target.z);

            float descentVelocity = 0f;
            float gravity = 9.81f;
            float maxFallSpeed = 30f;

            Quaternion noseDiveRotation = Quaternion.Euler(90f, rocketTransform.eulerAngles.y, 0f); // Nose straight down

            while (rocketTransform.position.y > groundPoint.y + 0.1f)
            {
                float totalDistance = Vector3.Distance(startingPosition, target);
                float distanceCovered = Vector3.Distance(startingPosition, rocketTransform.position);
                float t = distanceCovered / totalDistance;

                // Accelerate downward (simulate gravity)
                descentVelocity += gravity * Time.deltaTime;
                descentVelocity = Mathf.Clamp(descentVelocity, 0f, maxFallSpeed);

                rocketTransform.position = Vector3.MoveTowards(
                rocketTransform.position,
                groundPoint,
                ParentRocket.settings.flySpeedCurve.Evaluate(t) * Time.deltaTime * ParentRocket.settings.flySpeed
                );

                // Smoothly rotate to nose-dive position
                rocketTransform.rotation = Quaternion.Slerp(
                    rocketTransform.rotation,
                    noseDiveRotation,
                    Time.deltaTime * 2f
                );

                yield return null;
            }

            // Snap to exact landing position
            rocketTransform.position = groundPoint;
        }
        else
        {
            Logger.Log("No ground detected below target position", LogLevel.WARNING, LogType.ROCKETS);
        }


        ParentRocket.SetState(RocketState.IDLE);
    }
}
