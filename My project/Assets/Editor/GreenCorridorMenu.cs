#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;

public class GreenCorridorMenu
{
    [MenuItem("Tools/Green Corridor/Build WebGL and Run on Localhost")]
    static void BuildWebGLAndRun()
    {
        string buildPath = Path.Combine(Application.dataPath, "../Builds/WebGL");
        
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        UnityEngine.Debug.Log($"Building WebGL to: {buildPath}");
        
        string[] scenes = {
            "Assets/_Scenes/GreenCorridor_Ready.unity"
        };
        
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
            validScenes.Add("Assets/Scenes/SampleScene.unity");
        }
        
        BuildTarget buildTarget = BuildTarget.WebGL;
        
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            EditorUtility.DisplayDialog("WebGL Not Available", 
                "WebGL build target is not installed.\n\nPlease install WebGL module in Unity Hub:\n1. Open Unity Hub\n2. Click on Unity 2022.3.62f3\n3. Add Modules → WebGL Build Support\n4. Install", 
                "OK");
            return;
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
            
            StartServer(buildPath);
        }
        else
        {
            UnityEngine.Debug.LogError($"❌ Build failed! Errors: {summary.totalErrors}");
            EditorUtility.DisplayDialog("Build Failed", 
                $"Build failed with {summary.totalErrors} errors.\n\nCheck Console for details.", 
                "OK");
        }
    }
    
    static void StartServer(string buildPath)
    {
        string serverScript = Path.Combine(buildPath, "run_server.py");
        
        if (!File.Exists(serverScript))
        {
            CreateServerScript(buildPath);
        }
        
        UnityEngine.Debug.Log("🚀 Starting localhost server...");
        
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = $"\"{serverScript}\"",
            WorkingDirectory = buildPath,
            UseShellExecute = true,
            CreateNoWindow = false
        };
        
        Process.Start(startInfo);
        
        UnityEngine.Debug.Log("✅ Server started! Browser should open automatically.");
        UnityEngine.Debug.Log("🌐 Game running on: http://localhost:8000");
        
        System.Threading.Thread.Sleep(2000);
        Process.Start("http://localhost:8000");
    }
    
    static void CreateServerScript(string buildPath)
    {
        string serverScript = @"#!/usr/bin/env python3
import http.server
import socketserver
import os
import webbrowser
from pathlib import Path

PORT = 8000

class MyHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Cross-Origin-Embedder-Policy', 'require-corp')
        self.send_header('Cross-Origin-Opener-Policy', 'same-origin')
        super().end_headers()

if __name__ == '__main__':
    os.chdir(Path(__file__).parent)
    
    with socketserver.TCPServer(("", PORT), MyHTTPRequestHandler) as httpd:
        url = f'http://localhost:{PORT}'
        print(f'🚀 Green Corridor running on {url}')
        webbrowser.open(url)
        httpd.serve_forever()
";
        
        File.WriteAllText(Path.Combine(buildPath, "run_server.py"), serverScript);
        
        try
        {
            ProcessStartInfo chmod = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{Path.Combine(buildPath, "run_server.py")}\"",
                UseShellExecute = false
            };
            Process.Start(chmod).WaitForExit();
        }
        catch { }
    }
    
    [MenuItem("Tools/Green Corridor/Build WebGL Only")]
    static void BuildWebGLOnly()
    {
        string buildPath = Path.Combine(Application.dataPath, "../Builds/WebGL");
        
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        string[] scenes = {
            "Assets/_Scenes/GreenCorridor_Ready.unity"
        };
        
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
            validScenes.Add("Assets/Scenes/SampleScene.unity");
        }
        
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            EditorUtility.DisplayDialog("WebGL Not Available", 
                "WebGL build target is not installed.", 
                "OK");
            return;
        }
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = validScenes.ToArray(),
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("Build Complete", 
                $"Build succeeded!\n\nLocation: {buildPath}", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Build Failed", 
                $"Build failed with {summary.totalErrors} errors.", 
                "OK");
        }
    }
}
#endif


