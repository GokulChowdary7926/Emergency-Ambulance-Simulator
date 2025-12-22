#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Diagnostics;

public class AutoBuildAndRun
{
    // Command line entry point
    static void BuildWebGLCommandLine()
    {
        string buildPath = Path.Combine(Application.dataPath, "../Builds/WebGL");
        BuildWebGL(buildPath);
    }
    
    // Menu item moved to GreenCorridorMenu.cs to avoid duplicates
    // [MenuItem("Tools/Green Corridor/Build WebGL and Run on Localhost")]
    static void BuildAndRunInternal()
    {
        string buildPath = Path.Combine(Application.dataPath, "../Builds/WebGL");
        
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        // Check if already built
        string indexHtml = Path.Combine(buildPath, "index.html");
        if (File.Exists(indexHtml))
        {
            UnityEngine.Debug.Log("WebGL build already exists. Starting server...");
            StartServer(buildPath);
            return;
        }
        
        // Build WebGL
        UnityEngine.Debug.Log("Building WebGL...");
        
        string[] scenes = {
            "Assets/_Scenes/GreenCorridor_Ready.unity"
        };
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"✅ Build succeeded! Size: {summary.totalSize} bytes");
            StartServer(buildPath);
        }
        else
        {
            UnityEngine.Debug.LogError("❌ Build failed!");
        }
    }
    
    static void BuildWebGL(string buildPath)
    {
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        string[] scenes = {
            "Assets/_Scenes/GreenCorridor_Ready.unity"
        };
        
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"✅ Build succeeded! Size: {summary.totalSize} bytes");
            EditorApplication.Exit(0);
        }
        else
        {
            UnityEngine.Debug.LogError("❌ Build failed!");
            EditorApplication.Exit(1);
        }
    }
    
    static void StartServer(string buildPath)
    {
        string serverScript = Path.Combine(buildPath, "run_server.py");
        
        if (!File.Exists(serverScript))
        {
            UnityEngine.Debug.LogWarning("Server script not found. Creating...");
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
    }
}
#endif

