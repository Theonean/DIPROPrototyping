using UnityEditor;
using UnityEngine;
using System.IO;

public class ComponentTypeGeneratorEditor : EditorWindow
{
    private string inputName = "Falcon";
    private RocketComponentType componentType = RocketComponentType.FRONT;

    private string scriptOutputDir = "Assets/BlightProtocol/Scripts/Rockets/Generated/";
    private string soOutputDir = "Assets/BlightProtocol/Gameplay/Items/Generated/";

    [MenuItem("Tools/Component Type Generator")]
    public static void ShowWindow()
    {
        GetWindow<ComponentTypeGeneratorEditor>("Component Type Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Component Variant Creator", EditorStyles.boldLabel);
        inputName = EditorGUILayout.TextField("Name", inputName);
        componentType = (RocketComponentType)EditorGUILayout.EnumPopup("Component Type", componentType);

        if (GUILayout.Button("Generate Prefabs & Scriptable Object"))
        {
            GenerateVariants(inputName, componentType);
        }
    }

    private void GenerateVariants(string namePart, RocketComponentType type)
    {
        string typeStr = type.ToString();
        string componentTemplatePath = $"Assets/BlightProtocol/Player/RocketComponents/";

        string collectibleTemplatePath = $"Assets/BlightProtocol/Gameplay/Items/P_CollectibleTemplate.prefab";

        string componentOutputPath = $"Assets/BlightProtocol/Player/RocketComponents/Generated/";
        string collectibleOutputPath = $"Assets/BlightProtocol/Gameplay/Items/Generated/{namePart}Collectible.prefab";


        switch (type)
        {
            case RocketComponentType.FRONT:
                scriptOutputDir += "Front/";
                componentTemplatePath += "Front/P_RocketFrontTemplate.prefab";
                componentOutputPath += $"Front/{namePart}{typeStr}.prefab";
                break;
            case RocketComponentType.BODY:
                scriptOutputDir += "Body/";
                componentTemplatePath += "Front/P_RocketBodyTemplate.prefab";
                componentOutputPath += $"Body/{namePart}{typeStr}.prefab";
                break;
            case RocketComponentType.PROPULSION:
                scriptOutputDir += "Propulsion/";
                componentTemplatePath += "Front/P_RocketPropulsionTemplate.prefab";
                componentOutputPath += $"Propulsion/{namePart}{typeStr}.prefab";
                break;
        }

        string scriptPath = $"{scriptOutputDir}{namePart}{typeStr}.cs";
        string soPath = $"{soOutputDir}SO_{namePart}{typeStr}.asset";

        Directory.CreateDirectory(scriptOutputDir);
        Directory.CreateDirectory(soOutputDir);

        // 1. Create Component Prefab
        GameObject componentPrefab = CreateVariant(componentTemplatePath, componentOutputPath);
        if (componentPrefab == null) return;

        // 2. Create and attach script
        string baseClass = GetBaseClassForType(type);
        CreateComponentScript(namePart, typeStr, baseClass, scriptPath);

        // 3. Create SOItem
        var soItem = ScriptableObject.CreateInstance<SOItem>();
        soItem.prefab = componentPrefab;
        AssetDatabase.CreateAsset(soItem, soPath);

        // 4. Create Collectible Prefab
        GameObject collectiblePrefab = CreateVariant(collectibleTemplatePath, collectibleOutputPath);
        if (collectiblePrefab == null) return;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Created: {componentOutputPath}, {collectibleOutputPath}, {soPath}");
        Debug.LogWarning("Don't Forget to attach the component script to the component prefab root gameobject!");
    }

    private GameObject CreateVariant(string templatePath, string outputPath)
    {
        var template = AssetDatabase.LoadAssetAtPath<GameObject>(templatePath);
        if (!template)
        {
            Debug.LogError($"❌ Missing template at: {templatePath}");
            return null;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(template);
        PrefabUtility.SaveAsPrefabAsset(instance, outputPath);
        GameObject.DestroyImmediate(instance);
        return AssetDatabase.LoadAssetAtPath<GameObject>(outputPath);
    }

    private void CreateComponentScript(string name, string typeStr, string baseClass, string scriptPath)
    {
        if (File.Exists(scriptPath)) return;

        string content =
$@"using UnityEngine;

public class {name}{typeStr} : {baseClass}
{{
    // Auto-generated component
}}";
        File.WriteAllText(scriptPath, content);
    }

    private string GetBaseClassForType(RocketComponentType type)
    {
        return type switch
        {
            RocketComponentType.FRONT => "ACRocketFront",
            RocketComponentType.BODY => "ACRocketBody",
            RocketComponentType.PROPULSION => "ACRocketPropulsion",
            _ => "MonoBehaviour"
        };
    }
}
