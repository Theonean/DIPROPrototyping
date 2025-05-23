using System.Collections;
using UnityEngine;

public class Periscope : ACInteractable
{
    public static Periscope Instance { get; private set; }
    [Header("Screen")]
    [SerializeField] private Renderer screen;
    [SerializeField] private Camera periscopeCamera;
    [SerializeField] private RectTransform targetIcon;
    private Vector2 screenPoint;

    [Header("Animation")]
    [SerializeField] private Animator meshAnimator;

    [Header("Raycasting")]
    [SerializeField] private Collider mainCollider;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private LayerMask markerHitMask;
    Ray ray;
    RaycastHit hit;

    [Header("Interaction")]
    [SerializeField] private Button exitButton;
    [SerializeField] private TurnPeriscopeSlider turnSlider;

    private bool isActive = false;
    private bool isAnimating = false;

    void OnEnable()
    {
        exitButton.OnPressed.AddListener(EndActive);
    }

    void OnDisable()
    {
        exitButton.OnPressed.RemoveListener(EndActive);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        UpdateHover = true;
    }

    protected override void Start()
    {
        base.Start();
        markerHitMask = Map.Instance.hitMask;
        exitButton.IsCurrentlyInteractable = false;
        turnSlider.IsCurrentlyInteractable = false;
    }

    public override void OnStartHover()
    {
        if (!isActive) base.OnStartHover();
    }

    public override void OnUpdateHover(Vector2 MousePos)
    {
        if (isActive)
        {
            ray = FPVInputManager.Instance.fpvPlayerCam.ScreenPointToRay(MousePos);

            if (Physics.Raycast(ray, out hit, 100f, hitMask))
            {
                screenPoint = new Vector3(hit.textureCoord.x * periscopeCamera.pixelWidth, hit.textureCoord.y * periscopeCamera.pixelWidth, 0);
                if (screenPoint != null)
                {
                    targetIcon.anchoredPosition = screenPoint;
                    TouchPoint.transform.position = hit.point;
                }
            }
        }
    }

    public override void OnStartInteract()
    {
        if (isAnimating) return;
        if (isActive)
        {
            Ray hitRay = periscopeCamera.ScreenPointToRay(targetIcon.anchoredPosition);
            if (Physics.Raycast(hitRay, out RaycastHit _hit, Mathf.Infinity, markerHitMask))
            {
                Map.Instance.SetCustomMarker(_hit.point);
            }
        }
        else
        {
            base.OnStartInteract();
            meshAnimator.SetTrigger("activate");
            isAnimating = true;
            isActive = true;
            mainCollider.enabled = false;

            // enable screen
            periscopeCamera.enabled = true;
            screen.material.SetColor("_BaseColor", Color.white);
            exitButton.IsCurrentlyInteractable = true;
            turnSlider.IsCurrentlyInteractable = true;
        }

    }

    public override void OnEndInteract()
    { }

    public void EndActive(Button b)
    {
        if (isAnimating) return;

        meshAnimator.SetTrigger("deactivate");
        isAnimating = true;
        isActive = false;
        mainCollider.enabled = true;

        // disable screen
        periscopeCamera.enabled = false;
        screen.material.SetColor("_BaseColor", Color.black);

        exitButton.IsCurrentlyInteractable = false;
        turnSlider.IsCurrentlyInteractable = false;

        //reset turn
        turnSlider.SetPositionNormalized(0.5f);
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = meshAnimator.GetCurrentAnimatorStateInfo(0);

        // Check if animation has completed
        if (isAnimating && stateInfo.normalizedTime >= 1f)
        {
            isAnimating = false;
        }
    }
}