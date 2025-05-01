using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BoomerangPropulsion : ACRocketPropulsion
{
    public float boomerangSideOffset; // How far the rocket will offset sideways for its arc

    // Returns a point on a quadratic Bézier curve for parameter t (0 <= t <= 1)
    private Vector3 QuadraticBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        return Mathf.Pow(1 - t, 2) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow(t, 2) * p2;
    }

    // Approximates the arc length of a quadratic Bézier curve by subdividing it
    private float EstimateBezierArcLength(Vector3 p0, Vector3 p1, Vector3 p2, int subdivisions = 20)
    {
        float length = 0f;
        Vector3 previousPoint = p0;
        for (int i = 1; i <= subdivisions; i++)
        {
            float t = i / (float)subdivisions;
            Vector3 currentPoint = QuadraticBezier(t, p0, p1, p2);
            length += Vector3.Distance(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
        return length;
    }

    public override IEnumerator FlyToTargetPosition()
    {
        // Phase 1: Outbound arc — dynamically track target position.
        Vector3 startPos = rocketTransform.position;

        // Calculate fixed offset direction once using initial outbound direction
        Vector3 initialTarget = TargetPosition;
        Vector3 initialTravelDir = initialTarget - startPos;
        Vector3 offsetDir = Vector3.Cross(initialTravelDir, Vector3.up);
        if (offsetDir == Vector3.zero)
        {
            offsetDir = Vector3.right;
        }
        offsetDir = offsetDir.normalized * boomerangSideOffset;

        float t = 0f;

        while (t < 1f)
        {
            Vector3 currentTarget = TargetPosition;

            Vector3 midPos = (startPos + currentTarget) * 0.5f;
            Vector3 controlPoint = midPos + offsetDir;

            float arcLength = EstimateBezierArcLength(startPos, controlPoint, currentTarget);
            t += (parentRocket.settings.flySpeed * Time.deltaTime) / arcLength;
            t = Mathf.Clamp01(t);

            Vector3 pos = QuadraticBezier(t, startPos, controlPoint, currentTarget);
            rocketTransform.position = pos;

            // Smooth rotation
            float deltaT = 0.001f;
            Vector3 posAhead = QuadraticBezier(Mathf.Clamp01(t + deltaT), startPos, controlPoint, currentTarget);
            Vector3 derivative = posAhead - pos;
            if (derivative != Vector3.zero)
            {
                rocketTransform.rotation = Quaternion.LookRotation(derivative.normalized);
            }

            yield return null;
        }

        // Phase 2: Return arc
        Vector3 returnStart = rocketTransform.position;
        t = 0f;

        while (Vector3.Distance(rocketTransform.position, parentRocket.initialTransform.position) > 1f)
        {
            Vector3 currentTarget = parentRocket.initialTransform.position;
            Vector3 returnMid = (returnStart + currentTarget) * 0.5f;

            // Mirror the outbound offset direction
            Vector3 returnControl = returnMid - offsetDir;

            float inboundArcLength = EstimateBezierArcLength(returnStart, returnControl, currentTarget);
            t += (parentRocket.settings.flySpeed * Time.deltaTime) / inboundArcLength;
            t = Mathf.Clamp01(t);

            Vector3 pos = QuadraticBezier(t, returnStart, returnControl, currentTarget);
            rocketTransform.position = pos;

            float deltaT = 0.001f;
            Vector3 posAhead = QuadraticBezier(Mathf.Clamp01(t + deltaT), returnStart, returnControl, currentTarget);
            Vector3 derivative = posAhead - pos;
            if (derivative != Vector3.zero)
            {
                rocketTransform.rotation = Quaternion.LookRotation(derivative.normalized);
            }

            yield return null;
        }

        parentRocket.ReattachRocketToDrone();
    }
}
