using System.Collections;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    public SOItem itemToSpawn; // Prefab of the item  to spawn
    public SOItem[] itemsToSpawn;
    public bool spawnSingle = true;

    public int numberOfItems = 1; // Number of items to spawn
    public float spawnRadius = 5f; // Radius around the object to spawn items
    public Vector2 throwForceRange = new Vector2(5f, 10f); // Range of force to apply to the item when spawning
    private bool isSpawning = false; // Flag to prevent multiple spawns
    [SerializeField] GameObject particleEffect;

    public void DropItems()
    {
        if (isSpawning) return;
        isSpawning = true;

        StartCoroutine(DropItems(numberOfItems, spawnRadius));
        if (particleEffect != null) {
            Instantiate(particleEffect, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator DropItems(int count, float radius)
    {
        itemToSpawn = spawnSingle ? itemToSpawn : itemsToSpawn[Random.Range(0, itemsToSpawn.Length)];
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * radius;
            randomPosition.y = transform.position.y; // Keep the y position the same as this object

            GameObject item = Instantiate(itemToSpawn.prefab, randomPosition, Quaternion.identity, null);
            Vector3 randomUp = Random.onUnitSphere;
            randomUp.y = Mathf.Abs(randomUp.y); // Ensure the up vector is not inverted

            Rigidbody rb = item.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(randomUp * Random.Range(throwForceRange.x, throwForceRange.y), ForceMode.Impulse); //Make items "pop out" off destroyed objects
            }

            CollectibleItem collectible = item.AddComponent<CollectibleItem>();
            collectible.itemData = itemToSpawn;


            yield return null;
        }

        Destroy(gameObject);
    }
}