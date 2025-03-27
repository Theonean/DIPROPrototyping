using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;
    private bool interactKeyPressed = false;

    [Header("Cameras")]
    private Camera fpvCamera;

    [Header("Raycasting")]
    private Ray ray;
    private RaycastHit hit;
    public LayerMask hitMask;
    public float raycastRange;

    private IFPVInteractable lastHoveredObject = null;

    void Start()
    {
        fpvCamera = GetComponent<Camera>();
    }

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            interactKeyPressed = true;
        }
    }

    void FixedUpdate()
    {
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
