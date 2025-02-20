using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlyingDotMover : MonoBehaviour
{
    private Vector3 targetPosition;
    private GameObject dot;
    private Vector3 startPosition;
    private AnimationCurve curve;
    private float speed = 1000f; // Speed of the animation

    public void Initialize(Vector3 targetPos, GameObject dotObject, AnimationCurve curve)
    {
        targetPosition = targetPos;
        dot = dotObject;
        startPosition = dot.transform.position;
        this.curve = curve;
    }

    void Update()
    {
        // Move the dot towards the target position
        float t = Vector3.Distance(dot.transform.position, targetPosition) / Vector3.Distance(startPosition, targetPosition);
        dot.transform.position = Vector3.MoveTowards(dot.transform.position, targetPosition, speed * Time.deltaTime * curve.Evaluate(t));

        // Destroy the dot once it reaches the target
        if (Vector3.Distance(dot.transform.position, targetPosition) < 0.1f)
        {
            Destroy(dot);
            Destroy(this);
        }
    }
}