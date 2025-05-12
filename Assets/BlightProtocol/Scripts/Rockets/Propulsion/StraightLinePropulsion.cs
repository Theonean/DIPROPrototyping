using System.Collections;
using UnityEngine;

public class StraightLinePropulsion : ACRocketPropulsion
{
    public override IEnumerator FlyToTargetPosition()
    {
        Logger.Log("Straight line propulsion activated", LogLevel.INFO, LogType.ROCKETS);
        Vector3 startingPosition = rocketTransform.position;

        // Fly towards the TargetPosition
        while (Vector3.Distance(rocketTransform.position, TargetPosition) > 10f)
        {
            float totalDistance = Vector3.Distance(startingPosition, TargetPosition);
            float distanceCovered = Vector3.Distance(startingPosition, rocketTransform.position);
            float t = distanceCovered / totalDistance;

            rocketTransform.position = Vector3.MoveTowards(
                rocketTransform.position,
                TargetPosition,
                ParentRocket.settings.flySpeedCurve.Evaluate(t) * Time.deltaTime * ParentRocket.settings.flySpeed
            );

            rocketTransform.localScale = Vector3.Lerp(
                rocketTransform.localScale,
                rocketOriginalScale * ParentRocket.settings.flyScaleMultiplier,
                0.1f * Time.deltaTime * ParentRocket.settings.flySpeed
            );

            yield return null;
        }

        Logger.Log("Rocket reached TargetPosition", LogLevel.INFO, LogType.ROCKETS);
        // Raycast to find ground at the TargetPosition's XZ position
        RaycastHit hit;
        int groundLayerMask = LayerMask.GetMask("PL_IsEnvironmentPhysicalObject");

        // Cast straight down from above the TargetPosition position
        Vector3 rayOrigin = TargetPosition + Vector3.up * 50f;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 100f, groundLayerMask))
        {
            // Keep TargetPosition X and Z, use ground Y
            Vector3 groundPoint = new Vector3(TargetPosition.x, hit.point.y, TargetPosition.z);

            float descentVelocity = 0f;
            float gravity = 9.81f;
            float maxFallSpeed = 30f;

            Quaternion noseDiveRotation = Quaternion.Euler(90f, rocketTransform.eulerAngles.y, 0f); // Nose straight down

            while (Vector3.Distance(rocketTransform.position, groundPoint) > 0.1f)
            {
                float totalDistance = Vector3.Distance(startingPosition, TargetPosition);
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
        }
        else
        {
            Logger.Log("No ground detected below TargetPosition position", LogLevel.WARNING, LogType.ROCKETS);
        }


        ParentRocket.SetState(RocketState.IDLE);
    }
}
