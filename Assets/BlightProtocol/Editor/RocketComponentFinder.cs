using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class RocketComponentFinder : EditorWindow
{
    private List<Type> rocketTypes = new List<Type>();
    private Dictionary<Type, List<string>> foundAssets = new Dictionary<Type, List<string>>();

    [MenuItem("Tools/Rocket Component Finder")]
    public static void ShowWindow()
    {
        GetWindow<RocketComponentFinder>("Rocket Component Finder");
    }

    private void OnEnable()
    {
        FindRocketTypes();
        FindRocketAssets();
    }

    private void FindRocketTypes()
    {
        rocketTypes.Clear();
        Assembly assembly = Assembly.GetAssembly(typeof(MonoBehaviour)); // Get all MonoBehaviour types

        foreach (Type type in assembly.GetTypes())
        {
            if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(MonoBehaviour)))
            {
                if (typeof(IRocketFront).IsAssignableFrom(type) || typeof(IRocketBody).IsAssignableFrom(type) || typeof(IRocketPropulsion).IsAssignableFrom(type))
                {
                    rocketTypes.Add(type);
                }
            }
        }
    }

    private void FindRocketAssets()
    {
        foundAssets.Clear();

        foreach (Type type in rocketTypes)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Prefab t:ScriptableObject t:Scene");
            List<string> assetPaths = new List<string>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (asset != null)
                {
                    // Check if any component on the asset matches our rocket types
                    if (asset.GetComponentsInChildren(type, true).Length > 0)
                    {
                        assetPaths.Add(path);
                    }
                }
            }

            if (assetPaths.Count > 0)
            {
                foundAssets[type] = assetPaths;
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Rocket Components in Project", EditorStyles.boldLabel);

        if (foundAssets.Count == 0)
        {
            EditorGUILayout.HelpBox("No RocketTip or RocketBody components found in project assets.", MessageType.Info);
            return;
        }

        foreach (var entry in foundAssets)
        {
            Type type = entry.Key;
            GUILayout.Label($"Class: {type.Name}", EditorStyles.boldLabel);

            foreach (var path in entry.Value)
            {
                EditorGUILayout.ObjectField("Asset", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), typeof(UnityEngine.Object), false);
            }
        }

        if (GUILayout.Button("Refresh"))
        {
            FindRocketAssets();
        }
    }
}
