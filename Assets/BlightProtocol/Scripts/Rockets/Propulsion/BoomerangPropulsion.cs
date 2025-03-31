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

    public override IEnumerator FlyToTargetPosition(Vector3 target)
    {
        // Phase 1: Outbound arc from current position to target with a side offset.
        Vector3 startPos = rocketTransform.position;
        Vector3 endPos = target;
        Vector3 midPos = (startPos + endPos) * 0.5f;

        // Calculate a perpendicular offset direction.
        Vector3 travelDir = endPos - startPos;
        Vector3 offsetDir = Vector3.Cross(travelDir, Vector3.up);
        if (offsetDir == Vector3.zero)
        {
            offsetDir = Vector3.right;
        }
        offsetDir = offsetDir.normalized * boomerangSideOffset;

        // Control point for the Bézier curve.
        Vector3 controlPoint = midPos + offsetDir;

        // Estimate arc length and determine how much to increment t based on flyspeed.
        float outboundArcLength = EstimateBezierArcLength(startPos, controlPoint, endPos);
        float t = 0f;
        while (t < 1f)
        {
            // Increment t based on the flyspeed (units per second).
            t += (parentRocket.settings.flySpeed * Time.deltaTime) / outboundArcLength;
            t = Mathf.Clamp01(t);
            Vector3 pos = QuadraticBezier(t, startPos, controlPoint, endPos);
            rocketTransform.position = pos;

            // Compute an approximate derivative for smooth rotation.
            float deltaT = 0.001f;
            Vector3 posAhead = QuadraticBezier(Mathf.Clamp01(t + deltaT), startPos, controlPoint, endPos);
            Vector3 derivative = posAhead - pos;
            if (derivative != Vector3.zero)
            {
                rocketTransform.rotation = Quaternion.LookRotation(derivative.normalized);
            }
            yield return null;
        }

        // Phase 2: Inbound arc from target back to the player's current position.
        Vector3 returnStart = rocketTransform.position; // Start of the return arc.
        t = 0f;
        while (Vector3.Distance(rocketTransform.position, parentRocket.initialTransform.position) > 1f)
        {
            // Get the player's current position each frame.
            Vector3 currentTarget = parentRocket.initialTransform.position;

            // Recalculate the mid-point and control point based on the current target.
            Vector3 returnMid = (returnStart + currentTarget) * 0.5f;
            Vector3 returnControl = returnMid - offsetDir;

            // Estimate the arc length for the current curve.
            float inboundArcLength = EstimateBezierArcLength(returnStart, returnControl, currentTarget);

            // Increment t based on the fly speed and current arc length.
            t += (parentRocket.settings.flySpeed * Time.deltaTime) / inboundArcLength;
            t = Mathf.Clamp01(t);

            // Compute the new position along the bezier curve.
            Vector3 pos = QuadraticBezier(t, returnStart, returnControl, currentTarget);
            rocketTransform.position = pos;

            // Update the rotation based on the derivative of the curve.
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
