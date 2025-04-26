using UnityEngine;

public class FPVDroneGoggles : MonoBehaviour, IFPVInteractable
{
    public bool IsCurrentlyInteractable { get; set; } = true;
    public string lookAtText = "E";
    public string interactText = "";
    public bool UpdateHover { get; set; } = false;
    public bool UpdateInteract { get; set; } = false;
    [SerializeField] private Transform _touchPoint;
    [SerializeField] private GameObject mesh;
    private Animator meshAnimator;
    private bool isActive = false;
    private bool isAnimating = false;

    void Start()
    {
        meshAnimator = mesh.GetComponent<Animator>();
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
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

    public Transform TouchPoint
    {
        get => _touchPoint;
        set => _touchPoint = value;
    }
    public string LookAtText
    {
        get => lookAtText;
        set => lookAtText = value;
    }

    public string InteractText
    {
        get => interactText;
        set => interactText = value;
    }

    public void OnStartInteract()
    {
        if (isAnimating) return;
        else
        {
            meshAnimator.SetTrigger("activate");
            isAnimating = true;
            isActive = true;
            Invoke(nameof(EnterDroneMode), meshAnimator.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    void EnterDroneMode()
    {
        Debug.Log("enterDrone");
        PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
    }

    void OnPerspectiveSwitched()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV && isActive)
        {
            Debug.Log("exitDrone");
            meshAnimator.SetTrigger("deactivate");
            isAnimating = true;
            isActive = false;
        }
    }
    public void OnUpdateInteract() { }
    public void OnEndInteract() { }

    public void OnHover()
    {
        this.DefaultOnHover();
    }
}
