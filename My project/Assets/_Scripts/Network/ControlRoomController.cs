using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class ControlRoomController : MonoBehaviour
{
    [Header("UI References")]
    public Camera controlRoomCamera;
    public Camera[] cityCameras;
    public RawImage[] cameraFeeds;
    public Transform signalGrid;
    public GameObject signalButtonPrefab;
    [Header("Control Systems")]
    public TrafficManager trafficManager;
    [Header("Player Info")]
    public string playerName = "Traffic Controller";
    public int playerScore = 0;
    private Dictionary<int, TrafficSignal> controlledSignals = new Dictionary<int, TrafficSignal>();
    private int currentCameraIndex = 0;
    private bool isPaused = false;
    void Start()
    {
        InitializeControlRoom();
        SetupSignalGrid();
        if (cityCameras != null && cityCameras.Length > 0)
        {
            SwitchToCamera(0);
        }
    }
    void InitializeControlRoom()
    {
        TrafficSignal[] signals = FindObjectsOfType<TrafficSignal>();
        for (int i = 0; i < signals.Length; i++)
        {
            controlledSignals.Add(i, signals[i]);
        }
        if (cityCameras != null && cameraFeeds != null)
        {
            for (int i = 0; i < cityCameras.Length && i < cameraFeeds.Length; i++)
            {
                if (cityCameras[i] != null && cameraFeeds[i] != null)
                {
                    RenderTexture rt = new RenderTexture(256, 256, 16);
                    cityCameras[i].targetTexture = rt;
                    cameraFeeds[i].texture = rt;
                }
            }
        }
        Debug.Log($"Control Room Initialized. Controlling {controlledSignals.Count} signals.");
    }
    void SetupSignalGrid()
    {
        if (signalGrid == null || signalButtonPrefab == null) return;
        foreach (var kvp in controlledSignals)
        {
            GameObject button = Instantiate(signalButtonPrefab, signalGrid);
            SignalButtonUI buttonUI = button.GetComponent<SignalButtonUI>();
            if (buttonUI != null)
            {
                buttonUI.Initialize(kvp.Key, kvp.Value, this);
            }
        }
    }
    void Update()
    {
        HandleInput();
        UpdateUI();
    }
    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchToCamera(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchToCamera(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchToCamera(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchToCamera(3);
        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchToAmbulanceView();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleTrafficPause();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            OverrideAllSignals();
        }
    }
    void SwitchToCamera(int index)
    {
        if (cityCameras == null || index < 0 || index >= cityCameras.Length) return;
        foreach (Camera cam in cityCameras)
        {
            if (cam != null)
                cam.gameObject.SetActive(false);
        }
        if (cityCameras[index] != null)
        {
            cityCameras[index].gameObject.SetActive(true);
            currentCameraIndex = index;
        }
        Debug.Log($"Switched to camera {index}");
    }
    void SwitchToAmbulanceView()
    {
        GameObject ambulance = GameObject.FindGameObjectWithTag("Ambulance");
        if (ambulance != null)
        {
            GameObject followCam = new GameObject("FollowCamera");
            Camera cam = followCam.AddComponent<Camera>();
            SmoothFollow follow = followCam.AddComponent<SmoothFollow>();
            follow.target = ambulance.transform;
            if (cityCameras != null)
            {
                foreach (Camera c in cityCameras)
                {
                    if (c != null)
                        c.gameObject.SetActive(false);
                }
            }
            Debug.Log("Now following ambulance");
        }
    }
    void ToggleTrafficPause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        CivilianCarAI[] cars = FindObjectsOfType<CivilianCarAI>();
        foreach (var car in cars)
        {
            if (car != null)
                car.enabled = !isPaused;
        }
        Debug.Log($"Traffic {(isPaused ? "Paused" : "Resumed")}");
    }
    public void OverrideSignal(int signalId, string state)
    {
        if (controlledSignals.ContainsKey(signalId))
        {
            TrafficSignal signal = controlledSignals[signalId];
            Debug.Log($"Signal {signalId} override to {state}");
            AddScore(10);
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.SyncSignalState(signalId, state);
            }
        }
    }
    void OverrideAllSignals()
    {
        foreach (var kvp in controlledSignals)
        {
            OverrideSignal(kvp.Key, "red");
        }
        Debug.Log("All signals set to RED");
    }
    void AddScore(int points)
    {
        playerScore += points;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateControlRoomScore(playerScore);
        }
    }
    void UpdateUI()
    {
        foreach (var kvp in controlledSignals)
        {
            UpdateSignalUI(kvp.Key, kvp.Value);
        }
        if (trafficManager != null && UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTrafficDensity(trafficManager.roadDensity);
        }
    }
    void UpdateSignalUI(int signalId, TrafficSignal signal)
    {
    }
}
public class SignalButtonUI : MonoBehaviour
{
    public Button button;
    public Image statusLight;
    public Text signalIdText;
    public Text statusText;
    private int signalId;
    private TrafficSignal signal;
    private ControlRoomController controller;
    public void Initialize(int id, TrafficSignal trafficSignal, ControlRoomController ctrl)
    {
        signalId = id;
        signal = trafficSignal;
        controller = ctrl;
        if (signalIdText != null)
        {
            signalIdText.text = $"S-{id:00}";
        }
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    void Update()
    {
        if (signal == null || statusLight == null || statusText == null) return;
        if (signal.emergencyOverride)
        {
            statusLight.color = Color.green;
            statusText.text = "EMERGENCY";
        }
        else if (signal.isYellow)
        {
            statusLight.color = Color.yellow;
            statusText.text = "YELLOW";
        }
        else
        {
            statusLight.color = signal.currentGreenDirection == GetDirection() ?
                Color.green : Color.red;
            statusText.text = signal.currentGreenDirection == GetDirection() ?
                "GREEN" : "RED";
        }
    }
    void OnButtonClick()
    {
        if (controller == null) return;
        string[] states = { "green", "red", "yellow", "emergency" };
        string currentState = GetCurrentState();
        int currentIndex = System.Array.IndexOf(states, currentState);
        string nextState = states[(currentIndex + 1) % states.Length];
        controller.OverrideSignal(signalId, nextState);
    }
    string GetCurrentState()
    {
        if (signal == null) return "red";
        if (signal.emergencyOverride) return "emergency";
        if (signal.isYellow) return "yellow";
        return signal.currentGreenDirection == GetDirection() ? "green" : "red";
    }
    int GetDirection()
    {
        return transform.GetSiblingIndex() % 4;
    }
}
public class SmoothFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -10);
    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
            transform.LookAt(target);
        }
    }
}
