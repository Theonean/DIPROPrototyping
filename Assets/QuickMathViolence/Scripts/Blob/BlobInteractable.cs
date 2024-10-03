using UnityEngine;
using DG.Tweening;
public class BlobInteractable : MonoBehaviour
{
    public enum BlobInteractableState
    {
        None,
        Grabbed,
        Tweening,
        Thrown
    }

    public BlobInteractableState state = BlobInteractableState.None;

    private Transform holdPosition;

    private BlobFamilyHandler blobFamilyHandler;

    [Header("Physics")]
    public float groundedCOMY = 0;
    public float airborneCOMY = 0;
    public float currentCOMY;

    public float groundedColliderSize = 0.4f;
    public float thrownColliderSize = 0.8f;
    
    public float colliderScaleDelay;

    public float grabTweenTime = 0.5f;


    Rigidbody rb;
    SphereCollider myCollider;


    private BlobAudioHandler audioHandler;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioHandler = GetComponentInChildren<BlobAudioHandler>();
        //myCollider = GetComponent<SphereCollider>();
        blobFamilyHandler = GetComponent<BlobFamilyHandler>();
        rb.centerOfMass = new(0, groundedCOMY, 0);
        currentCOMY = groundedCOMY;
    }

    private void Update()
    {
        if (state == BlobInteractableState.Grabbed)
        {
            transform.localPosition = Vector3.zero;
        }
        else if (state == BlobInteractableState.None)
        {
            if (currentCOMY > groundedCOMY)
            {
                currentCOMY -= 0.1f;
                Debug.Log("tweening COM");
            }
                
            
            rb.centerOfMass = new(0, currentCOMY, 0);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (state == BlobInteractableState.Thrown)
        {
            if (collision.gameObject.layer == 10 || collision.gameObject.CompareTag("Blob"))
            {
                state = BlobInteractableState.None;
                foreach (var child in GetComponent<BlobFamilyHandler>().childBlobs)
                {
                    child.GetComponent<IndividualBlobHandler>().ScaleCollider(groundedColliderSize);
                }
            }
        }
    }

    public void InitiateGrab(Transform _holdPosition)
    {
        rb.isKinematic = true;
        holdPosition = _holdPosition;
        transform.DOLocalMove(Vector3.zero, grabTweenTime).SetEase(Ease.InQuart);
        state = BlobInteractableState.Tweening;
        Invoke(nameof(EndTween), grabTweenTime);

        transform.parent = _holdPosition;

        audioHandler.PlayAudioAction("Grab");
    }

    public void EndTween()
    {
        state = BlobInteractableState.Grabbed;
    }

    public void EndGrab(Transform _throwPosition, bool bigThrow)
    {
        transform.parent = null;

        // set COM
        state = BlobInteractableState.Thrown;
        rb.centerOfMass = new(0, airborneCOMY, 0); ;

        // scale collider
        foreach (var child in GetComponent<BlobFamilyHandler>().childBlobs)
        {
            child.GetComponent<IndividualBlobHandler>().ScaleCollider(thrownColliderSize);
        }

        // Optionally set position once at the moment of throw if needed:
        transform.position = _throwPosition.position;

        Invoke(nameof(ScaleCollider), colliderScaleDelay);

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
