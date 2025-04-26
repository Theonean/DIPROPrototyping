using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVFixedPlayerCam : MonoBehaviour
{
    [SerializeField] private List<Transform> positions;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float transitionTime = 1f;

    private int currentPosition = 0;

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) ChangePosition(-1);
        if (Input.GetKeyDown(KeyCode.D)) ChangePosition(1);
    }

    void ChangePosition(int direction) {
        currentPosition += direction;
        if (currentPosition >= positions.Count)  {
            currentPosition = 0;
        }
        if (currentPosition < 0) currentPosition = positions.Count - 1;
        StartCoroutine(SmoothRotate(positions[currentPosition].localPosition));
    }

    IEnumerator SmoothRotate(Vector3 target) {
        Quaternion currentRotation = transform.localRotation;
        Vector3 lookDir = target - transform.localPosition;
        Quaternion targetRotation = Quaternion.LookRotation(lookDir);
        float elapsedTime = 0f;
        while (elapsedTime < transitionTime) {
            Quaternion newRotation = Quaternion.Slerp(currentRotation, targetRotation, animationCurve.Evaluate(elapsedTime / transitionTime));
            transform.localRotation = Quaternion.Euler(0, newRotation.eulerAngles.y, 0);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
