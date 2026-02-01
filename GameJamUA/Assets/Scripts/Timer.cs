using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText; 

    [Header("Timer Settings")]
    [SerializeField] private float maxTime = 180f; 
    private float currentTime;

    private void Awake()
    {
        if (timerText == null)
        {
            Debug.LogError("TimerText is not assigned in the Inspector!");
        }

        currentTime = maxTime;
        UpdateTimerUI();
    }

    private void Update()
    {
        if (currentTime <= 0) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            UpdateTimerUI();
            SceneManager.LoadScene("WonScene"); 
        }
        else
        {
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}";
    }
}

