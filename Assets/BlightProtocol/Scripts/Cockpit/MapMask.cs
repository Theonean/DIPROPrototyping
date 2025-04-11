using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapMask : MonoBehaviour
{
    public Shader maskShader;
    private RenderTexture _maskTexture;
    private Material _material;
    public int resolution = 1024;
    private Vector2 _mapBoundsX;
    private Vector2 _mapBoundsZ;
    public RenderObjectsPass maskPass;
    private Renderer renderer;

#if UNITY_EDITOR
    [Header("Editor Preview")]
    [SerializeField, Range(128, 512)] private int _previewSize = 256;
    [SerializeField] private bool _showPreview = true;
#endif

    void Start()
    {
        renderer = GetComponent<Renderer>();
        InitializeTexture();
    }
    private void InitializeTexture()
    {
        if (_maskTexture != null)
        {
            _maskTexture.Release();
            Destroy(_maskTexture);
        }

        _maskTexture = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBFloat)
        {
            enableRandomWrite = true
        };
        _maskTexture.Create();

        if (_material != null) Destroy(_material);
        _material = new Material(maskShader);

        var generator = ProceduralTileGenerator.Instance;
        _mapBoundsX = new Vector2(generator.mapBoundsX.x, generator.mapBoundsX.y);
        _mapBoundsZ = new Vector2(generator.mapBoundsZ.x, generator.mapBoundsZ.y);

        ClearTexture(_maskTexture, Color.clear);
        renderer.material.SetTexture("_MaskTex", _maskTexture);
    }

    public Vector2 GetBounds(int axis)
    {
        if (axis == 0) return _mapBoundsX;
        if (axis == 1) return _mapBoundsZ;
        return Vector2.zero;
    }

    public void Paint(Vector3 worldPos, float radius, float strength)
    {
        if (_maskTexture == null || _material == null)
            InitializeTexture();

        Vector2 uvPos = WorldToUV(worldPos);
        Debug.Log(uvPos);

        _material.SetVector("_Coordinates", new Vector4(uvPos.x, uvPos.y, 0, 0));
        _material.SetFloat("_Strength", strength);
        _material.SetFloat("_Size", radius);

        RenderTexture temp1 = RenderTexture.GetTemporary(_maskTexture.descriptor);
        RenderTexture temp2 = RenderTexture.GetTemporary(_maskTexture.descriptor);

        try
        {
            // First pass: Copy current mask
            Graphics.Blit(_maskTexture, temp1);

            // Second pass: Apply paint
            Graphics.Blit(temp1, temp2, _material);

            // Final pass: Write back to main texture
            Graphics.Blit(temp2, _maskTexture);
        }
        finally
        {
            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);
        }
    }

    private Vector2 WorldToUV(Vector3 worldPos)
    {
        float uvX = Mathf.InverseLerp(_mapBoundsX.x, _mapBoundsX.y, worldPos.x);
        float uvY = Mathf.InverseLerp(_mapBoundsZ.x, _mapBoundsZ.y, worldPos.z);

        return new Vector2(uvX, uvY);
    }

    private void ClearTexture(RenderTexture target, Color color)
    {
        RenderTexture.active = target;
        GL.Clear(true, true, color);
        RenderTexture.active = null;
    }

    private void OnDestroy()
    {
        if (_maskTexture != null)
        {
            _maskTexture.Release();
            Destroy(_maskTexture);
        }
        if (_material != null)
        {
            Destroy(_material);
        }
    }

    public RenderTexture GetMaskTexture() => _maskTexture;

#if UNITY_EDITOR
    [CustomEditor(typeof(MapMask))]
    public class PureShaderMaskGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var generator = (MapMask)target;

            if (generator._maskTexture == null)
            {
                EditorGUILayout.HelpBox("Texture not initialized. Enter Play Mode or call InitializeTexture().", MessageType.Info);
                return;
            }

            if (generator._showPreview && generator._maskTexture != null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Texture Preview", EditorStyles.boldLabel);

                Rect previewRect = GUILayoutUtility.GetRect(
                    generator._previewSize,
                    generator._previewSize
                );

                EditorGUI.DrawPreviewTexture(
                    previewRect,
                    generator._maskTexture,
                    null,
                    ScaleMode.ScaleToFit
                );

                if (GUILayout.Button("Clear Texture"))
                {
                    generator.ClearTexture(generator._maskTexture, Color.clear);
                }
            }
        }
    }
#endif
}