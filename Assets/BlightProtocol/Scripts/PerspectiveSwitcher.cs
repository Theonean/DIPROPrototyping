using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

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
        playerCore.GetComponent<NavMeshAgent>().enabled = true;

        playerCore.transform.position = dronePositionInLoadingBay.position - Harvester.Instance.mover.GetMovementDirection() * 30f; //Position player behind harvester
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
        Cursor.visible = true;
    }

    public void SetFPVPerspective()
    {
        droneCamera.enabled = false;
        perspectiveSwitchCamera.enabled = false;
        fpCamera.gameObject.SetActive(true);
        mapCamera.gameObject.SetActive(true);
        Cursor.visible = true;

        PlayerCore playerCore = PlayerCore.Instance;

        playerCore.transform.parent = dronePositionInHarvester;
        playerCore.GetComponent<NavMeshAgent>().enabled = false;
        playerCore.transform.localPosition = Vector3.zero;
        playerCore.transform.localRotation = Quaternion.identity;
        playerCore.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        playerCore.shield.SetActive(false);

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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private IEnumerator AnimateCameraSwitch(CameraPerspective fromPerspective)
    {
        //Lerp from harvester to player or vice versa
        yield return null;

        Camera startCam = fromPerspective == CameraPerspective.FPV ? fpCamera : droneCamera;
        Camera endCam = fromPerspective == CameraPerspective.FPV ? droneCamera : fpCamera;

        perspectiveSwitchCamera.transform.position = startCam.transform.position;
        perspectiveSwitchCamera.transform.rotation = startCam.transform.rotation;

        perspectiveSwitchCamera.enabled = true;
        startCam.enabled = false;
        endCam.enabled = false;


        float t = 0f;
        while (t < animationDuration)
        {
            t += Time.deltaTime;
            perspectiveSwitchCamera.transform.position = Vector3.Lerp(startCam.transform.position, endCam.transform.position, animationCurve.Evaluate(t / animationDuration));
            perspectiveSwitchCamera.transform.rotation = Quaternion.Lerp(startCam.transform.rotation, endCam.transform.rotation, animationCurve.Evaluate(t / animationDuration));
            yield return null;
        }

        endCam.enabled = true;
        perspectiveSwitchCamera.enabled = false;
        SetPerspective(fromPerspective == CameraPerspective.FPV ? CameraPerspective.DRONE : CameraPerspective.FPV);
    }
}
