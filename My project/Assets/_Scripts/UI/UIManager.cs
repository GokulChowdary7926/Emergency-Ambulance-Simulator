using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [Header("UI Panels")]
    public GameObject briefingPanel;
    public GameObject levelCompletePanel;
    public GameObject pauseMenu;
    [Header("Text References")]
    public Text briefingText;
    public Text subtitleText;
    public Text levelCompleteTitle;
    public Text levelCompleteScore;
    public Text controlRoomScore;
    [Header("Traffic Display")]
    public Text[] trafficDensityTexts;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ShowBriefing(string briefing)
    {
        if (briefingPanel != null)
        {
            briefingPanel.SetActive(true);
            if (briefingText != null)
            {
                briefingText.text = briefing;
            }
            StartCoroutine(HideBriefingAfterDelay(5f));
        }
    }
    IEnumerator HideBriefingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (briefingPanel != null)
        {
            briefingPanel.SetActive(false);
        }
    }
    public void ShowSubtitle(string text, float duration)
    {
        if (subtitleText != null)
        {
            subtitleText.text = text;
            subtitleText.gameObject.SetActive(true);
            StartCoroutine(HideSubtitleAfterDelay(duration));
        }
    }
    IEnumerator HideSubtitleAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (subtitleText != null)
        {
            subtitleText.gameObject.SetActive(false);
        }
    }
    public void ShowLevelComplete(string levelName, int score, float time)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            if (levelCompleteTitle != null)
            {
                levelCompleteTitle.text = $"{levelName} Complete!";
            }
            if (levelCompleteScore != null)
            {
                levelCompleteScore.text = $"Score: {score}\nTime: {time:F1}s";
            }
        }
    }
    public void UpdateControlRoomScore(int score)
    {
        if (controlRoomScore != null)
        {
            controlRoomScore.text = $"Score: {score}";
        }
    }
    public void UpdateTrafficDensity(float[] densities)
    {
        if (trafficDensityTexts != null && densities != null)
        {
            for (int i = 0; i < trafficDensityTexts.Length && i < densities.Length; i++)
            {
                if (trafficDensityTexts[i] != null)
                {
                    trafficDensityTexts[i].text = $"Density: {densities[i]:F1}";
                }
            }
        }
    }
}
