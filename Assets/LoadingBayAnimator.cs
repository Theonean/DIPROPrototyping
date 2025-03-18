using System.Collections;
using UnityEngine;

public class LoadingBayAnimator : MonoBehaviour
{
    private float rampRotationClosed = 65f;
    private float rampRotationOpen = -10f;
    public float duration = 1f;
    public AnimationCurve animationCurve;
    public bool isPlayerNear = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            OpenRamp();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            CloseRamp();
        }
    }

    public void OpenRamp()
    {
        StartCoroutine(OpenRampCoroutine());
    }

    public void CloseRamp()
    {
        StartCoroutine(CloseRampCoroutine());
    }
    
    private IEnumerator OpenRampCoroutine()
    {
        float time = 0f;
        while (time < duration)
        {
            float angle = Mathf.Lerp(rampRotationClosed, rampRotationOpen, animationCurve.Evaluate(time / duration));
            transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = new Vector3(rampRotationOpen, 0f, 0f);
    }

    private IEnumerator CloseRampCoroutine()
    {
        float time = 0f;
        while (time < duration)
        {
            float angle = Mathf.Lerp(rampRotationOpen, rampRotationClosed, animationCurve.Evaluate(time / duration));
            transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = new Vector3(rampRotationClosed, 0f, 0f);
    }
}
