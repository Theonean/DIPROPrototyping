using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[SerializeField]
public enum CameraPerspective
{
    DRONE,
    FPV,
    SWITCHING
}

public class PerspectiveSwitcher : MonoBehaviour
{
    public static PerspectiveSwitcher Instance { get; private set; }

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private AnimationCurve animationCurve;

    [Header("Cameras")]
    [SerializeField] private Camera droneCamera;
    [SerializeField] private Camera gogglesCamera;
    [SerializeField] private Camera fpCamera;

    [Header("Spawn Positions")]
    [SerializeField] private Transform dronePositionInLoadingBay;
    [SerializeField] private float droneSpawnDistance = 10f;
    [SerializeField] private Transform dronePositionInHarvester;

    public CameraPerspective currentPerspective { get; private set; } = CameraPerspective.DRONE;
    public UnityEvent onPerspectiveSwitched;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        SetTopDownPerspective();
    }

    private void Update()
    {
        // Handle input only in SWITCHING mode
        if (currentPerspective == CameraPerspective.SWITCHING)
        {
            // always look at the Harvester
            var harvPos = Harvester.Instance.transform.position;
            droneCamera.transform.LookAt(harvPos);

            // Left‐click: pick spawn point & go into DRONE
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = droneCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        Vector3 spawnPosition = hit.point;
                        spawnPosition.y = DroneMovement.Instance.distanceFromGround;

                        PlayerCore.Instance.spawnPosition = spawnPosition;

                        SetPerspective(CameraPerspective.DRONE);
                    }
                }
            }
            // Right‐click: go straight into FPV
            else if (Input.GetMouseButtonDown(1))
            {
                SetPerspective(CameraPerspective.FPV);
            }
        }
    }

    /// <summary>
    /// Public entry to switch perspectives.
    /// </summary>
    public void SetPerspective(CameraPerspective perspective)
    {
        if (currentPerspective == perspective) return;

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
                SetSwitchingPerspective();
                break;
        }
    }

    private void SetTopDownPerspective()
    {
        // turn on the drone (top-down) camera
        droneCamera.enabled = true;
        gogglesCamera.enabled = false;
        fpCamera.gameObject.SetActive(false);

        // detach & place the player at the chosen spawn
        var playerCore = PlayerCore.Instance;
        playerCore.transform.parent = null;

        Vector3 spawnPos = playerCore.spawnPosition;
        if (spawnPos == Vector3.zero)
            spawnPos = GetDroneRespawnPosition();
        spawnPos.y = DroneMovement.Instance.distanceFromGround;
        playerCore.transform.position = spawnPos;

        // reset physics & visuals
        var rb = playerCore.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = false;

        playerCore.transform.rotation = Quaternion.identity;
        playerCore.transform.localScale = Vector3.one;
        playerCore.shield.SetActive(true);

        // re-enable colliders so UI works
        foreach (var c in playerCore.GetComponentsInChildren<Collider>())
            c.enabled = true;

        Cursor.lockState = CursorLockMode.None;
        Shader.SetGlobalFloat("_isTopDown", 1);

        // clear the spawn marker
        playerCore.spawnPosition = Vector3.zero;
    }

    private void SetFPVPerspective()
    {
        // cockpit view
        droneCamera.enabled = false;
        gogglesCamera.enabled = true;
        fpCamera.gameObject.SetActive(true);

        var playerCore = PlayerCore.Instance;
        var rb = playerCore.GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = false;

        // reattach rockets
        var rocketCtrl = playerCore.GetComponentInChildren<RocketAimController>();
        rocketCtrl.Rocket1.ReattachRocketToDrone();
        rocketCtrl.Rocket2.ReattachRocketToDrone();
        rocketCtrl.Rocket3.ReattachRocketToDrone();
        rocketCtrl.Rocket4.ReattachRocketToDrone();

        // parent into Harvester
        playerCore.transform.parent = dronePositionInHarvester;
        playerCore.transform.localPosition = Vector3.zero;
        playerCore.transform.localRotation = Quaternion.identity;
        playerCore.transform.localScale = Vector3.one * 0.6f;
        playerCore.shield.SetActive(false);

        // disable colliders so UI clicks go through
        foreach (var c in playerCore.GetComponentsInChildren<Collider>())
            c.enabled = false;

        Shader.SetGlobalFloat("_isTopDown", 0);
    }

    private void SetSwitchingPerspective()
    {
        // top‐down preview mode focused on Harvester
        droneCamera.enabled = true;
        gogglesCamera.enabled = false;
        fpCamera.gameObject.SetActive(false);

        // unlock cursor so user can click
        Cursor.lockState = CursorLockMode.None;
    }

    public void EnableTopDownPreview(bool enable)
    {
        gogglesCamera.enabled = enable;
    }

    public Vector3 GetDroneRespawnPosition()
    {
        Vector3 basePos = dronePositionInLoadingBay.position
                        - Harvester.Instance.transform.forward * droneSpawnDistance;
        basePos.y = DroneMovement.Instance.distanceFromGround;
        return basePos;
    }
}
