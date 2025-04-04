using UnityEngine;

public class MapMask : MonoBehaviour
{

    private RenderTexture maskTex;
    public Shader maskShader;
    private Material mapMaterial, maskMaterial;
    public int maskResolution = 1024;
    public float edgeSharpness = 5f;

    void Start()
    {
        maskMaterial = new Material(maskShader);
        maskMaterial.SetVector("_Color", Color.red);
        maskMaterial.SetFloat("_EdgeSharpness", edgeSharpness);

        mapMaterial = GetComponent<Renderer>().material;

        maskTex = new RenderTexture(maskResolution, maskResolution, 0, RenderTextureFormat.ARGBFloat);
        mapMaterial.SetTexture("_Mask", maskTex);
    }

    public void PaintOnMask(Vector2 coords, float range, float strength)
    {
        maskMaterial.SetVector("_Coordinates", new Vector4(coords.x, coords.y, 0, 0));
        maskMaterial.SetFloat("_Strength", strength);
        maskMaterial.SetFloat("_Size", range);

        RenderTexture temp = RenderTexture.GetTemporary(maskTex.width, maskTex.height, 0, RenderTextureFormat.ARGBFloat);

        // Copy the current mask texture into the temporary texture
        Graphics.Blit(maskTex, temp);

        // Use the temp texture as the input for the next pass
        maskMaterial.SetTexture("_MainTex", temp);

        // Blit using maskMaterial to apply the new paint without accumulating endlessly
        Graphics.Blit(temp, maskTex, maskMaterial);

        RenderTexture.ReleaseTemporary(temp);
    }
}
