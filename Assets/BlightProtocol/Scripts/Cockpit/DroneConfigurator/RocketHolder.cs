using UnityEngine;
using System.Collections;

public class RocketHolder : MonoBehaviour
{
    public ConfiguratorDummyRocket dummyRocket;
    [SerializeField] private float descendDistance, descendTime;
    [SerializeField] private AnimationCurve descendCurve;

    public void SetSelected(bool selected)
    {
        StartCoroutine(LerpHeight(selected ? 0f : -descendDistance));
    }

    private IEnumerator LerpHeight(float targetHeight)
    {
        float elapsedTime = 0f;
        float startHeight = transform.localPosition.y;
        while (elapsedTime < descendTime)
        {
            float height = Mathf.Lerp(startHeight, targetHeight, descendCurve.Evaluate(elapsedTime / descendTime));
            transform.localPosition = new Vector3(transform.localPosition.x, height, transform.localPosition.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
