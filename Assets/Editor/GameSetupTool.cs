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
            Debug.Log("[GameSetupTool] Scene cleared");
        }
        else
        {
            Debug.Log("[GameSetupTool] Scene is already clean");
        }
    }
}
