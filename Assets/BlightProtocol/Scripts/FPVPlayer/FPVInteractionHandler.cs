using UnityEngine;

public class FPVInteractionHandler : MonoBehaviour
{
    public static FPVInteractionHandler Instance { get; private set; }
    public ACInteractable HoveredInteractable { get; private set; }

    [Header("Input Settings")]
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float raycastRange = 3f;
    [SerializeField] private GameObject touchTarget;

    private Camera fpvCamera;
    private ACInteractable activeInteractable;
    private Ray currentRay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
        fpvCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        FPVInputManager.Instance.SetInteractionState(false);
    }

    private void Update()
    {
        HandleInput();
        UpdateRay();
    }

    private void FixedUpdate()
    {
        if (FPVInputManager.Instance.BlockInteraction) return;
        
        if (IsInteracting())
        {
            UpdateInteraction();
            return;
        }

        ProcessRaycast();
    }

    private void HandleInput()
    {
        if (Input.GetKeyUp(interactKey))
        {
            EndInteraction();
            FPVInputManager.Instance.SetInteractionState(false);
        }
    }

    private void UpdateRay()
    {
        currentRay = fpvCamera.ScreenPointToRay(Input.mousePosition);
    }

    private bool IsInteracting()
    {
        return FPVInputManager.Instance.IsInteracting && Input.GetKey(interactKey);
    }

    private void ProcessRaycast()
    {
        if (!Physics.Raycast(currentRay, out RaycastHit hit, raycastRange, hitMask))
        {
            ClearHover();
            return;
        }

        var interactable = hit.collider.GetComponentInParent<ACInteractable>();
        if (interactable == null)
        {
            ClearHover();
            return;
        }

        HandleHover(interactable);

        if (Input.GetKeyDown(interactKey) && interactable.IsCurrentlyInteractable)
        {
            StartInteraction(interactable);
        }
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
            interactable.OnUpdateHover();
        }
    }

    private void ClearHover()
    {
        if (HoveredInteractable == null) return;
        
        FPVUI.Instance?.ClearLookAtText();
        HoveredInteractable.OnEndHover();
        HoveredInteractable = null;
    }

    private void StartInteraction(ACInteractable interactable)
    {
        activeInteractable = interactable;
        UpdateTouchTargetPosition(interactable.TouchPoint.position);
        interactable.OnStartInteract();
        FPVInputManager.Instance.SetInteractionState(true);
    }

    private void UpdateInteraction()
    {
        if (activeInteractable?.UpdateInteract != true) return;
        
        UpdateTouchTargetPosition(activeInteractable.TouchPoint.position);
        activeInteractable.OnUpdateInteract();
    }

    private void UpdateTouchTargetPosition(Vector3 position)
    {
        if (touchTarget != null)
        {
            touchTarget.transform.position = position;
        }
    }

    private void EndInteraction()
    {
        if (activeInteractable == null) return;
        
        ResetTouchTarget();
        activeInteractable.OnEndInteract();
        activeInteractable = null;
    }

    public void AbortInteraction()
    {
        if (activeInteractable == null) return;
        
        ResetTouchTarget();
        activeInteractable = null;
    }

    private void ResetTouchTarget()
    {
        if (touchTarget != null)
        {
            touchTarget.transform.position = transform.position;
        }
    }
}