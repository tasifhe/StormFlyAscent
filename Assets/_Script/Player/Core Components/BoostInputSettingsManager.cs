using UnityEngine;

/// <summary>
/// Manages boost input optimization settings at runtime
/// Attach to the Character GameObject or in the scene
/// </summary>
public class BoostInputSettingsManager : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Reference to your Mobile Optimization Settings asset")]
    public MobileOptimizationSettings settings;
    
    [Header("Auto-Apply")]
    [Tooltip("Apply settings on Awake")]
    public bool applyOnAwake = true;
    
    private void Awake()
    {
        if (applyOnAwake && settings != null)
        {
            ApplySettings();
        }
    }
    
    /// <summary>
    /// Apply all mobile optimization settings
    /// </summary>
    public void ApplySettings()
    {
        if (settings == null)
        {
            Debug.LogWarning("⚠️ MobileOptimizationSettings not assigned!");
            return;
        }
        
        // Apply core settings
        settings.ApplySettings();
        
        // Apply to BoostInputDetector
        BoostInputDetector inputDetector = FindFirstObjectByType<BoostInputDetector>();
        if (inputDetector != null)
        {
            inputDetector.requireDoubleTap = settings.requireDoubleTap;
            inputDetector.doubleTapWindow = settings.doubleTapWindow;
            inputDetector.inputCooldown = settings.inputCooldown;
            inputDetector.minTouchDuration = settings.minTouchDuration;
            inputDetector.enableHapticFeedback = settings.enableHapticFeedback;
            inputDetector.excludeTopPercent = settings.excludeTopPercent;
            inputDetector.excludeBottomPercent = settings.excludeBottomPercent;
            inputDetector.excludeLeftPercent = settings.excludeLeftPercent;
            inputDetector.excludeRightPercent = settings.excludeRightPercent;
            
            Debug.Log("✓ Input settings applied to BoostInputDetector");
        }
        
        // Apply to BoostSystem
        BoostSystem boostSystem = FindFirstObjectByType<BoostSystem>();
        if (boostSystem != null)
        {
            boostSystem.lowPerformanceMode = settings.forceLowPerformanceMode;
            boostSystem.autoDetectPerformance = settings.autoDetectPerformance;
            boostSystem.disablePhysicsInLowMode = settings.disablePhysicsInLowMode;
            
            Debug.Log("✓ Performance settings applied to BoostSystem");
        }
        
        Debug.Log("✅ All mobile optimizations applied!");
    }
    
    /// <summary>
    /// Change quality preset at runtime
    /// </summary>
    public void SetQualityPreset(MobileQualityPreset preset)
    {
        if (settings != null)
        {
            settings.ApplyQualityPreset(preset);
            ApplySettings();
        }
    }
    
    /// <summary>
    /// Toggle double-tap requirement at runtime
    /// </summary>
    public void SetDoubleTapMode(bool enabled)
    {
        if (settings != null)
        {
            settings.requireDoubleTap = enabled;
            
            BoostInputDetector inputDetector = FindFirstObjectByType<BoostInputDetector>();
            if (inputDetector != null)
            {
                inputDetector.requireDoubleTap = enabled;
                Debug.Log($"Double-tap mode {(enabled ? "ENABLED" : "DISABLED")}");
            }
        }
    }
    
    /// <summary>
    /// Toggle haptic feedback at runtime
    /// </summary>
    public void SetHapticFeedback(bool enabled)
    {
        if (settings != null)
        {
            settings.enableHapticFeedback = enabled;
            
            BoostInputDetector inputDetector = FindFirstObjectByType<BoostInputDetector>();
            if (inputDetector != null)
            {
                inputDetector.enableHapticFeedback = enabled;
                Debug.Log($"Haptic feedback {(enabled ? "ENABLED" : "DISABLED")}");
            }
        }
    }
}
