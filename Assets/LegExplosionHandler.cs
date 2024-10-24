using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class LegExplosionHandler : MonoBehaviour
{
    public void SetExplosionRadius(float radius)
    {
        transform.parent.localScale = new Vector3(radius, radius, radius);
    }
    public void DestroyObject()
    {
        Destroy(transform.parent.gameObject);
    }
}
