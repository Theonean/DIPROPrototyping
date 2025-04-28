using System.Collections;
using UnityEngine;

public class CrystalRock : MonoBehaviour
{
    public GameObject crystalPrefab; // Prefab of the crystal to spawn
    public int numberOfCrystals = 5; // Number of crystals to spawn
    public float spawnRadius = 5f; // Radius around the rock to spawn crystals
    public Vector2 throwForceRange = new Vector2(5f, 10f); // Range of force to apply to the crystals
    private bool isSpawning = false; // Flag to prevent multiple spawns

    public void SpawnCrystals()
    {
        if (isSpawning) return;
        isSpawning = true;

        StartCoroutine(SpawnCrystals(numberOfCrystals, spawnRadius));
    }

    private IEnumerator SpawnCrystals(int count, float radius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomPosition = transform.position + Random.insideUnitSphere * radius;
            randomPosition.y = transform.position.y; // Keep the y position the same as the rock

            GameObject crystal = Instantiate(crystalPrefab, randomPosition, Quaternion.identity);
            crystal.transform.SetParent(null); // Set the rock as the parent of the crystal

            Vector3 randomUp = Random.onUnitSphere;
            randomUp.y = Mathf.Abs(randomUp.y); // Ensure the up vector is not inverted

            Rigidbody rb = crystal.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(randomUp * Random.Range(throwForceRange.x, throwForceRange.y), ForceMode.Impulse); // Add an upward force to the crystal
            }
            yield return null;
        }

        Destroy(gameObject); // Destroy the rock after spawning the crystals
    }
}