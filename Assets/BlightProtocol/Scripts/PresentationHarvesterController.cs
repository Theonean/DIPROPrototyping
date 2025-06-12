using UnityEngine;
using System.Collections;

public class PresentationHarvesterController : MonoBehaviour
{
    private Transform[] targetPositions = new Transform[0];
    [SerializeField] private CanvasGroup[] textGroups = new CanvasGroup[1];
    private int index = 0;
    [SerializeField] private float moveSpeed = 10f;

    void Start()
    {
        targetPositions = GetComponentsInChildren<Transform>();
        Harvester.Instance.mover.SetMoveSpeed(0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine(FadeAlpha(textGroups[index], 0));
            index++;
            Harvester.Instance.mover.SetDestination(targetPositions[index].position);
            Harvester.Instance.mover.SetMoveSpeed(moveSpeed);
        }

        if (Harvester.Instance.HasArrivedAtTarget() && textGroups[index].alpha == 0)
        {
            StartCoroutine(FadeAlpha(textGroups[index], 1));
        }
    }

    private IEnumerator FadeAlpha(CanvasGroup group, float target)
    {
        float elapsedTime = 0;
        float startAlpha = group.alpha;
        while (elapsedTime < 1f)
        {
            group.alpha = Mathf.Lerp(startAlpha, target, elapsedTime / 1f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        group.alpha = target;
    }
}
