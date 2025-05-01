using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public enum CameraPerspective
{
    DRONE,
    FPV,
    SWITCHING
}

public class PerspectiveSwitcher : MonoBehaviour
{
    public static PerspectiveSwitcher Instance { get; private set; }
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private Camera perspectiveSwitchCamera;
    [SerializeField] private Camera droneCamera;
    [SerializeField] private Camera fpCamera;
    [SerializeField] private Camera mapCamera;
    [SerializeField] private Transform dronePositionInLoadingBay;
    [SerializeField] private Transform dronePositionInHarvester;
    public CameraPerspective currentPerspective { get; private set; } = CameraPerspective.DRONE;
    public UnityEvent onPerspectiveSwitched;

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
    }

    // Start is called before the first frame update
    void Start()
    {
        SetTopDownPerspective();
    }

    public void SetPerspective(CameraPerspective perspective)
    {
        if (currentPerspective == perspective)
        {
            return;
        }

        CameraPerspective fromPerspective = currentPerspective;
        currentPerspective = perspective;
        onPerspectiveSwitched.Invoke();

        switch (perspective)
        {
            case CameraPerspective.DRONE:
                SetTopDownPerspective();
                break;
            case CameraPerspective.FPV:
                SetFPVPerspective();
                break;
            case CameraPerspective.SWITCHING:
                StartCoroutine(AnimateCameraSwitch(fromPerspective));
                break;
        }
    }

    public void SetTopDownPerspective()
    {
        droneCamera.enabled = true;
        perspectiveSwitchCamera.enabled = false;
        fpCamera.gameObject.SetActive(false);
        mapCamera.gameObject.SetActive(false);

        PlayerCore playerCore = PlayerCore.Instance;

        playerCore.transform.parent = null;

        Vector3 spawnPosition = dronePositionInLoadingBay.position - Harvester.Instance.mover.GetMovementDirection() * 30f; //Position player behind harvester
        spawnPosition.y = DroneMovement.Instance.distanceFromGround;
        playerCore.transform.position = spawnPosition;

        Rigidbody playerRB = playerCore.GetComponent<Rigidbody>();
        playerRB.linearVelocity = Vector3.zero;
        playerRB.isKinematic = false;

        playerCore.transform.rotation = Quaternion.identity;
        playerCore.transform.localScale = new Vector3(1f, 1f, 1f);
        playerCore.shield.SetActive(true);

        //Make players colliders not make UI Rocket selector to be unclickable
        Collider[] playerColliders = playerCore.GetComponentsInChildren<Collider>();
        foreach (Collider pC in playerColliders)
        {
            pC.enabled = true;
        }

        Cursor.lockState = CursorLockMode.None;
    }

    public void SetFPVPerspective()
    {
        droneCamera.enabled = false;
        perspectiveSwitchCamera.enabled = false;
        fpCamera.gameObject.SetActive(true);
        mapCamera.gameObject.SetActive(true);

        PlayerCore playerCore = PlayerCore.Instance;

        playerCore.transform.parent = dronePositionInHarvester;
        playerCore.transform.localPosition = Vector3.zero;
        playerCore.transform.localRotation = Quaternion.identity;
        playerCore.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        playerCore.shield.SetActive(false);

        Rigidbody playerRB = playerCore.GetComponent<Rigidbody>();
        playerRB.linearVelocity = Vector3.zero;
        playerRB.isKinematic = false;

        RocketAimController rocketAimController = playerCore.GetComponentInChildren<RocketAimController>();
        rocketAimController.Rocket1.ReattachRocketToDrone();
        rocketAimController.Rocket2.ReattachRocketToDrone();
        rocketAimController.Rocket3.ReattachRocketToDrone();
        rocketAimController.Rocket4.ReattachRocketToDrone();


        Harvester harvester = Harvester.Instance;
        //playerCore.transform.LookAt(harvester.mover.targetPosObject.transform.position);

        //Make players colliders not make UI Rocket selector to be unclickable
        Collider[] playerColliders = playerCore.GetComponentsInChildren<Collider>();
        foreach (Collider pC in playerColliders)
        {
            pC.enabled = false;
        }
    }

    private IEnumerator AnimateCameraSwitch(CameraPerspective fromPerspective)
    {
        Camera startCam = fromPerspective == CameraPerspective.FPV ? fpCamera : droneCamera;
        float startFOV = startCam.fieldOfView;
        Camera endCam = fromPerspective == CameraPerspective.FPV ? droneCamera : fpCamera;
        float endFOV = endCam.fieldOfView;

        // Set starting position and lock rotation to the destination
        perspectiveSwitchCamera.transform.position = startCam.transform.position;
        perspectiveSwitchCamera.transform.rotation = endCam.transform.rotation;

        perspectiveSwitchCamera.enabled = true;
        startCam.enabled = false;
        endCam.enabled = false;



        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            perspectiveSwitchCamera.transform.position = Vector3.Lerp(
                startCam.transform.position,
                endCam.transform.position,
                animationCurve.Evaluate(t / animationDuration)
            );

            perspectiveSwitchCamera.fieldOfView = Mathf.Lerp(startFOV, endFOV, animationCurve.Evaluate(t / animationDuration));

            if (fromPerspective == CameraPerspective.DRONE)
            {
                perspectiveSwitchCamera.transform.rotation = Quaternion.Lerp(
                    startCam.transform.rotation,
                    endCam.transform.rotation,
                    animationCurve.Evaluate(t / animationDuration)
                );
            }

            yield return null;
        }

        endCam.enabled = true;
        perspectiveSwitchCamera.enabled = false;
        SetPerspective(fromPerspective == CameraPerspective.FPV ? CameraPerspective.DRONE : CameraPerspective.FPV);
    }
}
