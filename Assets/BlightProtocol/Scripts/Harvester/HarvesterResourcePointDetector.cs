using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HarvesterResourcePointDetector : MonoBehaviour
{
    public ResourcePoint activeResourcePoint;
    public bool isOnResourcePoint = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ResourcePoint")
        && other.gameObject.transform.parent.TryGetComponent<ResourcePoint>(out ResourcePoint resourcePoint))
        {
            activeResourcePoint = resourcePoint;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ResourcePoint"))
        {
            activeResourcePoint = null;
        }
    }
}
