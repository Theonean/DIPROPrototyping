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
    public float dropForce;
    public float throwUpwardForce;
    public float dropUpwardForce;
    private float ejectForce;
    private float ejectUpwardForce;
    public KeyCode throwKey = KeyCode.Mouse0;
    public float throwRecoil;
    private bool willThrow = false;

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
        if (willThrow)
        {
            ThrowObject();
            willThrow = false;
        }
    }

    private void MyInput()
    {
        if (heldObject == null && !willThrow)
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
        else if (heldObject != null)
        {
            if (Input.GetKeyDown(throwKey))
            {
                willThrow = true;
                ejectForce = throwForce;
                ejectUpwardForce = throwUpwardForce;
            }
            else if (Input.GetKeyDown(splitKey))
            {
                willThrow = true;
                ejectForce = dropForce;
                ejectUpwardForce = dropUpwardForce;
            }
            
        }
    }

    private void GrabObject()
    {
        if (Physics.SphereCast(cameraObj.position,sphereCastRadius, cameraObj.forward, out objectHit, grabRange, grabbable))
        {
            heldObject = objectHit.transform.gameObject;
            if (heldObject.TryGetComponent<BlobInteractable>(out BlobInteractable blob)) {
                blob.InitiateGrab(holdPosition);
                blob.IgnorePlayerCollision(GetComponentInChildren<Collider>(), true);
            }
        }
    }

    private void ThrowObject()
    {
        if (heldObject.TryGetComponent<BlobInteractable>(out BlobInteractable blob))
        {
            blob.EndGrab(throwPosition);
            blob.GetComponent<Rigidbody>().isKinematic = false;
            Vector3 forceToAdd = cameraObj.forward * ejectForce + transform.up * ejectUpwardForce + rb.velocity;

            // recoil 
            rb.AddForce(cameraObj.forward*-1*throwRecoil, ForceMode.Impulse);

            if (blob.TryGetComponent<Rigidbody>(out Rigidbody blobRb))
            {
                blobRb.AddForce(forceToAdd, ForceMode.Impulse);
            }
            StartCoroutine(ReenableCollision(blob));
        }
        heldObject = null;
    }

    private void SplitObject()
    {
        if (Physics.SphereCast(cameraObj.position, sphereCastRadius, cameraObj.forward, out objectHit, grabRange, grabbable))
        {
            if (objectHit.collider.transform.parent.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler blob))
            {
                blob.Split();
            }
        }
    }

    IEnumerator ReenableCollision(BlobInteractable blob)
    {
        yield return new WaitForSeconds(0.5f);
        if (blob != null)
        {
            blob.IgnorePlayerCollision(GetComponentInChildren<Collider>(), false);
        }
    }
}
