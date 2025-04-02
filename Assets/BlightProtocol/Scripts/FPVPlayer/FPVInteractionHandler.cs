using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;
    private bool interactKeyPressed = false;

    [Header("Cameras")]
    private Camera fpvCamera;
    private FPVPlayerCam fpvPlayerCam;

    [Header("Raycasting")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float raycastRange = 3f;

    [Header("Physical Interaction")]
    [SerializeField] private GameObject touchTarget;

    private IFPVInteractable activeInteractable = null;  // The interactable being interacted with (when interact key is pressed)
    private IFPVInteractable hoveredInteractable = null;  // The interactable being hovered over (when interact key is not pressed)
    private IFPVInteractable previousHoveredInteractable = null;  // The previous hovered interactable (for hover change detection)

    void Start()
    {
        fpvCamera = GetComponent<Camera>();
        fpvPlayerCam = GetComponent<FPVPlayerCam>();
    }

    void Update()
    {
        interactKeyPressed = Input.GetKey(interactKey);

        // If interact key is pressed, handle active interaction
        if (activeInteractable != null && interactKeyPressed)
        {
            UpdateInteraction();
        }

        // If interact key is released, end the interaction
        if (Input.GetKeyUp(interactKey))
        {
            EndInteraction();
        }
    }

    void FixedUpdate()
    {
        // Skip raycasting if the camera is looking or an interaction is active
        if (fpvPlayerCam.isLooking || (activeInteractable != null && interactKeyPressed))
            return;

        Ray ray = fpvCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange, hitMask))
        {
            // Try to get IFPVInteractable from Rigidbody first
            IFPVInteractable interactable = null;
            Rigidbody hitRigidbody = hit.rigidbody;

            if (hitRigidbody != null)
            {
                hitRigidbody.TryGetComponent(out interactable);
            }
            else
            {
                // Fall back to Collider if no Rigidbody exists
                hit.collider.TryGetComponent(out interactable);
            }

            if (interactable != null && hit.collider.CompareTag("FPVInteractable"))
            {
                Hover(interactable);

                // If interact key is pressed and the hovered interactable is not the previous active one
                if (interactKeyPressed && interactable != activeInteractable)
                {
                    StartInteraction(interactable);
                }
            }
            else
            {
                EndHover();
            }
        }
        else
        {
            EndHover();
        }
    }

    private void Hover(IFPVInteractable interactable)
    {
        // If the hovered interactable is different from the current one
        if (interactable != hoveredInteractable)
        {
            previousHoveredInteractable = hoveredInteractable;  // Track the previous hovered interactable
            hoveredInteractable = interactable;  // Set new hovered interactable
            interactable.OnHover();  // Call the hover logic on the new interactable
        }
        else if (interactable.UpdateHover)
        {
            interactable.OnHover();  // Update hover for the same interactable if needed
        }
    }

    private void EndHover()
    {
        // If there was a hovered interactable, clear hover state
        if (hoveredInteractable != null)
        {
            FPVUI.Instance.ClearLookAtText();
            hoveredInteractable = null;
        }
    }

    private void StartInteraction(IFPVInteractable interactable)
    {
        activeInteractable = interactable;
        touchTarget.transform.position = interactable.TouchPoint.position;
        interactable.OnStartInteract();
    }

    private void UpdateInteraction()
    {
        // Update interaction if the active interactable has the necessary flag
        if (activeInteractable?.UpdateInteract == true)
        {
            touchTarget.transform.position = activeInteractable.TouchPoint.position;
            activeInteractable.OnUpdateInteract();
        }
    }

    private void EndInteraction()
    {
        if (activeInteractable != null)
        {
            touchTarget.transform.position = transform.position;
            activeInteractable.OnEndInteract();
            activeInteractable = null;
        }
    }
}
