#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class AutoSetupOnImport
{
    static AutoSetupOnImport()
    {
        EditorApplication.delayCall += SetupScene;
    }
    
    static void SetupScene()
    {
        // Check if scene needs setup
        if (EditorSceneManager.GetActiveScene().name == "GreenCorridor_Ready")
        {
            // Scene is already set up
            return;
        }
        
        // Auto-open the ready scene if it exists
        string scenePath = "Assets/_Scenes/GreenCorridor_Ready.unity";
        if (System.IO.File.Exists(scenePath))
        {
            EditorSceneManager.OpenScene(scenePath);
            Debug.Log("✅ Green Corridor Ready Scene opened automatically!");
            Debug.Log("🎮 Just press Play to start the game!");
        }
    }
}
#endif


