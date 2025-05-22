using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LoadingBayAnimator : MonoBehaviour
{
    [SerializeField] private GameObject ramp;
    [SerializeField] private EntityDetector playerNearRampDetector;
    [SerializeField] private EntityDetector playerInHarvesterDetector;
    private int playerCollidersInLoadingBay = 0;
    private float rampRotationClosed = 65f;
    private float rampRotationOpen = -10f;
    public float duration = 1f;
    public AnimationCurve animationCurve;
    public bool isPlayerNear = false;

    private Coroutine openCoroutine = null;
    private Coroutine closedCoroutine = null;

    private void OnEnable()
    {
        playerNearRampDetector.OnAgentEnter.AddListener(AddPlayerInLoadingBay);
        playerNearRampDetector.OnAgentExit.AddListener(RemovePlayerInLoadingBay);
        playerInHarvesterDetector.OnAgentEnter.AddListener(OnPlayerInHarvester);
    }

    private void OnDisable()
    {
        playerNearRampDetector.OnAgentEnter.RemoveListener(AddPlayerInLoadingBay);
        playerNearRampDetector.OnAgentExit.RemoveListener(RemovePlayerInLoadingBay);
        playerInHarvesterDetector.OnAgentEnter.RemoveListener(OnPlayerInHarvester);
    }

    private void AddPlayerInLoadingBay()
    {
        ModifyPlayerInLoadingBayCount(1);
    }

    private void RemovePlayerInLoadingBay()
    {
        ModifyPlayerInLoadingBayCount(-1);
    }

    private void ModifyPlayerInLoadingBayCount(int change)
    {
        playerCollidersInLoadingBay += change;
        isPlayerNear = playerCollidersInLoadingBay > 0;

        if (isPlayerNear && openCoroutine == null)
        {
            openCoroutine = StartCoroutine(OpenRampCoroutine());
        }
        else if (!isPlayerNear && closedCoroutine == null)
        {
            closedCoroutine = StartCoroutine(CloseRampCoroutine());
        }
    }

    private void OnPlayerInHarvester()
    {
        // Prevent perspective switch from being triggered by player's position being teleported into the drone bay
        if (DroneMovement.Instance.moveDirection.magnitude > 0.1f)
            PerspectiveSwitcher.Instance.SetPerspective(CameraPerspective.FPV);
    }

    private IEnumerator OpenRampCoroutine()
    {
        if (closedCoroutine != null)
        {
            StopCoroutine(closedCoroutine);
            closedCoroutine = null;
        }

        float time = 0f;
        float startAngle = ramp.transform.localEulerAngles.x;
        if (startAngle > 180f) startAngle -= 360f; // Normalize angle to [-180, 180]

        while (time < duration)
        {
            float angle = Mathf.Lerp(startAngle, rampRotationOpen, animationCurve.Evaluate(time / duration));
            ramp.transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            time += Time.deltaTime;
            yield return null;
        }

        ramp.transform.localEulerAngles = new Vector3(rampRotationOpen, 0f, 0f);
        openCoroutine = null;
    }

    private IEnumerator CloseRampCoroutine()
    {
        if (openCoroutine != null)
        {
            StopCoroutine(openCoroutine);
            openCoroutine = null;
        }

        float time = 0f;
        float startAngle = ramp.transform.localEulerAngles.x;
        if (startAngle > 180f) startAngle -= 360f; // Normalize angle to [-180, 180]

        while (time < duration)
        {
            float angle = Mathf.Lerp(startAngle, rampRotationClosed, animationCurve.Evaluate(time / duration));
            ramp.transform.localEulerAngles = new Vector3(angle, 0f, 0f);
            time += Time.deltaTime;
            yield return null;
        }

        ramp.transform.localEulerAngles = new Vector3(rampRotationClosed, 0f, 0f);
        closedCoroutine = null;
    }
}