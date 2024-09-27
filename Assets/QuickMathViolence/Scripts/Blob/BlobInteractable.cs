using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobInteractable : MonoBehaviour
{
    Rigidbody rb;
    SphereCollider myCollider;
    private bool isGrabbed;
    private bool isBeingThrown = false;
    private Transform holdPosition;
    private Transform throwPosition;
    private BlobMathHandler blobMathHandler;

    public float throwDelay;
    public float scaleDelay;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCollider = GetComponent<SphereCollider>();
        blobMathHandler = GetComponent<BlobMathHandler>();
    }

    private void Update()
    {
        if (isGrabbed)
        {
            transform.position = holdPosition.position;
        }
        else if (isBeingThrown)
        {
            transform.position = throwPosition.position;
        }
    }

    public void InitiateGrab(Transform _holdPosition)
    {
        rb.isKinematic = true;
        isGrabbed = true;
        holdPosition = _holdPosition;
        transform.parent = _holdPosition;

        myCollider.radius = myCollider.radius/2;

        blobMathHandler.isHeld = true;
    }

    public void EndGrab(Transform _throwPosition)
    {
        isGrabbed = false;
        isBeingThrown = true;
        transform.position = _throwPosition.position;
        throwPosition = _throwPosition;
        transform.parent = null;
        rb.isKinematic = false;

        Invoke(nameof(ScaleCollider), scaleDelay);
        Invoke(nameof(EndThrow), throwDelay);

        blobMathHandler.isHeld = false;
    }

    public void EndThrow()
    {
        isBeingThrown = false;
    }

    private void ScaleCollider()
    {
        myCollider.radius = myCollider.radius*2;
    }
}
