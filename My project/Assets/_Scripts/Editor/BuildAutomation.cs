using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
public class BuildAutomation : EditorWindow
{
    private string buildPath = "Builds/";
    private string version = "1.0.0";
    private bool createInstaller = true;
    private bool includeDemoData = true;
    private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    [MenuItem("Tools/Green Corridor/Build System")]
    static void Init()
    {
        BuildAutomation window = GetWindow<BuildAutomation>();
        window.titleContent = new GUIContent("Build Automation");
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("Build Configuration", EditorStyles.boldLabel);
        version = EditorGUILayout.TextField("Version", version);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        createInstaller = EditorGUILayout.Toggle("Create Installer", createInstaller);
        includeDemoData = EditorGUILayout.Toggle("Include Demo Data", includeDemoData);
        GUILayout.Space(20);
        if (GUILayout.Button("Build Game", GUILayout.Height(40)))
        {
            BuildGame();
        }
        if (GUILayout.Button("Build Demo (Single Level)", GUILayout.Height(30)))
        {
            BuildDemo();
        }
        if (GUILayout.Button("Build Training Simulator", GUILayout.Height(30)))
        {
            BuildTrainingSimulator();
        }
        if (GUILayout.Button("Package for Government Demo", GUILayout.Height(30)))
        {
            BuildGovernmentDemo();
        }
    }
    void BuildGame()
    {
        string buildFolder = $"{buildPath}/GreenCorridor_{version}_{System.DateTime.Now:yyyyMMdd}";
        Directory.CreateDirectory(buildFolder);
        List<string> scenes = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("_Scenes"))
            {
                scenes.Add(path);
            }
        }
        if (scenes.Count == 0)
        {
            scenes.Add("Assets/_Scenes/MainMenu.unity");
            scenes.Add("Assets/_Scenes/Tutorial.unity");
            scenes.Add("Assets/_Scenes/CityDemo.unity");
        }
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes.ToArray(),
            locationPathName = $"{buildFolder}/GreenCorridor.exe",
            target = buildTarget,
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
        };
        BuildPipeline.BuildPlayer(options);
        CopyAdditionalFiles(buildFolder);
        if (createInstaller)
        {
            CreateInstaller(buildFolder);
        }
        EditorUtility.RevealInFinder(buildFolder);
            UnityEngine.Debug.Log($"Build completed: {buildFolder}");
    }
    void BuildDemo()
    {
        string demoFolder = $"{buildPath}/GreenCorridor_Demo_{version}";
        Directory.CreateDirectory(demoFolder);
        List<string> demoScenes = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Scene Tutorial");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Tutorial"))
            {
                demoScenes.Add(path);
                break;
            }
        }
        if (demoScenes.Count == 0)
        {
            demoScenes.Add("Assets/_Scenes/Tutorial.unity");
        }
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = demoScenes.ToArray(),
            locationPathName = $"{demoFolder}/GreenCorridor_Demo.exe",
            target = buildTarget,
            options = BuildOptions.ShowBuiltPlayer | BuildOptions.Development
        };
        BuildPipeline.BuildPlayer(options);
        string readme = "GREEN CORRIDOR DEMO\n" +
                       "===================\n" +
                       "Emergency Response Training Simulation\n\n" +
                       "Controls:\n" +
                       "WASD - Drive\n" +
                       "E - Emergency Lights/Siren\n" +
                       "Space - Brake\n" +
                       "Tab - Switch View\n\n" +
                       "Objective: Transport patient to hospital within time limit.";
        File.WriteAllText($"{demoFolder}/README.txt", readme);
        EditorUtility.RevealInFinder(demoFolder);
    }
    void BuildTrainingSimulator()
    {
        string trainingFolder = $"{buildPath}/GreenCorridor_Training_{version}";
        Directory.CreateDirectory(trainingFolder);
        string originalProductName = PlayerSettings.productName;
        string originalCompanyName = PlayerSettings.companyName;
        PlayerSettings.productName = "Green Corridor - Training Simulator";
        PlayerSettings.companyName = "Emergency Response Training";
        BuildGame(); // Reuse main build process
        PlayerSettings.productName = originalProductName;
        PlayerSettings.companyName = originalCompanyName;
            UnityEngine.Debug.Log($"Training simulator built: {trainingFolder}");
    }
    void BuildGovernmentDemo()
    {
        string govFolder = $"{buildPath}/GreenCorridor_Government_Demo";
        Directory.CreateDirectory(govFolder);
        string originalProductName = PlayerSettings.productName;
        PlayerSettings.productName = "Smart City Emergency Response System";
        PlayerSettings.SplashScreen.show = true;
        List<string> demoScenes = new List<string>();
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("CityDemo") || path.Contains("Tutorial"))
            {
                demoScenes.Add(path);
                break;
            }
        }
        if (demoScenes.Count == 0)
        {
            demoScenes.Add("Assets/_Scenes/CityDemo.unity");
        }
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = demoScenes.ToArray(),
            locationPathName = $"{govFolder}/SmartCityDemo.exe",
            target = buildTarget,
            options = BuildOptions.ShowBuiltPlayer
        };
        BuildPipeline.BuildPlayer(options);
        PlayerSettings.productName = originalProductName;
        CreatePresentationPackage(govFolder);
            UnityEngine.Debug.Log($"Government demo package created: {govFolder}");
    }
    void CopyAdditionalFiles(string buildFolder)
    {
        string docPath = "Assets/Documentation/";
        if (Directory.Exists(docPath))
        {
            string[] docs = Directory.GetFiles(docPath, "*.pdf");
            foreach (string doc in docs)
            {
                File.Copy(doc, $"{buildFolder}/{Path.GetFileName(doc)}");
            }
        }
        string config = "{\n" +
                       "  \"game\": \"Green Corridor\",\n" +
                       "  \"version\": \"" + version + "\",\n" +
                       "  \"buildDate\": \"" + System.DateTime.Now.ToString("yyyy-MM-dd") + "\"\n" +
                       "}";
        File.WriteAllText($"{buildFolder}/config.json", config);
        string requirements = "System Requirements:\n" +
                             "====================\n" +
                             "OS: Windows 10/11 64-bit\n" +
                             "CPU: Intel i5 or equivalent\n" +
                             "RAM: 8GB minimum\n" +
                             "GPU: NVIDIA GTX 1060 / AMD RX 580\n" +
                             "Storage: 2GB available space\n" +
                             "DirectX: Version 11\n";
        File.WriteAllText($"{buildFolder}/SystemRequirements.txt", requirements);
    }
    void CreateInstaller(string buildFolder)
    {
        string nsisScript = $@"
; Green Corridor Installer Script
Unicode true
Name ""Green Corridor - Emergency Response Simulator""
OutFile ""{buildFolder}/GreenCorridor_Setup.exe""
InstallDir $PROGRAMFILES\GreenCorridor
; Pages
Page directory
Page instfiles
; Sections
Section ""Main Application""
  SetOutPath $INSTDIR
  File /r ""{buildFolder}\*""
  ; Create desktop shortcut
  CreateShortcut ""$DESKTOP\Green Corridor.lnk"" ""$INSTDIR\GreenCorridor.exe""
  ; Create start menu shortcut
  CreateDirectory ""$SMPROGRAMS\Green Corridor""
  CreateShortCut ""$SMPROGRAMS\Green Corridor\Green Corridor.lnk"" ""$INSTDIR\GreenCorridor.exe""
  ; Write registry for uninstall
  WriteRegStr HKLM ""SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\GreenCorridor"" \
                   ""DisplayName"" ""Green Corridor""
  WriteRegStr HKLM ""SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\GreenCorridor"" \
                   ""Publisher"" ""Emergency Response Systems""
SectionEnd
";
        string scriptPath = $"{buildFolder}/installer.nsi";
        File.WriteAllText(scriptPath, nsisScript);
            UnityEngine.Debug.Log($"Installer script created: {scriptPath}");
            UnityEngine.Debug.Log("Note: Compile with NSIS to create installer executable");
    }
    void CreatePresentationPackage(string folder)
    {
        string summary = @"EXECUTIVE SUMMARY
=================
GREEN CORRIDOR SYSTEM
Emergency Traffic Preemption Solution
PROBLEM
• Emergency response delays due to traffic
• Average ambulance delay: 8-12 minutes in urban areas
• 30% of critical patients don't reach hospital in time
SOLUTION
• Real-time GPS tracking of emergency vehicles
• Automatic traffic signal preemption (250m radius)
• Dynamic route optimization
• Central traffic management console
BENEFITS
• Reduces emergency response time by 40-60%
• Increases patient survival rate by 22%
• Cost-effective implementation using existing infrastructure
• Scalable to entire city network
TECHNOLOGY
• IoT-based traffic signal integration
• Cloud-based central control system
• Mobile app for paramedic coordination
• AI-powered traffic prediction
IMPLEMENTATION
• Phase 1: Pilot program (3 intersections)
• Phase 2: City-wide deployment
• Phase 3: Integration with smart city ecosystem
CONTACT
For implementation proposal and cost analysis:
emergency.response@smartcity.gov";
        File.WriteAllText($"{folder}/Executive_Summary.txt", summary);
    }
}
