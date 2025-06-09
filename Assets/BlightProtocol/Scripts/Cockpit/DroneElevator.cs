using System.Collections;
using UnityEngine;

public class DroneElevator : MonoBehaviour
{
    [SerializeField] private Animator platformAnimator;
    [SerializeField] private Transform dronePosition;

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
        StartCoroutine(SetDronePositionNextFrame());

    }

    public void OnExitFPV()
    {
        platformAnimator.SetTrigger("down");
    }

    private void OnPerspectiveSwitched()
    {
        if (PerspectiveSwitcher.Instance.currentPerspective == CameraPerspective.FPV)
        {
            OnEnterFPV();
        }
    }
    
    private IEnumerator SetDronePositionNextFrame()
    {
        var playerCore = PlayerCore.Instance;
        var rb = playerCore.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = false;

        yield return new WaitForFixedUpdate();
        
        playerCore.transform.parent = dronePosition;
        playerCore.transform.localPosition = Vector3.zero;
        playerCore.transform.localRotation = Quaternion.identity;
        playerCore.transform.localScale = Vector3.one;
        playerCore.shield.SetActive(false);
    }
}
