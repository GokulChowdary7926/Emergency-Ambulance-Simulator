using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class HUDController : MonoBehaviour
{
    [Header("Speed & Navigation")]
    public Text speedText;
    public Text gearText;
    public Slider speedSlider;
    public Text distanceToHospital;
    public Text etaText;
    public Image compass;
    [Header("Patient Information")]
    public Slider healthBar;
    public Text healthPercentage;
    public Text patientCondition;
    public Text goldenHourTimer;
    public Image[] treatmentIcons;
    [Header("Emergency Systems")]
    public GameObject emergencyIndicator;
    public Text emergencyStatus;
    public Image sirenIcon;
    public Text signalsCleared;
    [Header("GPS & Map")]
    public RawImage miniMap;
    public Text gpsCoordinates;
    public Text altitudeText;
    public Text headingText;
    [Header("Vehicle Status")]
    public Image fuelGauge;
    public Image engineTemp;
    public Text odometer;
    [Header("Mission Info")]
    public Text missionTitle;
    public Text missionTime;
    public Text scoreText;
    public Text bonusText;
    [Header("Audio")]
    public AudioClip beepSound;
    public AudioClip warningSound;
    private AmbulanceController ambulance;
    private AmbulanceGPS gps;
    private PatientSystem patient;
    private float flashTimer = 0f;
    private bool flashState = false;
    void Start()
    {
        ambulance = FindObjectOfType<AmbulanceController>();
        gps = FindObjectOfType<AmbulanceGPS>();
        patient = FindObjectOfType<PatientSystem>();
        StartCoroutine(UpdateHUD());
    }
    IEnumerator UpdateHUD()
    {
        while (true)
        {
            if (ambulance != null) UpdateVehicleHUD();
            if (gps != null) UpdateNavigationHUD();
            if (patient != null) UpdateMedicalHUD();
            if (TrafficManager.Instance != null) UpdateTrafficHUD();
            UpdateEmergencyHUD();
            UpdateMissionHUD();
            yield return new WaitForSeconds(0.1f); // Update 10 times per second
        }
    }
    void UpdateVehicleHUD()
    {
        if (ambulance == null) return;
        float speedKMH = Mathf.Abs(ambulance.currentSpeed) * 3.6f;
        if (speedText != null) speedText.text = $"{speedKMH:0}";
        if (speedSlider != null) speedSlider.value = speedKMH / 120f; // Assuming 120 max
        if (gearText != null)
        {
            if (ambulance.currentSpeed > 1f) gearText.text = "D";
            else if (ambulance.currentSpeed < -1f) gearText.text = "R";
            else gearText.text = "N";
        }
        if (compass != null)
        {
            float heading = ambulance.transform.eulerAngles.y;
            compass.transform.rotation = Quaternion.Euler(0, 0, -heading);
        }
    }
    void UpdateNavigationHUD()
    {
        if (gps == null) return;
        if (gpsCoordinates != null) gpsCoordinates.text = gps.GetFormattedCoordinates();
        if (altitudeText != null) altitudeText.text = $"{gps.altitude:0}m";
        if (headingText != null)
        {
            float heading = gps.heading;
            string direction = GetDirectionFromHeading(heading);
            headingText.text = $"{direction} ({heading:000}°)";
        }
        if (ambulance != null && GameManager.Instance != null && GameManager.Instance.hospital != null && distanceToHospital != null)
        {
            float distance = Vector3.Distance(
                ambulance.transform.position,
                GameManager.Instance.hospital.transform.position
            );
            distanceToHospital.text = $"{distance:0}m";
            if (etaText != null)
            {
                float speedKMH = Mathf.Abs(ambulance.currentSpeed) * 3.6f;
                if (speedKMH > 0.1f)
                {
                    float timeMinutes = (distance / 1000f) / (speedKMH / 60f);
                    int minutes = Mathf.FloorToInt(timeMinutes);
                    int seconds = Mathf.FloorToInt((timeMinutes - minutes) * 60);
                    etaText.text = $"ETA: {minutes}:{seconds:00}";
                }
                else
                {
                    etaText.text = "ETA: --:--";
                }
            }
        }
    }
    void UpdateMedicalHUD()
    {
        if (patient == null) return;
        if (healthBar != null) healthBar.value = patient.currentPatient.GetOverallHealth() / 100f;
        if (healthPercentage != null) healthPercentage.text = $"{patient.currentPatient.GetOverallHealth():F0}%";
        if (patientCondition != null) patientCondition.text = patient.emergencyType;
        if (goldenHourTimer != null)
        {
            float timeRemaining = patient.goldenHour - patient.timeSinceIncident;
            if (timeRemaining < 0) timeRemaining = 0;
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            goldenHourTimer.text = $"{minutes:00}:{seconds:00}";
            if (timeRemaining > 1200) goldenHourTimer.color = Color.green;
            else if (timeRemaining > 600) goldenHourTimer.color = Color.yellow;
            else goldenHourTimer.color = Color.red;
        }
        if (treatmentIcons != null && treatmentIcons.Length >= 3)
        {
            if (treatmentIcons[0] != null)
            {
                treatmentIcons[0].gameObject.SetActive(patient.currentPatient.requiresOxygen);
                treatmentIcons[0].color = patient.currentPatient.oxygenApplied ? Color.green : Color.red;
            }
            if (treatmentIcons[1] != null)
            {
                treatmentIcons[1].gameObject.SetActive(patient.currentPatient.isBleeding);
                treatmentIcons[1].color = patient.currentPatient.bleedingControlled ? Color.green : Color.red;
            }
            if (treatmentIcons[2] != null)
            {
                treatmentIcons[2].gameObject.SetActive(patient.currentPatient.requiresCPR);
                treatmentIcons[2].color = patient.currentPatient.cprInProgress ? Color.green : Color.red;
            }
        }
    }
    void UpdateTrafficHUD()
    {
        if (signalsCleared != null && TrafficManager.Instance != null)
        {
            signalsCleared.text = $"{TrafficManager.Instance.activeGreenCorridor.Count}";
        }
    }
    void UpdateEmergencyHUD()
    {
        if (ambulance == null) return;
        bool emergencyActive = ambulance.emergencyMode;
        if (emergencyIndicator != null) emergencyIndicator.SetActive(emergencyActive);
        if (emergencyStatus != null) emergencyStatus.text = emergencyActive ? "EMERGENCY ACTIVE" : "STAND BY";
        if (sirenIcon != null)
        {
            flashTimer += Time.deltaTime;
            if (flashTimer > 0.25f)
            {
                flashState = !flashState;
                flashTimer = 0f;
                sirenIcon.enabled = emergencyActive && flashState;
            }
            sirenIcon.color = emergencyActive ? Color.red : Color.gray;
        }
    }
    void UpdateMissionHUD()
    {
        if (GameManager.Instance == null) return;
        if (missionTime != null)
        {
            float time = GameManager.Instance.timeRemaining;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            missionTime.text = $"{minutes:00}:{seconds:00}";
        }
        if (scoreText != null) scoreText.text = $"{GameManager.Instance.score}";
        if (bonusText != null)
        {
            float timeBonus = Mathf.Max(0, GameManager.Instance.timeRemaining) * 10;
            int signalBonus = TrafficManager.Instance != null ?
                TrafficManager.Instance.activeGreenCorridor.Count * 50 : 0;
            int totalBonus = Mathf.RoundToInt(timeBonus + signalBonus);
            bonusText.text = $"+{totalBonus}";
        }
    }
    string GetDirectionFromHeading(float heading)
    {
        if (heading >= 337.5f || heading < 22.5f) return "N";
        if (heading >= 22.5f && heading < 67.5f) return "NE";
        if (heading >= 67.5f && heading < 112.5f) return "E";
        if (heading >= 112.5f && heading < 157.5f) return "SE";
        if (heading >= 157.5f && heading < 202.5f) return "S";
        if (heading >= 202.5f && heading < 247.5f) return "SW";
        if (heading >= 247.5f && heading < 292.5f) return "W";
        return "NW";
    }
    public void ShowNotification(string message, Color color, float duration = 3f)
    {
        StartCoroutine(DisplayNotification(message, color, duration));
    }
    IEnumerator DisplayNotification(string message, Color color, float duration)
    {
        GameObject notification = new GameObject("Notification");
        notification.transform.SetParent(transform);
        Text text = notification.AddComponent<Text>();
        text.text = message;
        text.color = color;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 24;
        text.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = notification.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 50);
        rt.anchorMin = new Vector2(0.5f, 0.8f);
        rt.anchorMax = new Vector2(0.5f, 0.8f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = 1f - (timer / duration);
            text.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        Destroy(notification);
    }
    public void OnSignalCleared()
    {
        ShowNotification("Signal Cleared! +50 Points", Color.green);
    }
    public void OnEmergencyActivated()
    {
        ShowNotification("EMERGENCY MODE ACTIVATED", Color.red, 5f);
    }
}
