using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVCamRotator : MonoBehaviour
{
    [SerializeField] private List<Transform> positions;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float transitionTime = 1f;

    private int currentPosition = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) ChangePosition(-1);
        if (Input.GetKeyDown(KeyCode.D)) ChangePosition(1);
    }

    void ChangePosition(int direction)
    {
        currentPosition += direction;
        if (currentPosition >= positions.Count)
        {
            currentPosition = 0;
        }
        if (currentPosition < 0) currentPosition = positions.Count - 1;
        StartCoroutine(SmoothRotate(positions[currentPosition].localPosition, direction));
    }

    IEnumerator SmoothRotate(Vector3 target, float direction = 1)
    {
        Quaternion currentRotation = transform.localRotation;
        float currentY = currentRotation.eulerAngles.y;

        Vector3 lookDir = target - transform.localPosition;
        float targetY = Quaternion.LookRotation(lookDir).eulerAngles.y;
        float yDelta = targetY - currentY;
        
        if (direction < 0) {
            if (yDelta > 0) targetY -= 360;
        }
        else 
        {
            if (yDelta < 0) targetY += 360;
        }

        float elapsedTime = 0f;
        while (elapsedTime < transitionTime)
        {
            float newRotation = Mathf.Lerp(currentY, targetY, animationCurve.Evaluate(elapsedTime / transitionTime));
            transform.localRotation = Quaternion.Euler(0, newRotation, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}