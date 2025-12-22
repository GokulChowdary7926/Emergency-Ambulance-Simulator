using UnityEngine;
using System.Collections;
public class TrafficSignal : MonoBehaviour
{
    [System.Serializable]
    public class SignalLight
    {
        public GameObject lightObject;
        public Material redMaterial;
        public Material yellowMaterial;
        public Material greenMaterial;
        public Light pointLight;
    }
    [Header("Signal Configuration")]
    public SignalLight[] signalLights; // 0=North, 1=East, 2=South, 3=West
    public float[] greenTimes = { 20f, 20f, 20f, 20f };
    public float yellowTime = 3f;
    public float allRedTime = 1f;
    [Header("Emergency Preemption")]
    public float preemptionRadius = 250f;
    public bool emergencyOverride = false;
    public int emergencyDirection = 0; // Which direction gets green
    [Header("Current State")]
    public int currentGreenDirection = 0;
    public float timer = 0f;
    public bool isYellow = false;
    [Header("Visuals")]
    public GameObject signalPole;
    public AudioClip changeSound;
    void Start()
    {
        StartCoroutine(TrafficCycle());
    }
    IEnumerator TrafficCycle()
    {
        while (true)
        {
            if (CheckForAmbulance())
            {
                yield return StartCoroutine(EmergencyMode());
                continue;
            }
            SetSignal(currentGreenDirection, "green");
            timer = greenTimes[currentGreenDirection];
            while (timer > 0 && !emergencyOverride)
            {
                timer -= Time.deltaTime;
                yield return null;
            }
            if (emergencyOverride) continue;
            SetSignal(currentGreenDirection, "yellow");
            isYellow = true;
            yield return new WaitForSeconds(yellowTime);
            isYellow = false;
            SetAllSignals("red");
            yield return new WaitForSeconds(allRedTime);
            currentGreenDirection = (currentGreenDirection + 1) % 4;
        }
    }
    bool CheckForAmbulance()
    {
        if (AmbulanceGPS.Instance == null) return false;
        Vector3 ambulancePos = AmbulanceGPS.Instance.GetPosition();
        float distance = Vector3.Distance(transform.position, ambulancePos);
        if (distance <= preemptionRadius)
        {
            Vector3 direction = ambulancePos - transform.position;
            float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
            emergencyDirection = Mathf.RoundToInt((angle + 45f) / 90f) % 4;
            if (emergencyDirection < 0) emergencyDirection += 4;
            return true;
        }
        return false;
    }
    IEnumerator EmergencyMode()
    {
        emergencyOverride = true;
        if (changeSound)
        {
            AudioSource.PlayClipAtPoint(changeSound, transform.position);
        }
        Debug.Log($"EMERGENCY PREEMPTION: Signal {name} turning green for direction {emergencyDirection}");
        SetAllSignals("red");
        yield return new WaitForSeconds(0.5f);
        SetSignal(emergencyDirection, "green");
        while (CheckForAmbulance())
        {
            bool lightOn = Mathf.Sin(Time.time * 4f) > 0;
            SetSignalMaterial(emergencyDirection, lightOn ? "green" : "off");
            yield return new WaitForSeconds(0.1f);
        }
        SetAllSignals("red");
        yield return new WaitForSeconds(1f);
        emergencyOverride = false;
        SetSignal(currentGreenDirection, "green");
    }
    void SetSignal(int direction, string state)
    {
        if (direction < 0 || direction >= signalLights.Length) return;
        SignalLight light = signalLights[direction];
        switch (state)
        {
            case "red":
                SetSignalMaterial(direction, "red");
                break;
            case "yellow":
                SetSignalMaterial(direction, "yellow");
                break;
            case "green":
                SetSignalMaterial(direction, "green");
                break;
            case "off":
                if (light.lightObject)
                {
                    Renderer renderer = light.lightObject.GetComponent<Renderer>();
                    if (renderer)
                    {
                        renderer.material = null;
                    }
                }
                if (light.pointLight)
                {
                    light.pointLight.enabled = false;
                }
                break;
        }
    }
    void SetSignalMaterial(int direction, string color)
    {
        SignalLight light = signalLights[direction];
        Material mat = null;
        switch (color)
        {
            case "red": mat = light.redMaterial; break;
            case "yellow": mat = light.yellowMaterial; break;
            case "green": mat = light.greenMaterial; break;
        }
        if (light.lightObject && mat)
        {
            Renderer renderer = light.lightObject.GetComponent<Renderer>();
            if (renderer)
            {
                renderer.material = mat;
            }
        }
        if (light.pointLight)
        {
            light.pointLight.enabled = (mat != null);
            light.pointLight.color = GetColorFromMaterial(mat);
        }
    }
    void SetAllSignals(string state)
    {
        for (int i = 0; i < signalLights.Length; i++)
        {
            SetSignal(i, state);
        }
    }
    Color GetColorFromMaterial(Material mat)
    {
        if (mat == null) return Color.white;
        if (mat.name.Contains("Red")) return Color.red;
        if (mat.name.Contains("Yellow")) return Color.yellow;
        if (mat.name.Contains("Green")) return Color.green;
        return Color.white;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = emergencyOverride ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, preemptionRadius);
        Gizmos.color = Color.blue;
        float arrowLength = 10f;
        Vector3[] directions = {
            Vector3.forward * arrowLength,
            Vector3.right * arrowLength,
            Vector3.back * arrowLength,
            Vector3.left * arrowLength
        };
        for (int i = 0; i < directions.Length; i++)
        {
            Gizmos.DrawRay(transform.position + Vector3.up * 2, directions[i]);
            if (i == currentGreenDirection)
            {
                Gizmos.color = isYellow ? Color.yellow : Color.green;
                Gizmos.DrawSphere(transform.position + Vector3.up * 2 + directions[i], 0.5f);
                Gizmos.color = Color.blue;
            }
        }
    }
}
