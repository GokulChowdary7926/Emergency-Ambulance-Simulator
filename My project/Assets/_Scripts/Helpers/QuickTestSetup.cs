using UnityEngine;
public class QuickTestSetup : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Setting up Green Corridor Test Scene...");
        new GameObject("GameManager").AddComponent<GameManager>();
        new GameObject("TrafficManager").AddComponent<TrafficManager>();
        GameObject road = GameObject.CreatePrimitive(PrimitiveType.Cube);
        road.transform.localScale = new Vector3(100, 0.1f, 10);
        road.name = "Road";
        GameObject ambulance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ambulance.name = "Ambulance";
        ambulance.tag = "Ambulance";
        ambulance.AddComponent<Rigidbody>();
        ambulance.AddComponent<AmbulanceController>();
        ambulance.AddComponent<AmbulanceGPS>();
        ambulance.transform.position = new Vector3(-40, 0.5f, 0);
        GameObject signal = new GameObject("TrafficSignal");
        signal.AddComponent<TrafficSignal>();
        signal.transform.position = new Vector3(0, 0, 0);
        GameObject hospital = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hospital.name = "Hospital";
        hospital.tag = "Hospital";
        hospital.transform.position = new Vector3(40, 0.5f, 0);
        hospital.transform.localScale = new Vector3(10, 5, 10);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ambulance = ambulance;
            GameManager.Instance.hospital = hospital;
        }
        Debug.Log("Test scene ready! Press E for emergency mode, WASD to drive.");
    }
}
