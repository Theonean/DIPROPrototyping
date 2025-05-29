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
    private const float maxSpawnDistance = 75;
    private const float minSpawnDistance = 5;
    [SerializeField] private float droneSpawnDistance = 10f;
    [SerializeField] private Transform dronePositionInHarvester;
    private bool spawnPositionInrange;
    [SerializeField] private LayerMask rayCastLayerMask;

    [Header("Spawn position visualization")]
    [SerializeField] private LineRenderer targetPositionLine;
    [SerializeField] private SpriteRenderer droneIcon;

    private float switchingCooldownTime = .5f;
    public float cooldownTimer = 0f;

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
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        // Handle input only in SWITCHING mode
        if (currentPerspective == CameraPerspective.SWITCHING)
        {
            // always look at the Harvester
            var harvPos = Harvester.Instance.transform.position;
            CameraTracker.Instance.objectToTrack = Harvester.Instance.gameObject;

            Ray ray = droneCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 spawnPosition = Vector3.zero;
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f, rayCastLayerMask))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    spawnPosition = hit.point;
                    spawnPosition.y = DroneMovement.Instance.distanceFromGround;
                }
            }

            //Set position 0 of line to harvPos and Position 1 to spawnPosition
            targetPositionLine.SetPosition(0, harvPos);
            targetPositionLine.SetPosition(1, spawnPosition);
            droneIcon.transform.position = spawnPosition;

            if (Vector3.Distance(harvPos, spawnPosition) is < maxSpawnDistance and > minSpawnDistance && !PlayerCore.Instance.isDead)
            {
                if (!spawnPositionInrange)
                {
                    spawnPositionInrange = true;
                    droneIcon.color = Color.green;
                    targetPositionLine.endColor = Color.green;
                }
            }
            else
            {
                if (spawnPositionInrange)
                {
                    spawnPositionInrange = false;
                    droneIcon.color = Color.red;
                    targetPositionLine.endColor = Color.red;
                }
            }


            // Left‐click: pick spawn point & go into DRONE
            if (Input.GetMouseButtonDown(0) && spawnPosition != Vector3.zero && spawnPositionInrange)
            {
                PlayerCore.Instance.transform.position = spawnPosition;
                SetPerspective(CameraPerspective.DRONE);
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
                cooldownTimer = switchingCooldownTime;
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

    public void OnDroneEnterSetSwitching()
    {
            SetPerspective(CameraPerspective.SWITCHING);
    }

    public void OverrideToFPV()
    {
        SetPerspective(CameraPerspective.FPV);
        FPVInputManager.Instance.fpvCamRotator.ChangePosition(1);
    }

    private void SetTopDownPerspective()
    {
        // turn on the drone (top-down) camera
        droneCamera.enabled = true;
        gogglesCamera.enabled = false;
        fpCamera.gameObject.SetActive(false);

        targetPositionLine.enabled = false;
        droneIcon.enabled = false;

        // detach & place the player at the chosen spawn
        var playerCore = PlayerCore.Instance;
        playerCore.transform.parent = null;

        CameraTracker.Instance.objectToTrack = playerCore.gameObject;
        playerCore.ToggleDisplayDrone(true);

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
    }

    private void SetFPVPerspective()
    {
        // cockpit view
        droneCamera.enabled = false;
        gogglesCamera.enabled = true;
        fpCamera.gameObject.SetActive(true);

        targetPositionLine.enabled = false;
        droneIcon.enabled = false;

        PlayerCore.Instance.ToggleDisplayDrone(true);

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

        PlayerCore.Instance.ToggleDisplayDrone(false);

        targetPositionLine.enabled = true;
        droneIcon.enabled = true;

        // unlock cursor so user can click
        Cursor.lockState = CursorLockMode.None;
    }

    public void EnableTopDownPreview(bool enable)
    {
        gogglesCamera.enabled = enable;
    }
}
