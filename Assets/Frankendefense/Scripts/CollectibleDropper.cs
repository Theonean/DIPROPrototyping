using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleDropper : MonoBehaviour
{
    public GameObject CollectiblePrefab;
    public float dropChance = 0.2f;
    public void DropCollectible()
    {
        if (Random.Range(0f, 1f) <= dropChance)
        {
            Instantiate(CollectiblePrefab, transform.position, Quaternion.identity);
        }
    }
}
