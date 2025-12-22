using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class PatientSystem : MonoBehaviour
{
    [System.Serializable]
    public class PatientVitals
    {
        [Header("Vital Signs")]
        [Range(0, 100)] public float consciousness = 100f;
        [Range(0, 200)] public float heartRate = 80f;
        [Range(0, 100)] public float oxygenSaturation = 98f;
        [Range(0, 200)] public float bloodPressureSystolic = 120f;
        [Range(0, 150)] public float bloodPressureDiastolic = 80f;
        [Header("Medical Conditions")]
        public bool isBleeding = false;
        public bool requiresCPR = false;
        public bool requiresOxygen = false;
        public float bloodLossRate = 0f;
        [Header("Treatment Applied")]
        public bool oxygenApplied = false;
        public bool bleedingControlled = false;
        public bool cprInProgress = false;
        public float GetOverallHealth()
        {
            float health = consciousness * 0.3f +
                          (oxygenSaturation / 100f) * 30f +
                          (Mathf.Clamp(200 - heartRate, 0, 100) / 100f) * 20f +
                          (Mathf.Clamp(140 - bloodPressureSystolic, 0, 100) / 100f) * 20f;
            if (isBleeding && !bleedingControlled) health *= 0.7f;
            if (requiresOxygen && !oxygenApplied) health *= 0.8f;
            return Mathf.Clamp(health, 0, 100);
        }
    }
    [Header("Current Patient")]
    public PatientVitals currentPatient = new PatientVitals();
    public string patientName = "John Doe";
    public int patientAge = 45;
    public string emergencyType = "Cardiac Arrest";
    [Header("Treatment Equipment")]
    public bool hasOxygenTank = true;
    public bool hasFirstAidKit = true;
    public bool hasDefibrillator = false;
    [Header("Time Constraints")]
    public float goldenHour = 3600f; // 60 minutes in seconds
    public float timeSinceIncident = 0f;
    public float timeToHospital = 0f;
    [Header("UI References")]
    public Slider healthSlider;
    public Text healthText;
    public Text timerText;
    public Image[] conditionIcons;
    [Header("Audio")]
    public AudioClip heartbeatSound;
    public AudioClip flatlineSound;
    private AudioSource audioSource;
    private float updateInterval = 0.5f;
    private bool isCritical = false;
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        InitializeRandomCondition();
        StartCoroutine(MonitorVitals());
    }
    public void InitializeRandomCondition()
    {
        string[] conditions = {
            "Cardiac Arrest", "Traumatic Injury", "Stroke",
            "Respiratory Failure", "Severe Bleeding", "Diabetic Emergency"
        };
        emergencyType = conditions[Random.Range(0, conditions.Length)];
        switch (emergencyType)
        {
            case "Cardiac Arrest":
                currentPatient.heartRate = Random.Range(0, 40f);
                currentPatient.consciousness = Random.Range(10, 40f);
                currentPatient.requiresCPR = true;
                break;
            case "Severe Bleeding":
                currentPatient.isBleeding = true;
                currentPatient.bloodLossRate = Random.Range(1f, 5f);
                currentPatient.consciousness = Random.Range(30, 70f);
                break;
            case "Respiratory Failure":
                currentPatient.oxygenSaturation = Random.Range(60, 85f);
                currentPatient.requiresOxygen = true;
                break;
        }
        Debug.Log($"Patient: {patientName}, {patientAge}y - {emergencyType}");
    }
    IEnumerator MonitorVitals()
    {
        while (true)
        {
            UpdateVitals();
            UpdateUI();
            CheckCriticalConditions();
            yield return new WaitForSeconds(updateInterval);
        }
    }
    void UpdateVitals()
    {
        float timeFactor = timeSinceIncident / goldenHour;
        currentPatient.consciousness -= timeFactor * 0.5f;
        currentPatient.oxygenSaturation -= timeFactor * 0.3f;
        if (currentPatient.isBleeding && !currentPatient.bleedingControlled)
        {
            currentPatient.consciousness -= currentPatient.bloodLossRate * 0.1f;
            currentPatient.heartRate += 0.2f; // Tachycardia from blood loss
        }
        if (currentPatient.requiresCPR && !currentPatient.cprInProgress)
        {
            currentPatient.heartRate -= 0.5f;
            currentPatient.consciousness -= 0.3f;
        }
        if (currentPatient.requiresOxygen && !currentPatient.oxygenApplied)
        {
            currentPatient.oxygenSaturation -= 0.4f;
        }
        if (currentPatient.oxygenApplied)
        {
            currentPatient.oxygenSaturation = Mathf.Min(
                currentPatient.oxygenSaturation + 1f, 100f);
        }
        if (currentPatient.bleedingControlled)
        {
            currentPatient.bloodLossRate = 0f;
        }
        if (currentPatient.cprInProgress)
        {
            currentPatient.heartRate = Mathf.Min(
                currentPatient.heartRate + 2f, 100f);
        }
        currentPatient.consciousness = Mathf.Clamp(currentPatient.consciousness, 0, 100);
        currentPatient.heartRate = Mathf.Clamp(currentPatient.heartRate, 0, 200);
        currentPatient.oxygenSaturation = Mathf.Clamp(currentPatient.oxygenSaturation, 0, 100);
        timeSinceIncident += updateInterval;
        if (AmbulanceGPS.Instance != null && GameManager.Instance != null && GameManager.Instance.hospital != null)
        {
            float distance = Vector3.Distance(
                AmbulanceGPS.Instance.GetPosition(),
                GameManager.Instance.hospital.transform.position
            );
            float speed = AmbulanceGPS.Instance.GetSpeedKMH();
            timeToHospital = (speed > 1f) ? distance / (speed / 3.6f) : 999f;
        }
    }
    void UpdateUI()
    {
        if (healthSlider != null)
        {
            float health = currentPatient.GetOverallHealth();
            healthSlider.value = health / 100f;
            var fillImage = healthSlider.fillRect?.GetComponent<UnityEngine.UI.Image>();
            if (fillImage != null)
            {
                if (health > 70f) fillImage.color = Color.green;
                else if (health > 30f) fillImage.color = Color.yellow;
                else fillImage.color = Color.red;
            }
        }
        if (healthText != null)
        {
            healthText.text = $"{currentPatient.GetOverallHealth():F0}%";
        }
        if (timerText != null)
        {
            float timeRemaining = goldenHour - timeSinceIncident;
            if (timeRemaining < 0) timeRemaining = 0;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";
            if (timeRemaining > 1200) timerText.color = Color.green; // >20 min
            else if (timeRemaining > 600) timerText.color = Color.yellow; // >10 min
            else timerText.color = Color.red; // <10 min
        }
        if (conditionIcons != null && conditionIcons.Length >= 3)
        {
            if (conditionIcons[0] != null)
            {
                conditionIcons[0].gameObject.SetActive(currentPatient.isBleeding);
                conditionIcons[0].color = currentPatient.bleedingControlled ? Color.green : Color.red;
            }
            if (conditionIcons[1] != null)
            {
                conditionIcons[1].gameObject.SetActive(currentPatient.requiresOxygen);
                conditionIcons[1].color = currentPatient.oxygenApplied ? Color.green : Color.red;
            }
            if (conditionIcons[2] != null)
            {
                conditionIcons[2].gameObject.SetActive(currentPatient.requiresCPR);
                conditionIcons[2].color = currentPatient.cprInProgress ? Color.green : Color.red;
            }
        }
    }
    void CheckCriticalConditions()
    {
        float health = currentPatient.GetOverallHealth();
        if (health < 20f && !isCritical)
        {
            isCritical = true;
            OnPatientCritical();
        }
        else if (health >= 20f && isCritical)
        {
            isCritical = false;
        }
        if (health <= 0f || currentPatient.heartRate <= 0f)
        {
            OnPatientDeath();
        }
    }
    void OnPatientCritical()
    {
        Debug.Log("PATIENT CRITICAL! Immediate intervention required.");
        if (audioSource != null && heartbeatSound != null)
        {
            audioSource.clip = heartbeatSound;
            audioSource.loop = true;
            audioSource.pitch = 1.5f; // Fast heartbeat
            audioSource.Play();
        }
        StartCoroutine(FlashCriticalWarning());
    }
    void OnPatientDeath()
    {
        Debug.Log("PATIENT DECEASED. Mission failed.");
        if (audioSource != null)
        {
            audioSource.Stop();
            if (flatlineSound != null)
            {
                audioSource.PlayOneShot(flatlineSound);
            }
        }
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseGame("Patient deceased");
        }
    }
    IEnumerator FlashCriticalWarning()
    {
        while (isCritical)
        {
            if (healthText != null)
            {
                healthText.color = (healthText.color == Color.red) ? Color.white : Color.red;
            }
            yield return new WaitForSeconds(0.5f);
        }
        if (healthText != null) healthText.color = Color.white;
    }
    public void ApplyOxygen()
    {
        if (hasOxygenTank && currentPatient.requiresOxygen)
        {
            currentPatient.oxygenApplied = true;
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.SpawnMedicalEffect("oxygen_mask", transform.position);
            }
            Debug.Log("Oxygen applied to patient");
        }
    }
    public void ControlBleeding()
    {
        if (hasFirstAidKit && currentPatient.isBleeding)
        {
            currentPatient.bleedingControlled = true;
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.SpawnMedicalEffect("blood_drip", transform.position);
            }
            Debug.Log("Bleeding controlled");
        }
    }
    public void StartCPR()
    {
        if (currentPatient.requiresCPR)
        {
            currentPatient.cprInProgress = true;
            if (ParticleManager.Instance != null)
            {
                ParticleManager.Instance.SpawnMedicalEffect("cpr_compression", transform.position);
            }
            Debug.Log("CPR in progress");
        }
    }
    public void StopCPR()
    {
        currentPatient.cprInProgress = false;
    }
    public float GetEstimatedSurvivalChance()
    {
        float health = currentPatient.GetOverallHealth();
        float timeFactor = 1f - (timeSinceIncident / goldenHour);
        float treatmentFactor = 1f;
        if (currentPatient.oxygenApplied) treatmentFactor += 0.2f;
        if (currentPatient.bleedingControlled) treatmentFactor += 0.3f;
        if (currentPatient.cprInProgress) treatmentFactor += 0.1f;
        return Mathf.Clamp((health / 100f) * timeFactor * treatmentFactor * 100f, 0, 100);
    }
}
