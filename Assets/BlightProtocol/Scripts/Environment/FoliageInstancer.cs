using System.Collections.Generic;
using UnityEngine;

public class FoliageInstancer : MonoBehaviour
{
    [SerializeField] int density;
    [SerializeField] float noiseScale = 0.01f;
    [SerializeField] float distanceInfluence = 0.5f;
    [SerializeField] Vector2 positionRange = new Vector2(10, 10);
    [SerializeField] float randomPosOffset;
    [SerializeField] Vector2 scaleRange = new Vector2(0.8f, 1.5f);
    [SerializeField] Vector3 rotationOffset = new Vector3(90, 0, 0);
    [SerializeField] Mesh mesh;
    public Material material;
    private List<List<Matrix4x4>> Batches = new List<List<Matrix4x4>>();

    private void RenderBatches()
    {
        RenderParams rp = new RenderParams(material);
        foreach (var batch in Batches)
        {
            Graphics.RenderMeshInstanced(rp, mesh, 0, batch);
        }
    }

    void Update()
    {
        RenderBatches();
    }

    void Start()
    {
        int addedMatrices = 0;
        Batches.Add(new List<Matrix4x4>());

        float stepSize = positionRange.x * 2f / density;

        for (float x = -positionRange.x * 0.5f; x < positionRange.x * 0.5; x += stepSize)
        {
            for (float y = -positionRange.y * 0.5f; y < positionRange.y * 0.5; y += stepSize)
            {
                if (SampleNoise(x, y))
                {
                    if (addedMatrices < 1000)
                    {
                        Vector3 offsetPos = new Vector3(x + Random.Range(-randomPosOffset, randomPosOffset), 0f, y + Random.Range(-randomPosOffset, randomPosOffset));

                        Batches[Batches.Count - 1].Add(Matrix4x4.TRS(
                            transform.TransformPoint(offsetPos),
                            Quaternion.Euler(0 + rotationOffset.x, Random.rotation.y + rotationOffset.y, 0f + rotationOffset.z),
                            Vector3.one * Random.Range(scaleRange.x, scaleRange.y)));
                        addedMatrices++;
                    }
                    else
                    {
                        Batches.Add(new List<Matrix4x4>());
                        addedMatrices = 0;
                    }
                }
            }
        }
    }

    private bool SampleNoise(float x, float y)
    {
        float value = Mathf.PerlinNoise((x + positionRange.x) * noiseScale, (y + positionRange.y) * noiseScale);
        float normalizedDistance = (1f - Vector2.Distance(new Vector2(x,y), Vector2.zero) / positionRange.x);
        value = (value + normalizedDistance * distanceInfluence) * normalizedDistance;
        if (value > 0.5f) return true;
        else return false;
    }
}
