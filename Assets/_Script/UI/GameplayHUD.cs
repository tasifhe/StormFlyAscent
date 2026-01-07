using UnityEngine;
using TMPro;

/// <summary>
/// Manages the in-game HUD display including score, combo, and stats
/// </summary>
public class GameplayHUD : MonoBehaviour
{
    [Header("Score Display")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private string scorePrefix = "Score: ";
    
    [Header("Combo Display")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private string comboPrefix = "Combo x";
    [SerializeField] private float comboDisplayDuration = 2f;
    
    [Header("Stats Display")]
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private bool showStats = false;
    
    [Header("Animation")]
    [SerializeField] private bool animateScoreChange = true;
    [SerializeField] private float scorePunchScale = 1.2f;
    [SerializeField] private float punchDuration = 0.2f;
    
    private int currentDisplayedScore = 0;
    private float comboHideTimer = 0f;
    private Vector3 originalScoreScale;
    private Coroutine scorePunchCoroutine;
    
    private void Start()
    {
        if (scoreText != null)
        {
            originalScoreScale = scoreText.transform.localScale;
        }
        
        // Subscribe to ObstacleManager events
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnScoreChanged += UpdateScore;
            ObstacleManager.Instance.OnObstacleInteraction += OnObstacleInteraction;
        }
        else
        {
            Debug.LogWarning("[GameplayHUD] ObstacleManager.Instance is null at Start. Will retry in Update.");
        }
        
        // Initialize display
        UpdateScore(0);
        UpdateComboDisplay();
        
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        // Subscribe when enabled
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnScoreChanged += UpdateScore;
            ObstacleManager.Instance.OnObstacleInteraction += OnObstacleInteraction;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe when disabled
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnScoreChanged -= UpdateScore;
            ObstacleManager.Instance.OnObstacleInteraction -= OnObstacleInteraction;
        }
    }
    
    private void Update()
    {
        // Retry subscription if ObstacleManager wasn't ready at Start
        if (ObstacleManager.Instance != null && ObstacleManager.Instance.OnScoreChanged != null)
        {
            // Check if we're not already subscribed by testing if our method is in the invocation list
            var delegates = ObstacleManager.Instance.OnScoreChanged.GetInvocationList();
            bool isSubscribed = false;
            foreach (var d in delegates)
            {
                if (d.Method.Name == "UpdateScore" && d.Target == this)
                {
                    isSubscribed = true;
                    break;
                }
            }
            
            if (!isSubscribed)
            {
                ObstacleManager.Instance.OnScoreChanged += UpdateScore;
                ObstacleManager.Instance.OnObstacleInteraction += OnObstacleInteraction;
            }
        }
        
        // Handle combo panel auto-hide
        if (comboPanel != null && comboPanel.activeSelf)
        {
            comboHideTimer -= Time.deltaTime;
            if (comboHideTimer <= 0f)
            {
                comboPanel.SetActive(false);
            }
        }
        
        // Update stats if enabled
        if (showStats && statsText != null)
        {
            UpdateStatsDisplay();
        }
    }
    
    /// <summary>
    /// Update score display
    /// </summary>
    private void UpdateScore(int newScore)
    {
        currentDisplayedScore = newScore;
        
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + newScore.ToString();
            
            if (animateScoreChange && newScore > 0)
            {
                AnimateScoreChange();
            }
        }
    }
    
    /// <summary>
    /// Handle obstacle interaction events
    /// </summary>
    private void OnObstacleInteraction(ObstacleBase obstacle, bool success)
    {
        if (success)
        {
            UpdateComboDisplay();
        }
    }
    
    /// <summary>
    /// Update combo multiplier display
    /// </summary>
    private void UpdateComboDisplay()
    {
        if (ObstacleManager.Instance == null) return;
        
        float comboMultiplier = ObstacleManager.Instance.GetComboMultiplier();
        
        if (comboText != null)
        {
            if (comboMultiplier > 1f)
            {
                comboText.text = comboPrefix + comboMultiplier.ToString("F1");
                
                if (comboPanel != null)
                {
                    comboPanel.SetActive(true);
                    comboHideTimer = comboDisplayDuration;
                }
            }
            else
            {
                if (comboPanel != null)
                {
                    comboPanel.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Update stats display (optional debug info)
    /// </summary>
    private void UpdateStatsDisplay()
    {
        if (ObstacleManager.Instance == null || statsText == null) return;
        
        var stats = ObstacleManager.Instance.GetStats();
        statsText.text = $"Total: {stats.total} | Success: {stats.success} | Failed: {stats.failed}";
    }
    
    /// <summary>
    /// Animate score text when it changes
    /// </summary>
    private void AnimateScoreChange()
    {
        if (scoreText == null) return;
        
        if (scorePunchCoroutine != null)
        {
            StopCoroutine(scorePunchCoroutine);
        }
        
        scorePunchCoroutine = StartCoroutine(ScorePunchAnimation());
    }
    
    /// <summary>
    /// Score punch animation coroutine
    /// </summary>
    private System.Collections.IEnumerator ScorePunchAnimation()
    {
        float elapsed = 0f;
        
        // Scale up
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (punchDuration / 2f);
            scoreText.transform.localScale = Vector3.Lerp(originalScoreScale, originalScoreScale * scorePunchScale, progress);
            yield return null;
        }
        
        elapsed = 0f;
        
        // Scale back down
        while (elapsed < punchDuration / 2f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (punchDuration / 2f);
            scoreText.transform.localScale = Vector3.Lerp(originalScoreScale * scorePunchScale, originalScoreScale, progress);
            yield return null;
        }
        
        scoreText.transform.localScale = originalScoreScale;
    }
    
    /// <summary>
    /// Public method to reset the HUD
    /// </summary>
    public void ResetHUD()
    {
        UpdateScore(0);
        UpdateComboDisplay();
        
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
    }
}
