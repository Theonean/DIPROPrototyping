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

        if (Physics.Raycast(ray, out hit, raycastRange))
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
                if (lastHoveredObject != null)
                {
                    FPVUI.Instance.ClearLookAtText();
                    lastHoveredObject = null;
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
            if (lastHoveredObject != null)
            {
                FPVUI.Instance.ClearLookAtText();
                lastHoveredObject = null;
            }
        }
    }

    void Interact()
    {
        switch (hit.collider.tag)
        {
            default:
                if (lastHoveredObject != null)
                {
                    lastHoveredObject.OnInteract();
                }
                break;
        }
    }
}
