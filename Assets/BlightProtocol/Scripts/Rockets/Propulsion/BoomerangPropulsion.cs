using System.Collections;
using UnityEngine;

public class BoomerangPropulsion : ACRocketPropulsion
{
    public float boomerangSideOffset; // How far the rocket will offset sideways for its arc
    public float controlPoint1Offset;
    public float controlPoint2Offset;

    // Returns a point on a cubic Bézier curve for parameter t (0 <= t <= 1)
    private Vector3 CubicBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float uu = u * u;
        float uuu = uu * u;
        float tt = t * t;
        float ttt = tt * t;

        Vector3 point = uuu * p0; // (1-t)³ * P0
        point += 3 * uu * t * p1; // 3(1-t)²t * P1
        point += 3 * u * tt * p2;  // 3(1-t)t² * P2
        point += ttt * p3;         // t³ * P3

        return point;
    }

    // Approximates the arc length of a cubic Bézier curve by subdividing it
    private float EstimateBezierArcLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int subdivisions = 20)
    {
        float length = 0f;
        Vector3 previousPoint = p0;
        for (int i = 1; i <= subdivisions; i++)
        {
            float t = i / (float)subdivisions;
            Vector3 currentPoint = CubicBezier(t, p0, p1, p2, p3);
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

            Vector3 controlPoint1 = startPos + (currentTarget - startPos) * controlPoint1Offset + offsetDir;
            Vector3 controlPoint2 = startPos + (currentTarget - startPos) * controlPoint2Offset + offsetDir;

            float arcLength = EstimateBezierArcLength(startPos, controlPoint1, controlPoint2, currentTarget);
            t += (flySpeedCurve.Evaluate(t) * flySpeed * Time.deltaTime) / arcLength;
            t = Mathf.Clamp01(t);

            Vector3 pos = CubicBezier(t, startPos, controlPoint1, controlPoint2, currentTarget);
            rocketTransform.position = pos;

            // Smooth rotation
            float deltaT = 0.001f;
            Vector3 posAhead = CubicBezier(Mathf.Clamp01(t + deltaT), startPos, controlPoint1, controlPoint2, currentTarget);
            Vector3 derivative = posAhead - pos;
            if (derivative != Vector3.zero)
            {
                rocketTransform.rotation = Quaternion.LookRotation(derivative.normalized);
            }

            yield return new WaitForFixedUpdate();
        }

        // Phase 2: Return arc
        Vector3 returnStart = rocketTransform.position;
        t = 0f;

        while (Vector3.Distance(rocketTransform.position, parentRocket.initialTransform.position) > 1f)
        {
            Vector3 currentTarget = parentRocket.initialTransform.position;

            // Mirror the outbound offset direction
            Vector3 returnControl1 = returnStart + (currentTarget - returnStart) * (1- controlPoint2Offset) - offsetDir;
            Vector3 returnControl2 = returnStart + (currentTarget - returnStart) * (1- controlPoint1Offset) - offsetDir;

            float inboundArcLength = EstimateBezierArcLength(returnStart, returnControl1, returnControl2, currentTarget);
            t += (flySpeed * Time.deltaTime) / inboundArcLength;
            t = Mathf.Clamp01(t);

            Vector3 pos = CubicBezier(t, returnStart, returnControl1, returnControl2, currentTarget);
            rocketTransform.position = pos;

            float deltaT = 0.001f;
            Vector3 posAhead = CubicBezier(Mathf.Clamp01(t + deltaT), returnStart, returnControl1, returnControl2, currentTarget);
            Vector3 derivative = posAhead - pos;
            if (derivative != Vector3.zero)
            {
                rocketTransform.rotation = Quaternion.LookRotation(derivative.normalized);
            }

            yield return new WaitForFixedUpdate();
        }

        parentRocket.ReattachRocketToDrone();
    }
}