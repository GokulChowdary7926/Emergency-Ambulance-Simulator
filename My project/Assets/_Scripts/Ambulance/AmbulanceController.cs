using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Rigidbody))]
public class AmbulanceController : MonoBehaviour
{
    [Header("Vehicle Settings")]
    public float maxSpeed = 120f;
    public float acceleration = 15f;
    public float brakeForce = 30f;
    public float turnSpeed = 2f;
    public float currentSpeed = 0f;
    [Header("Emergency Systems")]
    public bool emergencyMode = false;
    public Light[] emergencyLights;
    public AudioSource sirenAudio;
    public AudioSource engineAudio;
    [Header("Visual Effects")]
    public ParticleSystem tireSmoke;
    public ParticleSystem exhaust;
    public TrailRenderer[] skidMarks;
    [Header("GPS & Navigation")]
    public LineRenderer routeLine;
    public Transform hospitalTarget;
    private Rigidbody rb;
    [Header("UI References")]
    public Text speedDisplay;
    public Image emergencyIndicator;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0); // Lower center for stability
        if (engineAudio) engineAudio.Play();
        DrawRouteToHospital();
    }
    void Update()
    {
        HandleInput();
        UpdateSpeed();
        UpdateAudio();
        UpdateEmergencyLights();
        UpdateUI();
        UpdateParticleEffects();
        if (routeLine && hospitalTarget)
        {
            UpdateRoute();
        }
    }
    void UpdateParticleEffects()
    {
        if (ParticleManager.Instance != null)
        {
            bool isBraking = Input.GetKey(KeyCode.Space) || Input.GetAxis("Vertical") < -0.3f;
            bool isTurning = Mathf.Abs(Input.GetAxis("Horizontal")) > 0.5f;
            if ((isBraking || isTurning) && currentSpeed > 10f)
            {
                WheelCollider[] wheels = GetComponentsInChildren<WheelCollider>();
                foreach (WheelCollider wheel in wheels)
                {
                    if (wheel != null)
                    {
                        ParticleManager.Instance.SpawnTireSmoke(wheel, 0.5f);
                    }
                }
            }
            if (currentSpeed > 5f)
            {
                Vector3 exhaustPos = transform.position - transform.forward * 2f;
                ParticleManager.Instance.SpawnExhaust(exhaustPos, -transform.forward,
                    Mathf.Abs(Input.GetAxis("Vertical")));
            }
            if (emergencyMode)
            {
                ParticleManager.Instance.SpawnSirenLights(transform.position + Vector3.up * 2f, true);
            }
            else
            {
                ParticleManager.Instance.SpawnSirenLights(transform.position, false);
            }
        }
        if (SoundManager.Instance != null)
        {
            float rpm = Mathf.Abs(currentSpeed) * 100f;
            float acceleration = Input.GetAxis("Vertical");
            SoundManager.Instance.PlayEngineSound(rpm, acceleration);
            if (Input.GetKeyDown(KeyCode.E))
            {
                SoundManager.Instance.PlaySiren(emergencyMode);
            }
        }
    }
    void HandleInput()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        if (vertical > 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
        else if (vertical < 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, -maxSpeed/2, brakeForce * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, brakeForce * 0.5f * Time.deltaTime);
        }
        if (currentSpeed > 5f || currentSpeed < -2f)
        {
            float turn = horizontal * turnSpeed * (currentSpeed / maxSpeed);
            transform.Rotate(0, turn, 0);
        }
        rb.velocity = transform.forward * currentSpeed;
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleEmergencyMode();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (sirenAudio) sirenAudio.PlayOneShot(sirenAudio.clip);
        }
    }
    void ToggleEmergencyMode()
    {
        emergencyMode = !emergencyMode;
        foreach (Light light in emergencyLights)
        {
            if (light != null)
                light.enabled = emergencyMode;
        }
        if (emergencyMode)
        {
            if (sirenAudio) sirenAudio.Play();
        }
        else
        {
            if (sirenAudio) sirenAudio.Stop();
        }
        if (GameManager.Instance)
        {
            GameManager.Instance.ToggleEmergencyMode(emergencyMode);
        }
        if (TrafficManager.Instance != null)
        {
            TrafficManager.Instance.OnEmergencyActivated(transform.position, emergencyMode);
        }
    }
    void UpdateSpeed()
    {
        float speedKMH = Mathf.Abs(currentSpeed) * 3.6f;
        bool isBraking = Input.GetKey(KeyCode.Space) || Input.GetAxis("Vertical") < -0.1f;
        foreach (var skid in skidMarks)
        {
            if (skid != null)
                skid.emitting = isBraking && speedKMH > 10f;
        }
        if (tireSmoke)
        {
            var emission = tireSmoke.emission;
            emission.rateOverTime = isBraking ? 30f : 0f;
        }
    }
    void UpdateAudio()
    {
        if (engineAudio)
        {
            float speedRatio = Mathf.Abs(currentSpeed) / maxSpeed;
            engineAudio.pitch = 0.7f + speedRatio * 0.5f;
            engineAudio.volume = 0.3f + speedRatio * 0.3f;
        }
    }
    void UpdateEmergencyLights()
    {
        if (emergencyMode)
        {
            float flashSpeed = 2f;
            bool lightsOn = Mathf.Sin(Time.time * flashSpeed * Mathf.PI) > 0;
            foreach (Light light in emergencyLights)
            {
                if (light != null)
                    light.intensity = lightsOn ? 5f : 0f;
            }
        }
    }
    void UpdateUI()
    {
        if (speedDisplay)
        {
            float speedKMH = Mathf.Abs(currentSpeed) * 3.6f;
            speedDisplay.text = $"{speedKMH:0} km/h";
            speedDisplay.color = emergencyMode ? Color.red : Color.white;
        }
        if (emergencyIndicator)
        {
            emergencyIndicator.color = emergencyMode ? Color.red : Color.gray;
            emergencyIndicator.transform.localScale = emergencyMode ?
                Vector3.one * 1.2f : Vector3.one;
        }
    }
    void DrawRouteToHospital()
    {
        if (!routeLine || !hospitalTarget) return;
        routeLine.positionCount = 2;
        routeLine.SetPosition(0, transform.position);
        routeLine.SetPosition(1, hospitalTarget.position);
        routeLine.startColor = emergencyMode ? Color.green : Color.yellow;
        routeLine.endColor = emergencyMode ? Color.green : Color.yellow;
    }
    void UpdateRoute()
    {
        routeLine.SetPosition(0, transform.position);
        Vector3 direction = (hospitalTarget.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, hospitalTarget.position);
        if (distance > 50f)
        {
            routeLine.positionCount = 3;
            Vector3 midPoint = transform.position + direction * distance * 0.5f;
            midPoint.y += 10f; // Arc effect
            routeLine.SetPosition(1, midPoint);
            routeLine.SetPosition(2, hospitalTarget.position);
        }
        else
        {
            routeLine.positionCount = 2;
            routeLine.SetPosition(1, hospitalTarget.position);
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CivilianCar"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(-200);
            }
            if (SoundManager.Instance != null)
            {
                float impactForce = collision.relativeVelocity.magnitude;
                SoundManager.Instance.PlayCrashSound(impactForce, collision.contacts[0].point);
            }
            if (ParticleManager.Instance != null && collision.contacts.Length > 0)
            {
                ParticleManager.Instance.SpawnCrashEffect(
                    collision.contacts[0].point,
                    collision.contacts[0].normal,
                    collision.relativeVelocity.magnitude
                );
            }
            Debug.LogWarning("Collision with civilian vehicle! -200 points");
        }
    }
}
