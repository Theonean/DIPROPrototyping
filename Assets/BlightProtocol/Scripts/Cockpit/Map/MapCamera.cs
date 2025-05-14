using System;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    public float yPos;
    float xOffset;
    float zOffset;
    [SerializeField] private float minYPos;
    [SerializeField] private float maxYPos;
    [SerializeField] private float maxZoomSpeed = 10f; // Max units per second the camera can zoom
    private float targetYPos; // The desired y position we're smoothly moving toward
    public UnityEngine.Rendering.Universal.FullScreenPassRendererFeature depthPass;

    [Header("Reveal Radius")]
    [SerializeField] private UnityEngine.Rendering.Universal.FullScreenPassRendererFeature maskCompositing;
    [SerializeField] private float revealRadius = 10f;
    private Harvester harvester;

    void Start()
    {
        harvester = Harvester.Instance; 
        yPos = transform.position.y;
        targetYPos = yPos; // Initialize target position
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
            ApplyRevealRadius();
        }

        transform.position = new Vector3(Harvester.Instance.transform.position.x + xOffset, yPos, Harvester.Instance.transform.position.z + zOffset);
    }

    public void SetHeight(float normalizedValue)
    {
        // Set the target height instead of directly setting yPos
        float height = Mathf.Lerp(minYPos, maxYPos, normalizedValue);
        targetYPos = height;
        Debug.Log(height);
    }

    public void Move(Vector2 delta) {
        xOffset += delta.x;
        zOffset += delta.y;
    }

    private void ApplyRevealRadius() {
        // transform range from world to screen space
        if (harvester == null) {
            harvester = Harvester.Instance;
        }
        Vector3 worldSpaceRangeEnd = harvester.transform.position + new Vector3(0f, 0f, revealRadius);
        float screenSpaceRange = cam.WorldToViewportPoint(worldSpaceRangeEnd).y - 0.5f;
        maskCompositing.passMaterial.SetFloat("_RingSize", screenSpaceRange);
    }
}