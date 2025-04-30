using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    public static FPVInteractionHandler Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    [Header("Raycasting")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float raycastRange = 3f;
    [SerializeField] private GameObject touchTarget;

    private Camera fpvCamera;
    private ACInteractable activeInteractable;
    public ACInteractable hoveredInteractable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        fpvCamera = GetComponent<Camera>();
    }

    void Start()
    {
        FPVInputManager.Instance.SetInteractionState(false);
    }

    private void Update()
    {
        if (Input.GetKeyUp(interactKey))
        {
            EndInteraction();
            FPVInputManager.Instance.SetInteractionState(false);
        }
    }

    private void FixedUpdate()
    {
        if (FPVInputManager.Instance.BlockInteraction) return;
        if (FPVInputManager.Instance.IsInteracting && Input.GetKey(interactKey))
        {
            UpdateInteraction();
            return;
        }

        Ray ray = fpvCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastRange, hitMask))
        {
            var interactable = hit.collider.GetComponentInParent<ACInteractable>();
            if (interactable != null)
            {
                HandleHover(interactable);

                if (Input.GetKeyDown(interactKey) && interactable.IsCurrentlyInteractable)
                {
                    StartInteraction(interactable);
                }
            }
            else
            {
                ClearHover();
            }
        }
        else
        {
            ClearHover();
        }
    }

    private void HandleHover(ACInteractable interactable)
    {
        if (interactable != null && interactable != hoveredInteractable)
        {
            ClearHover();
            hoveredInteractable = interactable;
            hoveredInteractable.OnStartHover();
        }
        else if (interactable.UpdateHover)
        {
            interactable.OnUpdateHover();
        }
    }

    private void ClearHover()
    {
        if (hoveredInteractable != null)
        {
            FPVUI.Instance?.ClearLookAtText();
            hoveredInteractable.OnEndHover();
            hoveredInteractable = null;
        }
    }

    private void StartInteraction(ACInteractable interactable)
    {
        activeInteractable = interactable;
        touchTarget.transform.position = interactable.TouchPoint.position;
        interactable.OnStartInteract();
        FPVInputManager.Instance.SetInteractionState(true);
    }

    private void UpdateInteraction()
    {
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
    public void AbortInteraction()
    {
        if (activeInteractable != null)
        {
            touchTarget.transform.position = transform.position;
            activeInteractable = null;
        }
    }
}