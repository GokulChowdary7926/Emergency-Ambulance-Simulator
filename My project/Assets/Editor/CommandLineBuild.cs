#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

public class CommandLineBuild
{
    // This method can be called from Unity command line
    public static void BuildWebGL()
    {
        string buildPath = Path.Combine(Application.dataPath, "../Builds/WebGL");
        
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        UnityEngine.Debug.Log($"Building WebGL to: {buildPath}");
        
        // Get scenes
        string[] scenes = {
            "Assets/_Scenes/GreenCorridor_Ready.unity"
        };
        
        // Check if scenes exist, if not use SampleScene
        List<string> validScenes = new List<string>();
        foreach (string scene in scenes)
        {
            if (File.Exists(Path.Combine(Application.dataPath, "../", scene)))
            {
                validScenes.Add(scene);
            }
        }
        
        if (validScenes.Count == 0)
        {
            // Use SampleScene as fallback
            validScenes.Add("Assets/Scenes/SampleScene.unity");
        }
        
        // Determine build target
        BuildTarget buildTarget = BuildTarget.WebGL;
        
        // Check if WebGL is available, if not try Standalone build
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            UnityEngine.Debug.LogWarning("⚠️ WebGL not available, trying Standalone build instead...");
            buildTarget = BuildTarget.StandaloneOSX;
            buildPath = Path.Combine(Application.dataPath, "../Builds/Standalone");
            
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            UnityEngine.Debug.Log($"Building Standalone to: {buildPath}");
        }
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = validScenes.ToArray(),
            locationPathName = buildPath,
            target = buildTarget,
            options = BuildOptions.None
        };
        
        UnityEngine.Debug.Log("Starting WebGL build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"✅ Build succeeded! Size: {summary.totalSize / 1024 / 1024} MB");
            UnityEngine.Debug.Log($"Build location: {buildPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            UnityEngine.Debug.LogError($"❌ Build failed! Errors: {summary.totalErrors}");
            EditorApplication.Exit(1);
        }
    }
}
#endif

