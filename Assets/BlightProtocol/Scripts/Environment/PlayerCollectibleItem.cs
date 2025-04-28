using UnityEngine;
using System.Collections;


[RequireComponent(typeof(Collider))]
public class ACPlayerCollectibleItem : MonoBehaviour
{
    public EItemTypes itemType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("PL_IsPlayer"))
        {
            // Start the fly effect towards the player
            StartCoroutine(FlyToPlayer());
        }
    }

    private IEnumerator FlyToPlayer()
    {
        Vector3 targetPosition = PlayerCore.Instance.transform.position;
        float duration = 1f; // Duration of the fly effect
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject); // Destroy the item after flying to the player
    }
}