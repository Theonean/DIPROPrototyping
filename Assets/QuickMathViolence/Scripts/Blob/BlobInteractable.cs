using UnityEngine;

public class BlobInteractable : MonoBehaviour
{
    Rigidbody rb;
    SphereCollider myCollider;
    private bool isGrabbed;
    private Transform holdPosition;
    private BlobFamilyHandler blobFamilyHandler;
    public float scaleDelay;
    public bool isBeingThrown = false;

    [Header("Physics")]
    public Vector3 groundedCOM = Vector3.zero;
    public Vector3 ariborneCOM = Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        //myCollider = GetComponent<SphereCollider>();
        blobFamilyHandler = GetComponent<BlobFamilyHandler>();
        rb.centerOfMass = groundedCOM;
    }

    private void Update()
    {
        if (isGrabbed)
        {
            transform.localPosition = Vector3.zero;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 10 || collision.gameObject.CompareTag("Blob"))
        {
            if (isBeingThrown)
            {
                rb.centerOfMass = groundedCOM;
            }
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

        // set COM
        isBeingThrown = true;
        rb.centerOfMass = ariborneCOM;

        // Optionally set position once at the moment of throw if needed:
        transform.position = _throwPosition.position;

        Invoke(nameof(ScaleCollider), scaleDelay);

        blobFamilyHandler.isHeld = false;
    }

    private void ScaleCollider()
    {
        //myCollider.radius = myCollider.radius * 2;
    }

    public void IgnorePlayerCollision(Collider playerCollider, bool enable)
    {
        GameObject[] children = GetComponent<BlobFamilyHandler>().childBlobs.ToArray();
        foreach (GameObject child in children)
        {
            Physics.IgnoreCollision(child.GetComponent<Collider>(), playerCollider, enable);
        }
        
    }
}
