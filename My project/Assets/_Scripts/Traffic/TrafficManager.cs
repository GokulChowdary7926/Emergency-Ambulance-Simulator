using UnityEngine;
using System.Collections.Generic;
public class TrafficManager : MonoBehaviour
{
    public static TrafficManager Instance;
    [Header("Traffic Settings")]
    public List<TrafficSignal> allSignals = new List<TrafficSignal>();
    public List<GameObject> civilianVehicles = new List<GameObject>();
    public int maxVehicles = 50;
    public float spawnRate = 2f;
    [Header("Emergency System")]
    public bool cityWideEmergency = false;
    public List<TrafficSignal> activeGreenCorridor = new List<TrafficSignal>();
    public float corridorRefreshRate = 0.5f;
    [Header("Traffic Density")]
    public float[] roadDensity = new float[4]; // N,E,S,W densities
    public float updateInterval = 5f;
    private Vector3 lastAmbulancePosition;
    private float lastUpdateTime;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        TrafficSignal[] signals = FindObjectsOfType<TrafficSignal>();
        allSignals.AddRange(signals);
        Debug.Log($"Traffic Manager initialized with {allSignals.Count} signals");
    }
    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateTrafficDensity();
            lastUpdateTime = Time.time;
        }
    }
    public void OnEmergencyActivated(Vector3 ambulancePos, bool activated)
    {
        cityWideEmergency = activated;
        if (activated)
        {
            Debug.Log("CITY-WIDE EMERGENCY ACTIVATED");
            StartCoroutine(CreateGreenCorridor(ambulancePos));
            AlertCivilianVehicles(true);
        }
        else
        {
            Debug.Log("Emergency deactivated - returning to normal traffic");
            ClearGreenCorridor();
            AlertCivilianVehicles(false);
        }
    }
    System.Collections.IEnumerator CreateGreenCorridor(Vector3 startPos)
    {
        List<TrafficSignal> corridorSignals = FindSignalsAlongRoute(startPos);
        foreach (TrafficSignal signal in corridorSignals)
        {
            if (!activeGreenCorridor.Contains(signal))
            {
                activeGreenCorridor.Add(signal);
                signal.emergencyOverride = true;
            }
        }
        while (cityWideEmergency)
        {
            yield return new WaitForSeconds(corridorRefreshRate);
            if (AmbulanceGPS.Instance != null)
            {
                Vector3 currentPos = AmbulanceGPS.Instance.GetPosition();
                UpdateCorridorSignals(currentPos);
            }
        }
    }
    List<TrafficSignal> FindSignalsAlongRoute(Vector3 position)
    {
        List<TrafficSignal> nearbySignals = new List<TrafficSignal>();
        foreach (TrafficSignal signal in allSignals)
        {
            float distance = Vector3.Distance(position, signal.transform.position);
            if (distance < 500f)
            {
                if (AmbulanceGPS.Instance != null)
                {
                    Vector3 toSignal = signal.transform.position - position;
                    float dot = Vector3.Dot(toSignal.normalized,
                        AmbulanceGPS.Instance.transform.forward);
                    if (dot > 0.3f) // Signal is mostly in front
                    {
                        nearbySignals.Add(signal);
                    }
                }
            }
        }
        nearbySignals.Sort((a, b) =>
            Vector3.Distance(position, a.transform.position).CompareTo(
            Vector3.Distance(position, b.transform.position))
        );
        return nearbySignals;
    }
    void UpdateCorridorSignals(Vector3 currentPos)
    {
        for (int i = activeGreenCorridor.Count - 1; i >= 0; i--)
        {
            TrafficSignal signal = activeGreenCorridor[i];
            if (signal == null) continue;
            Vector3 toSignal = signal.transform.position - currentPos;
            if (AmbulanceGPS.Instance != null &&
                Vector3.Dot(toSignal, AmbulanceGPS.Instance.transform.forward) < -0.5f)
            {
                signal.emergencyOverride = false;
                activeGreenCorridor.RemoveAt(i);
            }
        }
        List<TrafficSignal> newSignals = FindSignalsAlongRoute(currentPos);
        foreach (TrafficSignal signal in newSignals)
        {
            if (!activeGreenCorridor.Contains(signal))
            {
                activeGreenCorridor.Add(signal);
                signal.emergencyOverride = true;
            }
        }
    }
    void ClearGreenCorridor()
    {
        foreach (TrafficSignal signal in activeGreenCorridor)
        {
            if (signal != null)
            {
                signal.emergencyOverride = false;
            }
        }
        activeGreenCorridor.Clear();
    }
    void AlertCivilianVehicles(bool emergency)
    {
        foreach (GameObject vehicle in civilianVehicles)
        {
            if (vehicle != null)
            {
                CivilianCarAI ai = vehicle.GetComponent<CivilianCarAI>();
                if (ai != null)
                {
                    ai.OnEmergencyAlert(emergency);
                }
            }
        }
    }
    void UpdateTrafficDensity()
    {
        for (int i = 0; i < roadDensity.Length; i++)
        {
            roadDensity[i] = 0f;
        }
        foreach (GameObject vehicle in civilianVehicles)
        {
            if (vehicle != null)
            {
                Vector3 pos = vehicle.transform.position;
            }
        }
    }
    public void ReceiveAmbulancePosition(Vector3 position, float heading, float speed)
    {
        lastAmbulancePosition = position;
        if (Time.frameCount % 60 == 0) // Every second at 60fps
        {
            Debug.Log($"Ambulance GPS Update: Pos={position}, Heading={heading}°, Speed={speed:F1} m/s");
        }
    }
    void OnDrawGizmos()
    {
        if (!cityWideEmergency) return;
        Gizmos.color = Color.green;
        foreach (TrafficSignal signal in activeGreenCorridor)
        {
            if (signal != null)
            {
                Gizmos.DrawWireSphere(signal.transform.position, 15f);
                int index = activeGreenCorridor.IndexOf(signal);
                if (index < activeGreenCorridor.Count - 1)
                {
                    TrafficSignal next = activeGreenCorridor[index + 1];
                    if (next != null)
                    {
                        Gizmos.DrawLine(signal.transform.position, next.transform.position);
                    }
                }
            }
        }
        if (AmbulanceGPS.Instance != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(AmbulanceGPS.Instance.GetPosition(), 2f);
        }
    }
}
