using UnityEngine;
using System;

public class DistortionZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        InfluencedByDistortionZone IBDZ = other.GetComponentInParent<InfluencedByDistortionZone>();
        if(!IBDZ) return;

        throw new NotImplementedException();
    }
}
