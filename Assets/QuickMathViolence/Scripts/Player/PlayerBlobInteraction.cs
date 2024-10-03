using System.Collections;
using Unity.VisualScripting;
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
    private bool bigThrow = false;

    [Header("Split")]
    public KeyCode splitKey = KeyCode.Mouse1;

    private BlobInteractable heldObject;
    private Rigidbody rb;

    BlobAudioHandler blobAudioHandler;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        blobAudioHandler = GetComponentInChildren<BlobAudioHandler>();
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
                bigThrow = true;
            }
            else if (Input.GetKeyDown(splitKey))
            {
                willThrow = true;
                ejectForce = dropForce;
                ejectUpwardForce = dropUpwardForce;
                bigThrow = false;
            }
            
        }
    }

    private void GrabObject()
    {
        if (Physics.SphereCast(cameraObj.position,sphereCastRadius, cameraObj.forward, out objectHit, grabRange, grabbable))
        {
            if (objectHit.transform.gameObject.TryGetComponent<BlobInteractable>(out BlobInteractable blobInt))
                heldObject = blobInt;
            else
                heldObject = objectHit.transform.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable;
            if (heldObject.TryGetComponent<BlobInteractable>(out BlobInteractable blob)) {
                blob.InitiateGrab(holdPosition);
                blob.IgnorePlayerCollision(GetComponentInChildren<Collider>(), true);
            }
        }
    }

    private void ThrowObject()
    {
        heldObject.EndGrab(throwPosition, bigThrow);
        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        Vector3 forceToAdd = cameraObj.forward * ejectForce + transform.up * ejectUpwardForce + rb.velocity;

        // recoil 
        rb.AddForce(cameraObj.forward*-1*throwRecoil, ForceMode.Impulse);

        if (heldObject.TryGetComponent<Rigidbody>(out Rigidbody blobRb))
        {
            blobRb.AddForce(forceToAdd, ForceMode.Impulse);
        }
        StartCoroutine(ReenableCollision(heldObject));
        heldObject = null;
    }

    private void SplitObject()
    {
        if (Physics.SphereCast(cameraObj.position, sphereCastRadius, cameraObj.forward, out objectHit, grabRange, grabbable))
        {
            if (objectHit.transform.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler blobFamily))
            {
                blobFamily.Split();
            }
            else if (objectHit.transform.gameObject.GetComponent<IndividualBlobHandler>().parentInteractable.TryGetComponent<BlobFamilyHandler>(out BlobFamilyHandler blob))
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
