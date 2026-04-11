using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool: one-click setup/clear game scene.
/// Creates the GameBootstrapper root object.
/// Sub-systems are created dynamically by Bootstrapper at runtime.
/// </summary>
public class GameSetupTool
{
    [MenuItem("Tools/Game Tools/Setup Game Scene")]
    public static void SetupGameScene()
    {
        GameObject existing = GameObject.Find("[GameBootstrapper]");
        if (existing != null)
        {
            Debug.LogWarning("[GameSetupTool] GameBootstrapper already exists in scene");
            Selection.activeGameObject = existing;
            return;
        }

        GameObject root = new GameObject("[GameBootstrapper]");
        root.AddComponent<GameBootstrapper>();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[GameSetupTool] GameBootstrapper created");
        Selection.activeGameObject = root;
    }

    [MenuItem("Tools/Game Tools/Clear Game Scene")]
    public static void ClearGameScene()
    {
        GameObject existing = GameObject.Find("[GameBootstrapper]");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
            Debug.Log("[GameSetupTool] Bootstrapper root cleared");
        }
        
        // Also clear common leftovers
        ClearUI();
        Debug.Log("[GameSetupTool] Game scene cleaned");
    }

    public static void ClearUI()
    {
        GameObject canvas = GameObject.Find("GameCanvas");
        if (canvas != null) Object.DestroyImmediate(canvas);
        GameObject eventSystem = GameObject.Find("EventSystem");
        if (eventSystem != null) Object.DestroyImmediate(eventSystem);
    }

    [MenuItem("Tools/Game Tools/[REBUILD FULL PROJECT]")]
    public static void RebuildFullProject()
    {
        Debug.Log("--- Starting Full Project Rebuild ---");

        // 0. Setup Project Settings (Layers & Tags) first!
        SetupLayersAndTags();
        
        // 1. Build assets (Resources/Prefabs/etc)
        PrefabBuilder.BuildAllPrefabs();
        
        // 2. Clear current scene to avoid overlaps
        ClearGameScene();
        
        // 3. Build UI
        UISceneBuilder.BuildUIScene();
        
        // 4. Setup Bootstrapper
        SetupGameScene();
        
        Debug.Log("--- Full Project Rebuild Complete! ---");
    }

    /// <summary>
    /// Programmatically adds required Layers and Tags to the Project Settings.
    /// This fixes the issue where .unitypackages don't carry over Layers.
    /// </summary>
    public static void SetupLayersAndTags()
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        
        // 1. Setup Tags
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        string[] requiredTags = { "Wall", "Enemy", "Pickup" };
        
        foreach (string tagName in requiredTags)
        {
            bool exists = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
                Debug.Log($"[GameSetup] Added missing Tag: {tagName}");
            }
        }

        // 2. Setup Layers
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        string[] requiredLayers = { "Player", "Enemy", "PlayerBullet", "EnemyBullet" };

        foreach (string layerName in requiredLayers)
        {
            bool exists = false;
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                if (layersProp.GetArrayElementAtIndex(i).stringValue == layerName)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                // Find first empty slot starting from index 8
                for (int i = 8; i < layersProp.arraySize; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(sp.stringValue))
                    {
                        sp.stringValue = layerName;
                        Debug.Log($"[GameSetup] Added missing Layer: {layerName} at index {i}");
                        break;
                    }
                }
            }
        }

        tagManager.ApplyModifiedProperties();
    }
}
