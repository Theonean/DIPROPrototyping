using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class CollectibleItem : MonoBehaviour
{
    public EItemTypes itemType;
    public SOItem itemData;
    public UnityEvent<CollectibleItem> arrivedAtPlayer = new UnityEvent<CollectibleItem>();

    private bool flyingToPlayer = false;

    public void StartflyingToPlayer()
    {
        if (flyingToPlayer) return;
        flyingToPlayer = true;

        StartCoroutine(FlyToPlayer());
    }

    private IEnumerator FlyToPlayer()
    {
        Vector3 targetPosition = PlayerCore.Instance.transform.position;
        float duration = 1f; // Duration of the fly effect
        float elapsedTime = 0f;

        while (Vector3.Distance(targetPosition, transform.position) > 1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        arrivedAtPlayer.Invoke(this);
        Destroy(gameObject); // Destroy the item after flying to the player
    }
}