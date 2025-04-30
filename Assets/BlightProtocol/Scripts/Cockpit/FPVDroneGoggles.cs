using UnityEngine;

public class FPVDroneGoggles : ACInteractable
{
    [SerializeField] private GameObject mesh;
    private Animator meshAnimator;
    private bool isActive = false;
    private bool isAnimating = false;

    protected override void Start()
    {
        base.Start();
        meshAnimator = mesh.GetComponent<Animator>();
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
        meshAnimator.SetTrigger("activate");
        isAnimating = true;
        isActive = true;
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

    public override void OnStartInteract()
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
        PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
    }

    void OnPerspectiveSwitched()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV && isActive)
        {
            meshAnimator.SetTrigger("deactivate");
            isAnimating = true;
            isActive = false;
        }
    }
}
