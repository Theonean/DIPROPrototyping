using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobInteractable : MonoBehaviour
{
    Rigidbody rb;
    SphereCollider myCollider;
    private bool isGrabbed;
    private Transform holdPosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        if (isGrabbed)
        {
            transform.position = holdPosition.position;
        }
    }

    public void InitiateGrab(Transform _holdPosition)
    {
        rb.isKinematic = true;
        isGrabbed = true;
        holdPosition = _holdPosition;
        transform.parent = _holdPosition;

        myCollider.radius = myCollider.radius/2;
    }

    public void EndGrab(Transform throwPosition)
    {
        rb.isKinematic = false;
        isGrabbed = false;
        transform.parent = null;
        transform.position = throwPosition.position;

        Invoke(nameof(ScaleCollider), 0.1f);
    }

    private void ScaleCollider()
    {
        myCollider.radius = myCollider.radius*2;
    }
}
