using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject prefab;
    private Queue<GameObject> availableObjects = new Queue<GameObject>();

    public GameObject Get()
    {
        if (availableObjects.Count == 0)
        {
            AddObjects(1);
        }

        return availableObjects.Dequeue();
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        availableObjects.Enqueue(obj);
    }

    private void AddObjects(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject newObj = Instantiate(prefab);
            newObj.SetActive(false);
            availableObjects.Enqueue(newObj);
        }
    }
}
