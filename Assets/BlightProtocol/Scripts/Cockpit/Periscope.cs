using System.Collections;
using UnityEngine;

public class Periscope : ACButton
{
    public static Periscope Instance { get; private set; }
    [SerializeField] private RectTransform targetIcon;
    private Vector2 screenPoint;
    [SerializeField] private Camera periscopeCamera;
    [SerializeField] private GameObject mesh;
    [SerializeField] private Renderer screen;
    [SerializeField] private Collider mainCollider;
    private Animator meshAnimator;
    Ray ray;
    RaycastHit hit;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private LayerMask markerHitMask;
    private bool isActive = false;
    private bool isAnimating = false;

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

    void Start()
    {
        markerHitMask = Map.Instance.hitMask;
        meshAnimator = mesh.GetComponent<Animator>();
    }

    public override void OnHover()
    {
        this.DefaultOnHover();
        if (isActive)
        {
            ray = FPVPlayerCam.Instance.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100f, hitMask))
            {
                screenPoint = new Vector3(hit.textureCoord.x * periscopeCamera.pixelWidth, hit.textureCoord.y * periscopeCamera.pixelHeight, 0);
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
        }

    }

    public override void OnEndInteract()
    { }

    public void EndActive()
    {
        if (isAnimating) return;

        meshAnimator.SetTrigger("deactivate");
        isAnimating = true;
        isActive = false;
        mainCollider.enabled = true;

        // disable screen
        periscopeCamera.enabled = false;
        screen.material.SetColor("_BaseColor", Color.black);
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