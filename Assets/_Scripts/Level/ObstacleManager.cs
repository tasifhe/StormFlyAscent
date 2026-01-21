using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton manager for tracking obstacle interactions and scoring
/// </summary>
public class ObstacleManager : MonoBehaviour
{
    public static ObstacleManager Instance { get; private set; }

    [Header("Stats")]
    [SerializeField] private int totalObstaclesPassed = 0;
    [SerializeField] private int successfulPasses = 0;
    [SerializeField] private int failedPasses = 0;
    [SerializeField] private int currentScore = 0;

    [Header("Events")]
    public System.Action<int> OnScoreChanged;
    public System.Action<ObstacleBase, bool> OnObstacleInteraction;
    public System.Action OnGameOver;

    [Header("Combo System")]
    [Tooltip("Enable combo multiplier for consecutive successes")]
    public bool useComboSystem = true;

    [Tooltip("Consecutive successes before combo starts")]
    public int comboThreshold = 3;

    [Tooltip("Maximum combo multiplier")]
    public float maxComboMultiplier = 5f;

    [Header("Game Over System")]
    [Tooltip("Maximum allowed failures before game over")]
    public int maxFailures = 3;

    [Tooltip("Enable game over system")]
    public bool useGameOverSystem = true;

    private int consecutiveSuccesses = 0;
    private float currentComboMultiplier = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log("[ObstacleManager] Instance created successfully");
    }

    /// <summary>
    /// Ensure ObstacleManager exists even if not in scene
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureInstance()
    {
        if (Instance == null)
        {
            GameObject managerObj = new GameObject("ObstacleManager (Auto-Created)");
            managerObj.AddComponent<ObstacleManager>();
            Debug.LogWarning("[ObstacleManager] Auto-created instance - consider adding ObstacleManager to your scene manually for better control");
        }
    }

    private void Start()
    {
        Debug.Log("[ObstacleManager] ✓ Initialized and ready!");
    }

    /// <summary>
    /// Called when player passes through an obstacle
    /// </summary>
    public void OnObstaclePassed(ObstacleBase obstacle, bool success)
    {
        totalObstaclesPassed++;

        if (success)
        {
            HandleSuccess(obstacle);
        }
        else
        {
            HandleFailure(obstacle);
        }

        // Invoke event
        OnObstacleInteraction?.Invoke(obstacle, success);
    }

    private void HandleSuccess(ObstacleBase obstacle)
    {
        successfulPasses++;
        consecutiveSuccesses++;

        // Update combo multiplier
        if (useComboSystem && consecutiveSuccesses >= comboThreshold)
        {
            currentComboMultiplier = Mathf.Min(
                1f + (consecutiveSuccesses - comboThreshold) * 0.5f,
                maxComboMultiplier
            );
        }

        // Calculate score with multiplier
        int basePoints = obstacle.pointValue;
        int earnedPoints = Mathf.RoundToInt(basePoints * currentComboMultiplier);

        AddScore(earnedPoints);

        Debug.Log($"✓ Obstacle passed! +{earnedPoints} points (x{currentComboMultiplier:F1} combo)");
    }

    private void HandleFailure(ObstacleBase obstacle)
    {
        failedPasses++;
        consecutiveSuccesses = 0;
        currentComboMultiplier = 1f;

        Debug.Log($"✗ Obstacle missed! ({failedPasses}/{maxFailures} failures)");

        // Check for game over
        if (useGameOverSystem && failedPasses >= maxFailures)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        Debug.Log("[ObstacleManager] GAME OVER!");
        OnGameOver?.Invoke();

        // Pause the game
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Public method to restart the game
    /// </summary>
    public void RestartGame()
    {
        // Reset time scale first
        Time.timeScale = 1f;

        // Reset stats before reloading
        ResetStats();

        // Use MenuSceneManager for reloading if available, otherwise fallback
        MenuSceneManager menuManager = MenuSceneManager.Instance;
        if (menuManager != null)
        {
            menuManager.ReloadCurrentScene();
        }
        else
        {
            Debug.LogWarning("[ObstacleManager] MenuSceneManager not found, using direct SceneManager");
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        // Note: Countdown will automatically trigger when the scene reloads via GameStartManager
    }

    /// <summary>
    /// Public method to go to main menu
    /// </summary>
    public void LoadMainMenu(string mainMenuSceneName = "Main Menu")
    {
        Time.timeScale = 1f;

        // Use MenuSceneManager for loading main menu if available, otherwise fallback
        MenuSceneManager menuManager = MenuSceneManager.Instance;
        if (menuManager != null)
        {
            menuManager.LoadMainMenuScene();
        }
        else
        {
            Debug.LogWarning("[ObstacleManager] MenuSceneManager not found, using direct SceneManager");
            // Basic fallback using string parameter
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    private void AddScore(int points)
    {
        currentScore += points;
        OnScoreChanged?.Invoke(currentScore);
    }

    /// <summary>
    /// Get current stats
    /// </summary>
    public (int total, int success, int failed, int score) GetStats()
    {
        return (totalObstaclesPassed, successfulPasses, failedPasses, currentScore);
    }

    /// <summary>
    /// Get current combo multiplier
    /// </summary>
    public float GetComboMultiplier()
    {
        return currentComboMultiplier;
    }

    /// <summary>
    /// Reset all stats
    /// </summary>
    public void ResetStats()
    {
        totalObstaclesPassed = 0;
        successfulPasses = 0;
        failedPasses = 0;
        currentScore = 0;
        consecutiveSuccesses = 0;
        currentComboMultiplier = 1f;

        OnScoreChanged?.Invoke(currentScore);
    }
}
