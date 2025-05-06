using UnityEditor;
using UnityEngine;
using System.IO;

public class ReverseEmbeddedAnimation : Editor
{
    [MenuItem("Tools/Reverse Embedded FBX Animation")]
    private static void ReverseFbxAnimation()
    {
        var clip = Selection.activeObject as AnimationClip;
        if (clip == null)
        {
            Debug.LogError("Select an embedded AnimationClip in the Project view (e.g., inside an FBX).");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(clip);
        if (Path.GetExtension(assetPath).ToLower() != ".fbx")
        {
            Debug.LogError("Selected clip is not from an FBX file.");
            return;
        }

        string fbxDir = Path.GetDirectoryName(assetPath);
        string reversedName = clip.name + "_Reversed";
        string reversedPath = Path.Combine(fbxDir, reversedName + ".anim").Replace("\\", "/");

        // Duplicate clip into writable asset
        AnimationClip reversedClip = new AnimationClip
        {
            name = reversedName,
            frameRate = clip.frameRate
        };

        foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
        {
            AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(clip, binding);
            if (originalCurve == null || originalCurve.length == 0) continue;

            float lastTime = originalCurve.keys[originalCurve.length - 1].time;
            Keyframe[] reversedKeys = new Keyframe[originalCurve.length];

            for (int i = 0; i < originalCurve.length; i++)
            {
                Keyframe originalKey = originalCurve.keys[i];
                reversedKeys[i] = new Keyframe(
                    lastTime - originalKey.time,
                    originalKey.value,
                    -originalKey.outTangent,
                    -originalKey.inTangent
                );
            }

            System.Array.Sort(reversedKeys, (a, b) => a.time.CompareTo(b.time));
            AnimationCurve reversedCurve = new AnimationCurve(reversedKeys);
            AnimationUtility.SetEditorCurve(reversedClip, binding, reversedCurve);
        }

        AssetDatabase.CreateAsset(reversedClip, reversedPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Reversed animation saved at: {reversedPath}");
    }
}
