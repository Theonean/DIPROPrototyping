using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPVCamRotator : MonoBehaviour
{
    [SerializeField] private List<Transform> positions;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private float transitionTime = 1f;
    private bool isRotating = false;

    private int currentPosition = 1;

    void Start()
    {
        SetPosition(1);
    }

    private void SetPosition(int position) {
        if (position > 0 && position < positions.Count) {
            Vector3 lookDir = positions[currentPosition].localPosition - transform.localPosition;
            float targetY = Quaternion.LookRotation(lookDir).eulerAngles.y;
            transform.localRotation = Quaternion.Euler(0, targetY, 0);
        }
    }

    public void ChangePosition(int direction)
    {
        if (isRotating) return;
        if (TutorialManager.Instance.progressState is > TutorialProgress.PERSPECTIVESWITCHTOFPV and < TutorialProgress.SWITCHDIRECTION_A_D) return;

        isRotating = true;
        FPVInputManager.Instance.isActive = false;
        currentPosition += direction;
        if (currentPosition >= positions.Count)
        {
            currentPosition = 0;
        }
        if (currentPosition < 0) currentPosition = positions.Count - 1;
        StartCoroutine(SmoothRotate(positions[currentPosition].localPosition, direction));

        if(TutorialManager.Instance.progressState == TutorialProgress.SWITCHDIRECTION_A_D)
        {
            TutorialManager.Instance.CompleteSWITCHDIRECTION_A_D();
        }
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
        isRotating = false;
        FPVInputManager.Instance.isActive = true;
    }
}