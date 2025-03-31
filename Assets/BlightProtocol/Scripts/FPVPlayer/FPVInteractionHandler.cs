using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    [Header("Input")]
    KeyCode interactKey = KeyCode.Mouse0;
    private bool interactKeyPressed = false;

    [Header("Cameras")]
    private Camera fpvCamera;
    private FPVPlayerCam fpvPlayerCam;

    [Header("Raycasting")]
    private Ray ray;
    private RaycastHit hit;
    public LayerMask hitMask;
    public float raycastRange;

    private IFPVInteractable lastHoveredObject = null;

    void Start()
    {
        fpvCamera = GetComponent<Camera>();
        fpvPlayerCam = GetComponent<FPVPlayerCam>();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            interactKeyPressed = true;
        }
        if (Input.GetKeyUp(interactKey))
        {
            interactKeyPressed = false;
        }
    }

    void FixedUpdate()
    {
        if (fpvPlayerCam.isLooking)
        {
            return;
        }

        ray = fpvCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, raycastRange, hitMask))
        {
            Rigidbody hitRigidbody = hit.rigidbody;

            if (hitRigidbody != null)
            {
                if (hitRigidbody.TryGetComponent<IFPVInteractable>(out IFPVInteractable interactable) && hit.collider.CompareTag("FPVInteractable"))
                {
                    if (interactable != lastHoveredObject)
                    {
                        lastHoveredObject = interactable;
                        interactable.OnHover();
                    }
                    else if (interactable.UpdateHover)
                    {
                        interactable.OnHover();
                    }
                }
                else
                {
                    ClearLookAtText();
                }
            }
            else
            {
                if (hit.collider.TryGetComponent<IFPVInteractable>(out IFPVInteractable interactable) && hit.collider.CompareTag("FPVInteractable"))
                {
                    if (interactable != lastHoveredObject)
                    {
                        lastHoveredObject = interactable;
                        interactable.OnHover();
                    }
                    else if (interactable.UpdateHover)
                    {
                        interactable.OnHover();
                    }
                }
                else
                {
                    ClearLookAtText();
                }
            }

            if (interactKeyPressed)
            {
                Interact();
                interactKeyPressed = false;
            }
        }
        else
        {
            ClearLookAtText();
        }
    }

    private void ClearLookAtText()
    {
        if (lastHoveredObject != null)
        {
            FPVUI.Instance.ClearLookAtText();
            lastHoveredObject = null;
        }
    }

    void Interact()
    {
        if (hit.collider != null)
        {
            Rigidbody hitRigidbody = hit.rigidbody;

            if (hitRigidbody != null)
            {
                if (hitRigidbody.TryGetComponent<IFPVInteractable>(out IFPVInteractable interactable))
                {
                    interactable.OnInteract();
                }
            }
            else
            {
                if (hit.collider.TryGetComponent<IFPVInteractable>(out IFPVInteractable interactable))
                {
                    interactable.OnInteract();
                }
            }
        }
    }
}
