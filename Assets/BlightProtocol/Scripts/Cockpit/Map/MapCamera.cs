using System;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    private Camera cam;
    public float yPos;
    float xOffset;
    float zOffset;
    public float minYPos;
    public float maxYPos;
    public FullScreenPassRendererFeature depthPass;

    public Vector2 minDepthRange = new(0.11f, 0.57f);
    public Vector2 maxDepthRange = new(0.14f, 0.61f);

    [Header("Reveal Radius")]
    [SerializeField] private FullScreenPassRendererFeature maskCompositing;
    [SerializeField] private float  revealRadius = 10f;
    private Harvester harvester;

    void Start()
    {
        harvester = Harvester.Instance; 
        yPos = transform.position.y;
        cam = GetComponent<Camera>();
        ApplyDepthValues();
        ApplyRevealRadius();      
    }

    void Update()
    {
        transform.position = new Vector3(Harvester.Instance.transform.position.x + xOffset, yPos, Harvester.Instance.transform.position.z + zOffset);
    }

    public void SetHeight(float height)
    {
        yPos = height;
        ApplyDepthValues();
        ApplyRevealRadius();
    }

    public void Move(Vector2 delta) {
        xOffset += delta.x;
        zOffset += delta.y;
    }

    private void ApplyDepthValues()
    {
        if (depthPass?.passMaterial != null)
        {
            float minDepth = Mathf.Lerp(minDepthRange.x, minDepthRange.y, (yPos - minYPos) / (maxYPos - minYPos));
            float maxDepth = Mathf.Lerp(maxDepthRange.x, maxDepthRange.y, (yPos - minYPos) / (maxYPos - minYPos));;
            depthPass.passMaterial.SetFloat("_DepthMin", minDepth);
            depthPass.passMaterial.SetFloat("_DepthMax", maxDepth);
        }
        else
        {
            Debug.LogWarning("Depth pass or material is not assigned.");
        }
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
