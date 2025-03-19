using System.Collections;
using UnityEngine;

public class LoadingBayAnimator : MonoBehaviour
{
    [SerializeField] private GameObject ramp;
    [SerializeField] private EntityDetector playerNearRampDetector;
    [SerializeField] private EntityDetector playerInHarvesterDetector;
    private float rampRotationClosed = 65f;
    private float rampRotationOpen = -10f;
    public float duration = 1f;
    public AnimationCurve animationCurve;
    public bool isPlayerNear = false;

    private void Start()
    {
        playerNearRampDetector.OnAgentEnter.AddListener(OpenRamp);
        playerNearRampDetector.OnAgentExit.AddListener(CloseRamp);
        playerInHarvesterDetector.OnAgentEnter.AddListener(OnPlayerInHarvester);
    }

    private void OpenRamp()
    {
        StartCoroutine(OpenRampCoroutine());
    }

    private void CloseRamp()
    {
        StartCoroutine(CloseRampCoroutine());
    }

    private void OnPlayerInHarvester()
    {
        //Prevent perspective switch from being trigered by players position being teleported into the drone bay
        if (DroneMovement.Instance.moveDirection.magnitude > 0.1f)
            PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.SWITCHING);
    }

    private IEnumerator OpenRampCoroutine()
    {
        float time = 0f;
        while (time < duration)
        {
            float angle = Mathf.Lerp(rampRotationClosed, rampRotationOpen, animationCurve.Evaluate(time / duration));
            ramp.transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            time += Time.deltaTime;
            yield return null;
        }
        ramp.transform.localEulerAngles = new Vector3(rampRotationOpen, 0f, 0f);
    }

    private IEnumerator CloseRampCoroutine()
    {
        float time = 0f;
        while (time < duration)
        {
            float angle = Mathf.Lerp(rampRotationOpen, rampRotationClosed, animationCurve.Evaluate(time / duration));
            ramp.transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            time += Time.deltaTime;
            yield return null;
        }
        ramp.transform.localEulerAngles = new Vector3(rampRotationClosed, 0f, 0f);
    }
}
