using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    public static FPVInteractionHandler Instance { get; private set; }
    public ACInteractable HoveredInteractable { get; private set; }

    [Header("Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;
    [SerializeField] private LayerMask interactionMask;
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private GameObject touchTarget;

    private Camera fpvCamera;
    private FPVInputManager inputManager;
    private ACInteractable activeInteractable;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        fpvCamera = GetComponent<Camera>();
    }

    void Start()
    {
        inputManager = FPVInputManager.Instance;
    }

    private Vector3 cachedMousePosition;

    private void Update()
    {
        cachedMousePosition = Input.mousePosition;
    }

    private void LateUpdate()
    {
        if (activeInteractable != null && inputManager.lookState == LookState.INTERACTING)
        {
            UpdateInteraction();
            return;
        }

        HandleRaycast();
    }

    private void HandleRaycast()
    {
        Ray ray = fpvCamera.ScreenPointToRay(cachedMousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
        {
            ClearHover();
            return;
        }

        ACInteractable interactable = hit.collider.GetComponentInParent<ACInteractable>();
        if (interactable == null || !interactable.IsCurrentlyInteractable)
        {
            ClearHover();
            return;
        }

        HandleHover(interactable);
    }

    private void HandleHover(ACInteractable interactable)
    {
        if (interactable != HoveredInteractable)
        {
            ClearHover();
            HoveredInteractable = interactable;
            HoveredInteractable.OnStartHover();
        }
        else if (interactable.UpdateHover)
        {
            interactable.OnUpdateHover(cachedMousePosition);
        }
    }

    private void ClearHover()
    {
        if (HoveredInteractable == null) return;

        HoveredInteractable.OnEndHover();
        HoveredInteractable = null;
    }

    public void StartInteraction()
    {
        activeInteractable = HoveredInteractable;
        UpdateTouchTarget(activeInteractable.TouchPoint.position);
        activeInteractable.OnStartInteract();
    }

    private void UpdateInteraction()
    {
        if (activeInteractable.UpdateInteract)
        {
            UpdateTouchTarget(activeInteractable.TouchPoint.position);
            activeInteractable.OnUpdateInteract();
        }
    }

    private void UpdateTouchTarget(Vector3 position)
    {
        if (touchTarget != null)
        {
            touchTarget.transform.position = position;
        }
    }

    public void EndInteraction()
    {
        if (activeInteractable == null) return;

        ResetTouchTarget();
        activeInteractable.OnEndInteract();
        activeInteractable = null;
        inputManager.SetLookState(LookState.IDLE);
    }

    private void ResetTouchTarget()
    {
        if (touchTarget != null)
        {
            touchTarget.transform.position = transform.position;
        }
    }

    public void AbortInteraction()
    {
        ResetTouchTarget();
        activeInteractable = null;
    }
}