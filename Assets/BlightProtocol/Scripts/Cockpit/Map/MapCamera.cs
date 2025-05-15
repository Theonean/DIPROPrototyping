using System;
using UnityEngine;
using UnityEngine.Rendering;

public enum MapCameraMode
{
    FPV,
    DRONE
}

public class MapCamera : MonoBehaviour
{
    public MapCameraMode mode = MapCameraMode.FPV;
    [SerializeField] private Camera cam;
    private Transform harvester, drone, followTransform;
    public float yPos;
    [SerializeField] private float droneYPos = 500f;
    float xOffset;
    float zOffset;
    [SerializeField] private float minYPos;
    [SerializeField] private float maxYPos;
    [SerializeField] private float maxZoomSpeed = 10f; // Max units per second the camera can zoom
    private float targetYPos; // The desired y position we're smoothly moving toward
    public UnityEngine.Rendering.Universal.FullScreenPassRendererFeature depthPass;

    [Header("Reveal Radius")]
    [SerializeField] private UnityEngine.Rendering.Universal.FullScreenPassRendererFeature maskCompositing;
    private float revealRadius = 10f;

    void Start()
    {
        harvester = Harvester.Instance.transform;
        drone = PlayerCore.Instance.transform;
        yPos = transform.position.y;
        targetYPos = yPos; // Initialize target position
        followTransform = drone;
        revealRadius = HarvesterAlarmHandler.Instance.GetComponent<SphereCollider>().radius;
    }

    void OnEnable()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.AddListener(OnPerspectiveSwitched);
    }

    void OnDisable()
    {
        PerspectiveSwitcher.Instance.onPerspectiveSwitched.RemoveListener(OnPerspectiveSwitched);
    }

    private void OnPerspectiveSwitched()
    {
        switch (PerspectiveSwitcher.Instance.currentPerspective)
        {
            case CameraPerspective.FPV:
                mode = MapCameraMode.FPV;
                followTransform = harvester;
                ApplyRevealRadius();
                break;
            case CameraPerspective.DRONE:
                mode = MapCameraMode.DRONE;
                followTransform = drone;
                yPos = droneYPos;
                maskCompositing.passMaterial.SetFloat("_RingSize", 1f);
                break;
        }
    }

    void Update()
    {
        // Smoothly move toward target y position
        if (!Mathf.Approximately(yPos, targetYPos))
        {
            float maxDelta = maxZoomSpeed * Time.deltaTime;
            yPos = Mathf.MoveTowards(yPos, targetYPos, maxDelta);

            // Update camera clipping planes
            cam.farClipPlane = yPos + 100;
            cam.nearClipPlane = yPos - minYPos + 10;
            if (mode == MapCameraMode.FPV) ApplyRevealRadius();
        }

        transform.position = new Vector3(followTransform.position.x + xOffset, yPos, followTransform.position.z + zOffset);
    }

    public void SetHeight(float normalizedValue)
    {
        // Set the target height instead of directly setting yPos
        float height = Mathf.Lerp(minYPos, maxYPos, normalizedValue);
        targetYPos = height;
        Debug.Log(height);
    }

    public void Move(Vector2 delta)
    {
        xOffset += delta.x;
        zOffset += delta.y;
    }

    private void ApplyRevealRadius()
    {
        // transform range from world to screen space
        if (harvester == null)
        {
            harvester = Harvester.Instance.transform;
        }
        Vector3 worldSpaceRangeEnd = harvester.transform.position + new Vector3(0f, 0f, revealRadius);
        float screenSpaceRange = cam.WorldToViewportPoint(worldSpaceRangeEnd).y - 0.5f;
        maskCompositing.passMaterial.SetFloat("_RingSize", screenSpaceRange);
    }
}