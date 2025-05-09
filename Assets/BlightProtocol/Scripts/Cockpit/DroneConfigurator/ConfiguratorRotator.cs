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
        Vector3 startRotation = transform.rotation.eulerAngles;
        while (elapsedTime < rotationTime)
        {
            float newRotation = animationCurve.Evaluate(elapsedTime / rotationTime) * 180f + startRotation.x;

            transform.rotation = Quaternion.Euler(newRotation, startRotation.y, startRotation.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

}
