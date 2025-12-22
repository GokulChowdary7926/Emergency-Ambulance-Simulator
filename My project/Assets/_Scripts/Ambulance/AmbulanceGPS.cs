using UnityEngine;
using System.Collections.Generic;
public class AmbulanceGPS : MonoBehaviour
{
    public static AmbulanceGPS Instance;
    [Header("GPS Settings")]
    public Vector3 currentPosition;
    public Vector3 currentVelocity;
    public float altitude;
    public float heading; // 0-360 degrees
    [Header("Tracking History")]
    public List<Vector3> positionHistory = new List<Vector3>();
    public int maxHistoryPoints = 100;
    [Header("Transmission")]
    public float updateFrequency = 1f; // Updates per second
    private float lastUpdateTime = 0f;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Update()
    {
        UpdateGPSData();
        if (Time.time - lastUpdateTime >= 1f / updateFrequency)
        {
            TransmitPosition();
            lastUpdateTime = Time.time;
        }
    }
    void UpdateGPSData()
    {
        currentPosition = transform.position;
        altitude = currentPosition.y;
        if (positionHistory.Count > 0)
        {
            Vector3 lastPos = positionHistory[positionHistory.Count - 1];
            currentVelocity = (currentPosition - lastPos) / Time.deltaTime;
        }
        heading = (transform.eulerAngles.y + 360) % 360;
        positionHistory.Add(currentPosition);
        if (positionHistory.Count > maxHistoryPoints)
        {
            positionHistory.RemoveAt(0);
        }
    }
    void TransmitPosition()
    {
        if (TrafficManager.Instance != null)
        {
            TrafficManager.Instance.ReceiveAmbulancePosition(
                currentPosition,
                heading,
                currentVelocity.magnitude
            );
        }
    }
    public Vector3 GetPosition()
    {
        return currentPosition;
    }
    public float GetSpeedKMH()
    {
        return currentVelocity.magnitude * 3.6f;
    }
    public string GetFormattedCoordinates()
    {
        float lat = 13.0827f + (currentPosition.x / 111000f);
        float lon = 80.2707f + (currentPosition.z / 111000f);
        return $"Lat: {lat:F6}, Lon: {lon:F6}";
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        for (int i = 1; i < positionHistory.Count; i++)
        {
            Gizmos.DrawLine(positionHistory[i-1], positionHistory[i]);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentPosition, 0.5f);
        Gizmos.DrawRay(currentPosition, transform.forward * 3f);
    }
}
