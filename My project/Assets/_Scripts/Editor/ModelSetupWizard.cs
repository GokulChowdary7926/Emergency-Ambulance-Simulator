using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
public class ModelSetupWizard : EditorWindow
{
    private GameObject ambulanceModel;
    private Material ambulanceMaterial;
    private bool addLights = true;
    private bool addColliders = true;
    private bool optimizeMesh = true;
    [MenuItem("Tools/Green Corridor/Model Setup")]
    static void Init()
    {
        ModelSetupWizard window = GetWindow<ModelSetupWizard>();
        window.titleContent = new GUIContent("Model Setup Wizard");
        window.Show();
    }
    void OnGUI()
    {
        GUILayout.Label("Ambulance Setup", EditorStyles.boldLabel);
        ambulanceModel = (GameObject)EditorGUILayout.ObjectField("Ambulance Model", ambulanceModel, typeof(GameObject), false);
        ambulanceMaterial = (Material)EditorGUILayout.ObjectField("Material", ambulanceMaterial, typeof(Material), false);
        addLights = EditorGUILayout.Toggle("Add Emergency Lights", addLights);
        addColliders = EditorGUILayout.Toggle("Add Colliders", addColliders);
        optimizeMesh = EditorGUILayout.Toggle("Optimize Mesh", optimizeMesh);
        if (GUILayout.Button("Setup Ambulance"))
        {
            SetupAmbulance();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Setup Traffic Signal"))
        {
            SetupTrafficSignal();
        }
        if (GUILayout.Button("Setup Civilian Car"))
        {
            SetupCivilianCar();
        }
    }
    void SetupAmbulance()
    {
        if (ambulanceModel == null) return;
        GameObject ambulance = Instantiate(ambulanceModel);
        ambulance.name = "Ambulance_Prefab";
        Rigidbody rb = ambulance.AddComponent<Rigidbody>();
        rb.mass = 2000f;
        rb.drag = 0.3f;
        rb.angularDrag = 3f;
        ambulance.AddComponent<AmbulanceController>();
        ambulance.AddComponent<AmbulanceGPS>();
        ambulance.tag = "Ambulance";
        if (addColliders)
        {
            BoxCollider collider = ambulance.AddComponent<BoxCollider>();
            collider.size = new Vector3(2f, 2.5f, 5f);
            collider.center = new Vector3(0, 1.25f, 0);
        }
        if (addLights)
        {
            CreateEmergencyLights(ambulance);
        }
        if (ambulanceMaterial != null)
        {
            Renderer[] renderers = ambulance.GetComponentsInChildren<Renderer>();
            foreach (Renderer r in renderers)
            {
                r.material = ambulanceMaterial;
            }
        }
        string path = "Assets/_Prefabs/Vehicles/Ambulance.prefab";
        System.IO.Directory.CreateDirectory("Assets/_Prefabs/Vehicles");
        PrefabUtility.SaveAsPrefabAsset(ambulance, path);
        DestroyImmediate(ambulance);
        Debug.Log($"Ambulance prefab created at {path}");
    }
    void CreateEmergencyLights(GameObject vehicle)
    {
        GameObject lightBar = new GameObject("LightBar");
        lightBar.transform.parent = vehicle.transform;
        lightBar.transform.localPosition = new Vector3(0, 2.5f, 1f);
        GameObject redLight = new GameObject("RedLight");
        redLight.transform.parent = lightBar.transform;
        redLight.transform.localPosition = new Vector3(-0.3f, 0, 0);
        Light red = redLight.AddComponent<Light>();
        red.color = Color.red;
        red.intensity = 5f;
        red.range = 50f;
        red.type = LightType.Spot;
        red.spotAngle = 60f;
        GameObject blueLight = new GameObject("BlueLight");
        blueLight.transform.parent = lightBar.transform;
        blueLight.transform.localPosition = new Vector3(0.3f, 0, 0);
        Light blue = blueLight.AddComponent<Light>();
        blue.color = Color.blue;
        blue.intensity = 5f;
        blue.range = 50f;
        blue.type = LightType.Spot;
        blue.spotAngle = 60f;
        GameObject siren = new GameObject("SirenAudio");
        siren.transform.parent = vehicle.transform;
        siren.transform.localPosition = Vector3.zero;
        AudioSource audio = siren.AddComponent<AudioSource>();
        audio.spatialBlend = 1f;
        audio.rolloffMode = AudioRolloffMode.Logarithmic;
        audio.minDistance = 5f;
        audio.maxDistance = 200f;
    }
    void SetupTrafficSignal()
    {
        GameObject signal = new GameObject("TrafficSignal_Prefab");
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.parent = signal.transform;
        pole.transform.localScale = new Vector3(0.2f, 3f, 0.2f);
        pole.transform.localPosition = new Vector3(0, 1.5f, 0);
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.transform.parent = signal.transform;
        box.transform.localScale = new Vector3(1f, 1.5f, 0.3f);
        box.transform.localPosition = new Vector3(0, 3f, 0);
        string[] directions = { "North", "East", "South", "West" };
        Vector3[] positions = {
            new Vector3(0, 3.5f, 0.2f),
            new Vector3(0.2f, 3.5f, 0),
            new Vector3(0, 3.5f, -0.2f),
            new Vector3(-0.2f, 3.5f, 0)
        };
        for (int i = 0; i < 4; i++)
        {
            GameObject light = new GameObject($"Light_{directions[i]}");
            light.transform.parent = box.transform;
            light.transform.localPosition = positions[i];
            GameObject red = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            red.transform.parent = light.transform;
            red.transform.localScale = Vector3.one * 0.2f;
            red.transform.localPosition = new Vector3(0, 0.2f, 0);
            red.GetComponent<Renderer>().material.color = Color.red;
            GameObject yellow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            yellow.transform.parent = light.transform;
            yellow.transform.localScale = Vector3.one * 0.2f;
            yellow.transform.localPosition = Vector3.zero;
            yellow.GetComponent<Renderer>().material.color = Color.yellow;
            GameObject green = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            green.transform.parent = light.transform;
            green.transform.localScale = Vector3.one * 0.2f;
            green.transform.localPosition = new Vector3(0, -0.2f, 0);
            green.GetComponent<Renderer>().material.color = Color.green;
        }
        signal.AddComponent<TrafficSignal>();
        System.IO.Directory.CreateDirectory("Assets/_Prefabs/Infrastructure");
        PrefabUtility.SaveAsPrefabAsset(signal, "Assets/_Prefabs/Infrastructure/TrafficSignal.prefab");
        DestroyImmediate(signal);
    }
    void SetupCivilianCar()
    {
        GameObject car = new GameObject("CivilianCar_Prefab");
        car.AddComponent<Rigidbody>();
        car.AddComponent<CivilianCarAI>();
        car.tag = "CivilianCar";
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.parent = car.transform;
        body.transform.localScale = new Vector3(1.5f, 1f, 3f);
        System.IO.Directory.CreateDirectory("Assets/_Prefabs/Vehicles");
        PrefabUtility.SaveAsPrefabAsset(car, "Assets/_Prefabs/Vehicles/CivilianCar.prefab");
        DestroyImmediate(car);
        Debug.Log("Civilian car prefab created");
    }
}
