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
    
    [Header("Combo System")]
    [Tooltip("Enable combo multiplier for consecutive successes")]
    public bool useComboSystem = true;
    
    [Tooltip("Consecutive successes before combo starts")]
    public int comboThreshold = 3;
    
    [Tooltip("Maximum combo multiplier")]
    public float maxComboMultiplier = 5f;
    
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
        
        Debug.Log($"✗ Obstacle missed!");
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
