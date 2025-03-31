using System.Collections;
using UnityEngine;

public class StraightLinePropulsion : ACRocketPropulsion
{
    public override IEnumerator FlyToTargetPosition(Vector3 target)
    {
        Logger.Log("Straight line propulsion activated", LogLevel.INFO, LogType.ROCKETS);
        Vector3 startingPosition = rocketTransform.position;


        while (Vector3.Distance(rocketTransform.position, target) > 0.1f)
        {
            float totalDistance = Vector3.Distance(startingPosition, target);
            float distanceCovered = Vector3.Distance(startingPosition, rocketTransform.position);
            float t = distanceCovered / totalDistance;

            // Update position to move towards target position using animation curve
            rocketTransform.position = Vector3.MoveTowards(rocketTransform.position, target, ParentRocket.settings.flySpeedCurve.Evaluate(t) * Time.deltaTime * ParentRocket.settings.flySpeed);

            // Lerp scale by how close leg is to final position
            rocketTransform.localScale = Vector3.Lerp(rocketTransform.localScale, rocketOriginalScale * ParentRocket.settings.flyScaleMultiplier, 0.1f * Time.deltaTime * ParentRocket.settings.flySpeed);

            yield return null;
        }

        Logger.Log("Rocket reached target", LogLevel.INFO, LogType.ROCKETS);
        ParentRocket.SetState(RocketState.IDLE);
    }
}