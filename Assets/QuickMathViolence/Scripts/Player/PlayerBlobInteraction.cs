using System.Collections;
using UnityEngine;

public class PlayerBlobInteraction : MonoBehaviour
{
    [Header("Grab")]
    public Transform holdPosition;
    public Transform throwPosition;
    public Transform cameraObj;
    public float grabRange;
    public float sphereCastRadius;
    public LayerMask grabbable;
    public KeyCode grabKey = KeyCode.Mouse0;

    private RaycastHit objectHit;

    [Header("Throw")]
    public float throwForce;
    public float throwUpwardForce;
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwRecoil;
    private bool willBeThrown = false;

    [Header("Split")]
    public KeyCode splitKey = KeyCode.Mouse1;

    private GameObject heldObject;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        MyInput();
    }

    private void FixedUpdate()
    {
        if (willBeThrown)
        {
            ThrowObject();
            willBeThrown = false;
        }
    }

    private void MyInput()
    {
        if (heldObject == null)
        {
            if (Input.GetKeyDown(grabKey))
            {
                GrabObject();
            }
            else if (Input.GetKeyUp(splitKey))
            {
                SplitObject();
            }
            
        }
        else if (heldObject != null && Input.GetKeyDown(throwKey))
        {
            willBeThrown = true;
        }
    }

    private void GrabObject()
    {
        if (Physics.SphereCast(cameraObj.position,sphereCastRadius, cameraObj.forward, out objectHit, grabRange, grabbable))
        {
            heldObject = objectHit.transform.gameObject;
            if (heldObject.TryGetComponent<BlobInteractable>(out BlobInteractable blob)) {
                blob.InitiateGrab(holdPosition);
                Physics.IgnoreCollision(heldObject.GetComponent<Collider>(), GetComponentInChildren<Collider>(), true);
            }
        }
    }

    private void ThrowObject()
    {
        if (heldObject.TryGetComponent<BlobInteractable>(out BlobInteractable blob))
        {
            blob.EndGrab(throwPosition);
            blob.GetComponent<Rigidbody>().isKinematic = false;
            Vector3 forceToAdd = cameraObj.forward * throwForce + transform.up * throwUpwardForce + rb.velocity;

            // recoil 
            rb.AddForce(cameraObj.forward*-1*throwRecoil, ForceMode.Impulse);

            if (blob.TryGetComponent<Rigidbody>(out Rigidbody blobRb))
            {
                blobRb.AddForce(forceToAdd, ForceMode.Impulse);
            }
            StartCoroutine(ReenableCollision(heldObject));
        }
        heldObject = null;
    }

    private void SplitObject()
    {
        if (Physics.SphereCast(cameraObj.position, sphereCastRadius, cameraObj.forward, out objectHit, grabRange, grabbable))
        {
            if (objectHit.collider.TryGetComponent<BlobMathHandler>(out BlobMathHandler blob))
            {
                blob.Split();
            }
        }
    }

    IEnumerator ReenableCollision(GameObject blob)
    {
        yield return new WaitForSeconds(2f);
        if (blob != null)
        {
            Physics.IgnoreCollision(blob.GetComponent<Collider>(), GetComponentInChildren<Collider>(), false);
        }
    }
}
