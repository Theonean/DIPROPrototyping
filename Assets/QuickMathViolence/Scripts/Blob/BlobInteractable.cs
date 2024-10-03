using UnityEngine;
using DG.Tweening;
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
    public float groundedColliderSize = 0.4f;
    public float thrownColliderSize = 0.8f;
    public float grabTweenTime = 0.5f;

    private BlobAudioHandler audioHandler;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioHandler = GetComponentInChildren<BlobAudioHandler>();
        //myCollider = GetComponent<SphereCollider>();
        blobFamilyHandler = GetComponent<BlobFamilyHandler>();
        rb.centerOfMass = groundedCOM;
    }

    private void Update()
    {
        if (isGrabbed)
        {
            if (!isTweening)
            {
                transform.localPosition = Vector3.zero;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isBeingThrown)
        {
            if (collision.gameObject.layer == 10 || collision.gameObject.CompareTag("Blob"))
            {
                isBeingThrown = false;
                rb.centerOfMass = groundedCOM;
                foreach (var child in GetComponent<BlobFamilyHandler>().childBlobs)
                {
                    child.GetComponent<IndividualBlobHandler>().ScaleCollider(groundedColliderSize);
                }
            }
        }
    }

    private bool isTweening = false;
    public void InitiateGrab(Transform _holdPosition)
    {
        rb.isKinematic = true;
        isGrabbed = true;
        holdPosition = _holdPosition;
        transform.DOLocalMove(Vector3.zero, grabTweenTime).SetEase(Ease.InQuart);
        isTweening = true;
        Invoke(nameof(EndTween), grabTweenTime);

        transform.parent = _holdPosition;

        blobFamilyHandler.isHeld = true;

        audioHandler.PlayAudioAction("Grab");
    }

    public void EndTween()
    {
        isTweening = false;
    }

    public void EndGrab(Transform _throwPosition, bool bigThrow)
    {
        isGrabbed = false;
        transform.parent = null;

        // set COM
        isBeingThrown = true;
        rb.centerOfMass = ariborneCOM;

        // scale collider
        foreach (var child in GetComponent<BlobFamilyHandler>().childBlobs)
        {
            child.GetComponent<IndividualBlobHandler>().ScaleCollider(thrownColliderSize);
        }

        // Optionally set position once at the moment of throw if needed:
        transform.position = _throwPosition.position;

        Invoke(nameof(ScaleCollider), scaleDelay);

        blobFamilyHandler.isHeld = false;

        if (bigThrow)
            audioHandler.PlayAudioAction("Throw Long");
        else
            audioHandler.PlayAudioAction("Throw Short");
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
