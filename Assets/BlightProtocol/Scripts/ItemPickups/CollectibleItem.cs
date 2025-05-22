using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Linq;

public class CollectibleItem : MonoBehaviour
{
    public EItemTypes itemType;
    public SOItem itemData;
    public UnityEvent<CollectibleItem> arrivedAtPlayer = new UnityEvent<CollectibleItem>();

    private bool flyingToPlayer = false;


    private void Start()
    {
        LayerMask IsCollectible = LayerMask.NameToLayer("PL_IsCollectible");
        gameObject.layer = IsCollectible;

        foreach (GameObject gO in GetComponentsInChildren<Transform>().Select(transform => transform.gameObject)) 
            gO.layer = IsCollectible;
    }

    public void StartflyingToPlayer()
    {
        if (flyingToPlayer) return;
        flyingToPlayer = true;

        StartCoroutine(FlyToPlayer());
    }

    private IEnumerator FlyToPlayer()
    {
        PlayerCore player = PlayerCore.Instance;
        float duration = 1f; // Duration of the fly effect
        float elapsedTime = 0f;

        while (Vector3.Distance(player.transform.position, transform.position) > 2f)
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        arrivedAtPlayer.Invoke(this);
        Destroy(gameObject); // Destroy the item after flying to the player
    }
}