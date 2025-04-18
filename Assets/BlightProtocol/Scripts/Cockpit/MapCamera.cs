using System;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    public float yPos;
    float xOffset;
    float zOffset;
    public float minYPos;
    public float maxYPos;
    public FullScreenPassRendererFeature depthPass;

    public Vector2 minDepthRange = new(0.11f, 0.57f);
    public Vector2 maxDepthRange = new(0.14f, 0.61f);
    void Awake()
    {
        yPos = transform.position.y;
        ApplyDepthValues();
    }

    void Update()
    {
        transform.position = new Vector3(Harvester.Instance.transform.position.x + xOffset, yPos, Harvester.Instance.transform.position.z + zOffset);
    }

    public void SetHeight(float height)
    {
        yPos = height;
        ApplyDepthValues();
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
}
