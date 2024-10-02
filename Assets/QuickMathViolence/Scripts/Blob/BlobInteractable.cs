using UnityEngine;

public class BlobInteractable : MonoBehaviour
{
    Rigidbody rb;
    SphereCollider myCollider;
    private bool isGrabbed;
    private Transform holdPosition;
    private BlobFamilyHandler blobFamilyHandler;
    public float scaleDelay;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //myCollider = GetComponent<SphereCollider>();
        blobFamilyHandler = GetComponent<BlobFamilyHandler>();
    }

    private void Update()
    {
        if (isGrabbed)
        {
            transform.localPosition = Vector3.zero;
        }
    }

    public void InitiateGrab(Transform _holdPosition)
    {
        rb.isKinematic = true;
        isGrabbed = true;
        holdPosition = _holdPosition;
        transform.localPosition = Vector3.zero;
        transform.parent = _holdPosition;

        //myCollider.radius = myCollider.radius / 2;

        blobFamilyHandler.isHeld = true;
    }

    public void EndGrab(Transform _throwPosition)
    {
        isGrabbed = false;
        transform.parent = null;

        // Optionally set position once at the moment of throw if needed:
        transform.position = _throwPosition.position;

        Invoke(nameof(ScaleCollider), scaleDelay);

        blobFamilyHandler.isHeld = false;
    }

    private void ScaleCollider()
    {
        //myCollider.radius = myCollider.radius * 2;
    }
}
