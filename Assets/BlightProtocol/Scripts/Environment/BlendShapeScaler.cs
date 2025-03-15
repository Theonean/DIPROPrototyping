using UnityEngine;

public class BlendShapeScaler : MonoBehaviour
{
    public Vector3 maxScale;
    public Vector3 offsetWithBlendShape;
    public int attributeNumber;
    public SkinnedMeshRenderer meshRenderer;

    // Start is called before the first frame update
    public void UpdateNavmeshBounds()
    {
        //Get the attribute value from the mesh renderer
        float attributeValue = meshRenderer.GetBlendShapeWeight(attributeNumber);
        float t = attributeValue / 100f;
        transform.localScale = Vector3.Lerp(Vector3.one, maxScale, t);
        transform.localPosition = Vector3.Lerp(Vector3.zero, offsetWithBlendShape, t);
    }
}
