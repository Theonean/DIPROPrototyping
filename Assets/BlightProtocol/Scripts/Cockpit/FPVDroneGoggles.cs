using UnityEngine;
using UnityEngine.Events;

public class DroneGoggles : ACInteractable
{
    public static DroneGoggles Instance;
    [SerializeField] private Animator meshAnimator;
    private bool isActivated = false;
    private bool isAnimating = false;
    public UnityEvent perspectiveSwitchStarted;
    protected void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    protected override void Start()
    {
        base.Start();
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
        meshAnimator.SetTrigger("activate");
        isAnimating = true;
        isActivated = true;
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
            isActivated = true;
            PerspectiveSwitcher.Instance.EnableTopDownPreview(true);

            //Block player inputs
            FPVInputManager.Instance.isActive = false;
            perspectiveSwitchStarted.Invoke();
        }
    }

    public void OnAnimationFinish()
    {
        if (isActivated)
        {
            PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
        }
        else
        {
            PerspectiveSwitcher.Instance.EnableTopDownPreview(false);
        }

    }

    void OnPerspectiveSwitched()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV && isActivated)
        {
            meshAnimator.SetTrigger("deactivate");
            isAnimating = true;
            isActivated = false;
        }
    }
}
