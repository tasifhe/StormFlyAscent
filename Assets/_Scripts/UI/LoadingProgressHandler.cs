using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Handles loading progress tracking with multiple stages
/// Provides detailed feedback during scene loading
/// </summary>
public class LoadingProgressHandler : MonoBehaviour
{
    [Serializable]
    public class LoadingStage
    {
        public string stageName;
        public string displayText;
        [Range(0f, 1f)]
        public float progressWeight = 0.1f;
        public bool isCompleted = false;
    }
    
    [Header("Loading Stages Configuration")]
    [SerializeField] private List<LoadingStage> loadingStages = new List<LoadingStage>()
    {
        new LoadingStage { stageName = "Initialize", displayText = "Initializing...", progressWeight = 0.1f },
        new LoadingStage { stageName = "LoadScene", displayText = "Loading Scene...", progressWeight = 0.4f },
        new LoadingStage { stageName = "LoadAssets", displayText = "Loading Assets...", progressWeight = 0.2f },
        new LoadingStage { stageName = "PrepareEnvironment", displayText = "Preparing Environment...", progressWeight = 0.15f },
        new LoadingStage { stageName = "Finalize", displayText = "Finalizing...", progressWeight = 0.15f }
    };
    
    [Header("Progress Tracking")]
    [SerializeField] private bool enableDetailedLogging = false;
    
    // Events
    public event Action<LoadingStage, float> OnStageStarted;
    public event Action<LoadingStage, float> OnStageCompleted;
    public event Action<float, string> OnProgressUpdated;
    public event Action OnAllStagesCompleted;
    
    // State
    private int currentStageIndex = 0;
    private float totalProgress = 0f;
    private bool isTracking = false;
    private LoadingScreenManager loadingScreen;
    
    private static LoadingProgressHandler instance;
    
    public static LoadingProgressHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LoadingProgressHandler>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        loadingScreen = LoadingScreenManager.Instance;
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Start tracking loading progress
    /// </summary>
    public void StartTracking()
    {
        if (isTracking)
        {
            Debug.LogWarning("[LoadingProgressHandler] Already tracking");
            return;
        }
        
        isTracking = true;
        currentStageIndex = 0;
        totalProgress = 0f;
        
        // Reset all stages
        foreach (var stage in loadingStages)
        {
            stage.isCompleted = false;
        }
        
        if (enableDetailedLogging)
        {
            Debug.Log("[LoadingProgressHandler] Started tracking with " + loadingStages.Count + " stages");
        }
        
        // Start first stage
        if (loadingStages.Count > 0)
        {
            StartStage(0);
        }
    }
    
    /// <summary>
    /// Complete the current stage and move to next
    /// </summary>
    public async Task CompleteCurrentStageAsync()
    {
        if (!isTracking || currentStageIndex >= loadingStages.Count)
            return;
        
        var currentStage = loadingStages[currentStageIndex];
        currentStage.isCompleted = true;
        
        // Calculate progress
        float stageProgress = 0f;
        for (int i = 0; i <= currentStageIndex; i++)
        {
            stageProgress += loadingStages[i].progressWeight;
        }
        totalProgress = stageProgress;
        
        if (enableDetailedLogging)
        {
            Debug.Log($"[LoadingProgressHandler] Completed stage: {currentStage.stageName} ({totalProgress * 100}%)");
        }
        
        // Update UI
        UpdateLoadingUI(totalProgress, currentStage.displayText);
        OnStageCompleted?.Invoke(currentStage, totalProgress);
        
        // Brief pause for visual feedback
        await Task.Delay(100);
        
        // Move to next stage
        currentStageIndex++;
        
        if (currentStageIndex < loadingStages.Count)
        {
            StartStage(currentStageIndex);
        }
        else
        {
            // All stages complete
            FinishTracking();
        }
    }
    
    /// <summary>
    /// Update progress within current stage
    /// </summary>
    public void UpdateStageProgress(float stageProgress)
    {
        if (!isTracking || currentStageIndex >= loadingStages.Count)
            return;
        
        stageProgress = Mathf.Clamp01(stageProgress);
        
        // Calculate total progress including current stage
        float previousProgress = 0f;
        for (int i = 0; i < currentStageIndex; i++)
        {
            previousProgress += loadingStages[i].progressWeight;
        }
        
        float currentStageWeight = loadingStages[currentStageIndex].progressWeight;
        totalProgress = previousProgress + (currentStageWeight * stageProgress);
        
        // Update UI
        var currentStage = loadingStages[currentStageIndex];
        UpdateLoadingUI(totalProgress, currentStage.displayText);
    }
    
    /// <summary>
    /// Skip to a specific stage
    /// </summary>
    public void SkipToStage(string stageName)
    {
        int index = loadingStages.FindIndex(s => s.stageName == stageName);
        if (index >= 0)
        {
            // Mark previous stages as complete
            for (int i = 0; i < index; i++)
            {
                loadingStages[i].isCompleted = true;
            }
            
            currentStageIndex = index;
            StartStage(index);
        }
    }
    
    /// <summary>
    /// Stop tracking
    /// </summary>
    public void StopTracking()
    {
        isTracking = false;
        if (enableDetailedLogging)
        {
            Debug.Log("[LoadingProgressHandler] Stopped tracking");
        }
    }
    
    /// <summary>
    /// Get current progress (0-1)
    /// </summary>
    public float GetProgress() => totalProgress;
    
    /// <summary>
    /// Get current stage name
    /// </summary>
    public string GetCurrentStageName()
    {
        if (currentStageIndex < loadingStages.Count)
            return loadingStages[currentStageIndex].stageName;
        return "Complete";
    }
    
    /// <summary>
    /// Check if tracking is active
    /// </summary>
    public bool IsTracking => isTracking;
    
    #endregion
    
    #region Private Methods
    
    private void StartStage(int stageIndex)
    {
        if (stageIndex >= loadingStages.Count)
            return;
        
        var stage = loadingStages[stageIndex];
        
        if (enableDetailedLogging)
        {
            Debug.Log($"[LoadingProgressHandler] Starting stage: {stage.stageName}");
        }
        
        // Update UI
        UpdateLoadingUI(totalProgress, stage.displayText);
        OnStageStarted?.Invoke(stage, totalProgress);
    }
    
    private void FinishTracking()
    {
        totalProgress = 1f;
        UpdateLoadingUI(1f, "Complete!");
        
        if (enableDetailedLogging)
        {
            Debug.Log("[LoadingProgressHandler] All stages completed");
        }
        
        OnAllStagesCompleted?.Invoke();
        isTracking = false;
    }
    
    private void UpdateLoadingUI(float progress, string status)
    {
        // Update loading screen if available
        if (loadingScreen != null && loadingScreen.IsLoading)
        {
            loadingScreen.SetProgress(progress, status);
        }
        
        // Invoke event
        OnProgressUpdated?.Invoke(progress, status);
    }
    
    #endregion
    
    #region Editor Helpers
    
    /// <summary>
    /// Add a custom loading stage
    /// </summary>
    public void AddStage(string name, string displayText, float weight)
    {
        loadingStages.Add(new LoadingStage
        {
            stageName = name,
            displayText = displayText,
            progressWeight = weight,
            isCompleted = false
        });
    }
    
    /// <summary>
    /// Clear all stages
    /// </summary>
    public void ClearStages()
    {
        loadingStages.Clear();
        currentStageIndex = 0;
        totalProgress = 0f;
    }
    
    #endregion
}
