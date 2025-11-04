using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Mobile Optimization Manager - Handles FPS unlocking and mobile-specific optimizations
/// Attach to a persistent GameObject in your first scene
/// </summary>
public class MobileOptimizationManager : MonoBehaviour
{
    [Header("Frame Rate Settings")]
    [SerializeField] private bool unlockFrameRate = true;
    [SerializeField] private int targetFrameRate = -1; // -1 = unlimited, 60 = capped at 60, 30 = capped at 30
    [SerializeField] private bool disableVSync = true;
    
    [Header("Mobile Optimizations")]
    [SerializeField] private bool optimizeForMobile = true;
    [SerializeField] private bool reduceShadowDistance = true;
    [SerializeField] private float mobileShadowDistance = 30f;
    
    [Header("Battery Optimization")]
    [SerializeField] private bool enableBatterySaving = false;
    [SerializeField] private int batterySavingTargetFPS = 30;
    
    [Header("Auto Quality")]
    [SerializeField] private bool autoAdjustQuality = true;
    [SerializeField] private float lowFPSThreshold = 25f;
    [SerializeField] private float highFPSThreshold = 55f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private float fpsCheckTimer = 0f;
    private const float fpsCheckInterval = 2f;
    private int currentQualityLevel;
    private bool hasAdjustedQuality = false;
    
    private void Awake()
    {
        // Make persistent
        DontDestroyOnLoad(gameObject);
        
        // Apply optimizations immediately
        ApplyOptimizations();
    }
    
    private void Start()
    {
        currentQualityLevel = QualitySettings.GetQualityLevel();
        
        if (showDebugLogs)
        {
            LogSystemInfo();
        }
    }
    
    private void Update()
    {
        // Auto quality adjustment based on FPS
        if (autoAdjustQuality)
        {
            fpsCheckTimer += Time.unscaledDeltaTime;
            if (fpsCheckTimer >= fpsCheckInterval)
            {
                fpsCheckTimer = 0f;
                CheckAndAdjustQuality();
            }
        }
    }
    
    /// <summary>
    /// Apply all optimization settings
    /// </summary>
    private void ApplyOptimizations()
    {
        // Unlock/Set Frame Rate
        ApplyFrameRateSettings();
        
        // Mobile-specific optimizations
        if (optimizeForMobile)
        {
            ApplyMobileOptimizations();
        }
        
        // Battery saving mode
        if (enableBatterySaving)
        {
            ApplyBatterySaving();
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[MobileOptimization] Optimizations applied!");
            Debug.Log($"[MobileOptimization] Target FPS: {targetFrameRate} | VSync: {(disableVSync ? "OFF" : "ON")}");
        }
    }
    
    /// <summary>
    /// Unlock FPS and disable VSync
    /// </summary>
    private void ApplyFrameRateSettings()
    {
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
            if (showDebugLogs) Debug.Log("[MobileOptimization] VSync: DISABLED");
        }
        
        if (unlockFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
            
            if (targetFrameRate == -1)
            {
                if (showDebugLogs) Debug.Log("[MobileOptimization] Frame Rate: UNLOCKED");
            }
            else
            {
                if (showDebugLogs) Debug.Log($"[MobileOptimization] Frame Rate: CAPPED at {targetFrameRate} FPS");
            }
        }
    }
    
    /// <summary>
    /// Apply mobile-specific optimizations
    /// </summary>
    private void ApplyMobileOptimizations()
    {
        // Reduce shadow distance on mobile
        if (reduceShadowDistance)
        {
            QualitySettings.shadowDistance = mobileShadowDistance;
            if (showDebugLogs) Debug.Log($"[MobileOptimization] Shadow Distance: {mobileShadowDistance}m");
        }
        
        // Screen settings
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        // Optimize physics
        Physics.defaultSolverIterations = 6;
        Physics.defaultSolverVelocityIterations = 1;
        
        // Reduce pixel light count for mobile
        QualitySettings.pixelLightCount = 1;
        
        if (showDebugLogs) Debug.Log("[MobileOptimization] Mobile optimizations applied");
    }
    
    /// <summary>
    /// Enable battery saving mode (lower FPS)
    /// </summary>
    private void ApplyBatterySaving()
    {
        Application.targetFrameRate = batterySavingTargetFPS;
        
        if (showDebugLogs)
        {
            Debug.Log($"[MobileOptimization] Battery Saving Mode: {batterySavingTargetFPS} FPS");
        }
    }
    
    /// <summary>
    /// Automatically adjust quality based on current FPS
    /// </summary>
    private void CheckAndAdjustQuality()
    {
        float currentFPS = 1f / Time.unscaledDeltaTime;
        int maxQualityLevel = QualitySettings.names.Length - 1;
        
        // FPS too low - reduce quality
        if (currentFPS < lowFPSThreshold && currentQualityLevel > 0)
        {
            currentQualityLevel--;
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            hasAdjustedQuality = true;
            
            if (showDebugLogs)
            {
                Debug.LogWarning($"[MobileOptimization] FPS Low ({currentFPS:F1}) - Reducing quality to: {QualitySettings.names[currentQualityLevel]}");
            }
        }
        // FPS high enough - increase quality
        else if (currentFPS > highFPSThreshold && currentQualityLevel < maxQualityLevel && hasAdjustedQuality)
        {
            currentQualityLevel++;
            QualitySettings.SetQualityLevel(currentQualityLevel, true);
            
            if (showDebugLogs)
            {
                Debug.Log($"[MobileOptimization] FPS High ({currentFPS:F1}) - Increasing quality to: {QualitySettings.names[currentQualityLevel]}");
            }
        }
    }
    
    /// <summary>
    /// Log system information for debugging
    /// </summary>
    private void LogSystemInfo()
    {
        Debug.Log("=== SYSTEM INFO ===");
        Debug.Log($"Device Model: {SystemInfo.deviceModel}");
        Debug.Log($"Device Type: {SystemInfo.deviceType}");
        Debug.Log($"Operating System: {SystemInfo.operatingSystem}");
        Debug.Log($"Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} cores)");
        Debug.Log($"System Memory: {SystemInfo.systemMemorySize} MB");
        Debug.Log($"Graphics Device: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"Graphics Memory: {SystemInfo.graphicsMemorySize} MB");
        Debug.Log($"Max Texture Size: {SystemInfo.maxTextureSize}");
        Debug.Log($"Graphics API: {SystemInfo.graphicsDeviceType}");
        Debug.Log($"Current Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log("==================");
    }
    
    #region Public Methods
    
    /// <summary>
    /// Toggle VSync at runtime
    /// </summary>
    public void ToggleVSync(bool enabled)
    {
        disableVSync = !enabled;
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        
        if (showDebugLogs)
        {
            Debug.Log($"[MobileOptimization] VSync: {(enabled ? "ENABLED" : "DISABLED")}");
        }
    }
    
    /// <summary>
    /// Set target frame rate at runtime
    /// </summary>
    public void SetTargetFrameRate(int fps)
    {
        targetFrameRate = fps;
        Application.targetFrameRate = fps;
        
        if (showDebugLogs)
        {
            Debug.Log($"[MobileOptimization] Target FPS set to: {fps}");
        }
    }
    
    /// <summary>
    /// Toggle battery saving mode
    /// </summary>
    public void ToggleBatterySaving(bool enabled)
    {
        enableBatterySaving = enabled;
        if (enabled)
        {
            ApplyBatterySaving();
        }
        else
        {
            ApplyFrameRateSettings();
        }
    }
    
    /// <summary>
    /// Force a specific quality level
    /// </summary>
    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        currentQualityLevel = level;
        
        if (showDebugLogs)
        {
            Debug.Log($"[MobileOptimization] Quality set to: {QualitySettings.names[level]}");
        }
    }
    
    #endregion
}
