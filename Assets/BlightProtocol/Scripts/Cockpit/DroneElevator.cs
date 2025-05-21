using UnityEngine;

public class DroneElevator : MonoBehaviour
{
    [SerializeField] private Animator platformAnimator;

    void OnEnable()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
        DroneGoggles.Instance.perspectiveSwitchStarted.AddListener(OnExitFPV);
    }
    
    void OnDisable()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(OnPerspectiveSwitched);
        DroneGoggles.Instance.perspectiveSwitchStarted.RemoveListener(OnExitFPV);
    }

    private void OnEnterFPV()
    {
        platformAnimator.SetTrigger("up");
    }

    public void OnExitFPV()
    {
        platformAnimator.SetTrigger("down");
    }

    private void OnPerspectiveSwitched() {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV)
        {
            OnEnterFPV();
        }
    }
}
