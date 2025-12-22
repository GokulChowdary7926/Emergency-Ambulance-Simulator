using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Game Settings")]
    public bool isEmergencyActive = false;
    public float gameTime = 0f;
    public int score = 0;
    [Header("References")]
    public GameObject ambulance;
    public GameObject hospital;
    public Text timerText;
    public Text scoreText;
    public GameObject winScreen;
    public GameObject loseScreen;
    [Header("Mission Settings")]
    public float goldenHour = 600f; // 10 minutes in seconds
    public float timeRemaining;
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
        timeRemaining = goldenHour;
    }
    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            gameTime += Time.deltaTime;
            UpdateTimerUI();
            if (ambulance && hospital)
            {
                float distanceToHospital = Vector3.Distance(
                    ambulance.transform.position,
                    hospital.transform.position
                );
                if (distanceToHospital < 10f) // Within hospital grounds
                {
                    WinGame();
                }
            }
            if (timeRemaining <= 0)
            {
                LoseGame("Time's up! Golden Hour expired.");
            }
        }
    }
    void UpdateTimerUI()
    {
        if (timerText)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"TIME: {minutes:00}:{seconds:00}";
        }
    }
    public void AddScore(int points)
    {
        score += points;
        if (scoreText)
        {
            scoreText.text = $"SCORE: {score}";
        }
    }
    void WinGame()
    {
        Time.timeScale = 0f;
        if (winScreen)
        {
            winScreen.SetActive(true);
        }
        int timeBonus = Mathf.FloorToInt(timeRemaining * 10);
        AddScore(timeBonus + 1000);
        Debug.Log("MISSION SUCCESS! Patient saved.");
    }
    public void LoseGame(string reason)
    {
        Time.timeScale = 0f;
        if (loseScreen)
        {
            loseScreen.SetActive(true);
            Text reasonText = loseScreen.transform.Find("ReasonText")?.GetComponent<Text>();
            if (reasonText)
            {
                reasonText.text = reason;
            }
        }
        Debug.LogError($"GAME OVER: {reason}");
    }
    public void ToggleEmergencyMode(bool active)
    {
        isEmergencyActive = active;
        if (active)
        {
            AddScore(500); // Bonus for using emergency mode
            Debug.Log("EMERGENCY MODE ACTIVATED - Green corridor forming");
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
