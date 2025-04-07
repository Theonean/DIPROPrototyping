using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterResourcePointDetector : MonoBehaviour
{
    public List<GameObject> activeResourcePoints = new List<GameObject>();
    public bool isOnResourcePoint = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResourcePoint")) {
            GameObject resourcePointParent = other.attachedRigidbody.gameObject;
            OnResourcePointEnter(resourcePointParent);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("ResourcePoint")) {
            GameObject resourcePointParent = other.attachedRigidbody.gameObject;
            OnResourcePointExit(resourcePointParent);
        }
    }

    void OnResourcePointEnter(GameObject resourcePoint) {
        activeResourcePoints.Add(resourcePoint);
        isOnResourcePoint = true;
        
    }

    void OnResourcePointExit(GameObject resourcePoint) {
        activeResourcePoints.Remove(resourcePoint);
        if (activeResourcePoints.Count <= 0) {
            isOnResourcePoint = false;
        }
    }
}
