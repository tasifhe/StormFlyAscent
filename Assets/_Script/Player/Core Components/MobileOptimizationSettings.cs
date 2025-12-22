using UnityEngine;

/// <summary>
/// ScriptableObject for mobile optimization settings
/// Allows easy tweaking without rebuilding
/// Create via: Assets > Create > Mobile Optimization Settings
/// </summary>
[CreateAssetMenu(fileName = "MobileOptimizationSettings", menuName = "StormFly/Mobile Optimization Settings", order = 1)]
public class MobileOptimizationSettings : ScriptableObject
{
    [Header("Input Settings")]
    [Tooltip("Require double-tap to boost (prevents accidental boosts)")]
    public bool requireDoubleTap = false;
    
    [Tooltip("Max time between taps for double-tap")]
    [Range(0.1f, 0.5f)]
    public float doubleTapWindow = 0.3f;
    
    [Tooltip("Minimum time between boost inputs")]
    [Range(0f, 0.5f)]
    public float inputCooldown = 0.1f;
    
    [Tooltip("Minimum touch duration to count as valid")]
    [Range(0f, 0.2f)]
    public float minTouchDuration = 0.05f;
    
    [Tooltip("Enable haptic feedback")]
    public bool enableHapticFeedback = true;
    
    [Header("Screen Zone Exclusions (%)")]
    [Range(0f, 30f)]
    public float excludeTopPercent = 15f;
    
    [Range(0f, 30f)]
    public float excludeBottomPercent = 20f;
    
    [Range(0f, 30f)]
    public float excludeLeftPercent = 10f;
    
    [Range(0f, 30f)]
    public float excludeRightPercent = 10f;
    
    [Header("Performance Settings")]
    [Tooltip("Enable low performance mode manually")]
    public bool forceLowPerformanceMode = false;
    
    [Tooltip("Auto-detect and adjust to device performance")]
    public bool autoDetectPerformance = true;
    
    [Tooltip("Disable physics forces in low performance mode")]
    public bool disablePhysicsInLowMode = false;
    
    [Tooltip("Target frame rate for mobile")]
    [Range(30, 120)]
    public int targetFrameRate = 60;
    
    [Header("Quality Presets")]
    [Tooltip("Apply preset on game start")]
    public bool applyPresetOnStart = true;
    
    public MobileQualityPreset qualityPreset = MobileQualityPreset.Balanced;
    
    /// <summary>
    /// Apply settings to the game
    /// </summary>
    public void ApplySettings()
    {
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
        
        // Apply quality preset
        if (applyPresetOnStart)
        {
            ApplyQualityPreset(qualityPreset);
        }
        
        Debug.Log($"📱 Mobile settings applied: {qualityPreset} @ {targetFrameRate}fps");
    }
    
    /// <summary>
    /// Apply quality preset
    /// </summary>
    public void ApplyQualityPreset(MobileQualityPreset preset)
    {
        switch (preset)
        {
            case MobileQualityPreset.HighPerformance:
                // Prioritize frame rate
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
                QualitySettings.particleRaycastBudget = 64;
                targetFrameRate = 60;
                forceLowPerformanceMode = true;
                disablePhysicsInLowMode = true;
                break;
                
            case MobileQualityPreset.Balanced:
                // Balance between quality and performance
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.antiAliasing = 2;
                QualitySettings.particleRaycastBudget = 256;
                targetFrameRate = 60;
                forceLowPerformanceMode = false;
                disablePhysicsInLowMode = false;
                break;
                
            case MobileQualityPreset.HighQuality:
                // Prioritize visual quality
                QualitySettings.vSyncCount = 1;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 4;
                QualitySettings.particleRaycastBudget = 1024;
                targetFrameRate = 60;
                forceLowPerformanceMode = false;
                disablePhysicsInLowMode = false;
                break;
        }
        
        Debug.Log($"✓ Quality preset applied: {preset}");
    }
}

/// <summary>
/// Quality presets for different device tiers
/// </summary>
public enum MobileQualityPreset
{
    HighPerformance,  // Low-end devices (30fps minimum)
    Balanced,         // Mid-range devices (60fps target)
    HighQuality       // High-end devices (60fps+ smooth)
}
