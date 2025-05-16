using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ConfiguratorRotator : MonoBehaviour
{
    public float rotationTime = 1f;
    [SerializeField] private AnimationCurve animationCurve;

    public void StartRotation() {StartCoroutine(RotateBase());}

    private IEnumerator RotateBase()
    {
        float elapsedTime = 0f;
        Vector3 startRotation = transform.localRotation.eulerAngles;
        while (elapsedTime < rotationTime)
        {
            float newRotation = animationCurve.Evaluate(elapsedTime / rotationTime) * 220f + startRotation.x;

            transform.localRotation = Quaternion.Euler(newRotation, startRotation.y, startRotation.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
