using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[SerializeField]
public enum CameraPerspective
{
    DRONE,
    FPV
}

public class PerspectiveSwitcher : MonoBehaviour
{
    public static PerspectiveSwitcher Instance { get; private set; }
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve animationCurve;
    [SerializeField] private Camera droneCamera, gogglesCamera;
    [SerializeField] private Camera fpCamera;
    [SerializeField] private Transform dronePositionInLoadingBay;
    [SerializeField] private float droneSpawnDistance = 10f;
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
        }
    }

    public void SetTopDownPerspective()
    {
        droneCamera.enabled = true;
        gogglesCamera.enabled = false;
        fpCamera.gameObject.SetActive(false);

        PlayerCore playerCore = PlayerCore.Instance;

        playerCore.transform.parent = null;

        Vector3 spawnPosition = GetDroneRespawnPosition();
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

        Shader.SetGlobalFloat("_isTopDown", 1);
    }

    public void SetFPVPerspective()
    {
        droneCamera.enabled = false;
        gogglesCamera.enabled = true;
        fpCamera.gameObject.SetActive(true);

        PlayerCore playerCore = PlayerCore.Instance;

        Rigidbody playerRB = playerCore.GetComponent<Rigidbody>();
        playerRB.linearVelocity = Vector3.zero;
        playerRB.isKinematic = false;

        RocketAimController rocketAimController = playerCore.GetComponentInChildren<RocketAimController>();
        rocketAimController.Rocket1.ReattachRocketToDrone();
        rocketAimController.Rocket2.ReattachRocketToDrone();
        rocketAimController.Rocket3.ReattachRocketToDrone();
        rocketAimController.Rocket4.ReattachRocketToDrone();

        playerCore.transform.parent = dronePositionInHarvester;
        playerCore.transform.localPosition = Vector3.zero;
        playerCore.transform.localRotation = Quaternion.identity;
        playerCore.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        playerCore.shield.SetActive(false);

        Harvester harvester = Harvester.Instance;
        //playerCore.transform.LookAt(harvester.mover.targetPosObject.transform.position);

        //Make players colliders not make UI Rocket selector to be unclickable
        Collider[] playerColliders = playerCore.GetComponentsInChildren<Collider>();
        foreach (Collider pC in playerColliders)
        {
            pC.enabled = false;
        }
        Shader.SetGlobalFloat("_isTopDown", 0);
    }

    public void EnableTopDownPreview(bool enable)
    {
        gogglesCamera.enabled = enable;
    }

    public void OverrideToFPV()
    {
        SetPerspective(CameraPerspective.FPV);
        FPVInputManager.Instance.fpvCamRotator.ChangePosition(1);
    }

    public Vector3 GetDroneRespawnPosition()
    {
        Vector3 pos = dronePositionInLoadingBay.position - Harvester.Instance.transform.forward * droneSpawnDistance;
        pos.y = DroneMovement.Instance.distanceFromGround;
        return pos;
    }

}
