#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class WebGLBuildScript
{
    // Duplicate menu item - using GreenCorridorMenu.cs instead
    // [MenuItem("Tools/Green Corridor/Build WebGL for Localhost")]
    static void BuildWebGL()
    {
        // Build path
        string buildPath = Path.Combine(Application.dataPath, "../Builds/WebGL");
        
        // Ensure directory exists
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        // Get all scenes
        string[] scenes = {
            "Assets/_Scenes/GreenCorridor_Ready.unity"
        };
        
        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };
        
        Debug.Log("Building WebGL for localhost...");
        Debug.Log($"Build path: {buildPath}");
        
        // Execute build
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;
        
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"✅ WebGL build succeeded! Size: {summary.totalSize} bytes");
            Debug.Log($"Build location: {buildPath}");
            Debug.Log("\n🚀 To run on localhost:");
            Debug.Log($"1. cd {buildPath}");
            Debug.Log("2. python3 -m http.server 8000");
            Debug.Log("3. Open browser: http://localhost:8000");
            
            // Create server script
            CreateServerScript(buildPath);
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.LogError("❌ WebGL build failed!");
        }
    }
    
    static void CreateServerScript(string buildPath)
    {
        // Create Python server script
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
        print(f'📂 Serving from: {os.getcwd()}')
        print('Press Ctrl+C to stop')
        
        # Auto-open browser
        webbrowser.open(url)
        
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print('\n🛑 Server stopped')
";
        
        string scriptPath = Path.Combine(buildPath, "run_server.py");
        File.WriteAllText(scriptPath, serverScript);
        
        // Make executable on Unix systems
        if (System.Environment.OSVersion.Platform == System.PlatformID.Unix ||
            System.Environment.OSVersion.Platform == System.PlatformID.MacOSX)
        {
            System.Diagnostics.Process.Start("chmod", $"+x {scriptPath}");
        }
        
        // Create shell script for easy running
        string shellScript = $"#!/bin/bash\ncd \"{buildPath}\"\npython3 run_server.py\n";
        string shellPath = Path.Combine(buildPath, "run.sh");
        File.WriteAllText(shellPath, shellScript);
        
        if (System.Environment.OSVersion.Platform == System.PlatformID.Unix ||
            System.Environment.OSVersion.Platform == System.PlatformID.MacOSX)
        {
            System.Diagnostics.Process.Start("chmod", $"+x {shellPath}");
        }
        
        Debug.Log($"✅ Server scripts created in: {buildPath}");
    }
}
#endif

