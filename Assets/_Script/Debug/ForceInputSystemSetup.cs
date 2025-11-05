using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Forces Input System setup for Android touch input
/// Add this to the first scene that loads (Main Menu or Splash)
/// </summary>
[DefaultExecutionOrder(-2000)] // Run VERY early
public class ForceInputSystemSetup : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private void Awake()
    {
        // Force enable Enhanced Touch Support
        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
            if (showDebugLogs)
                Debug.Log("[ForceInputSystemSetup] Enhanced Touch Support ENABLED");
        }
        
        // Enable Touch Simulation for editor testing
        TouchSimulation.Enable();
        if (showDebugLogs)
            Debug.Log("[ForceInputSystemSetup] Touch Simulation ENABLED for editor");
        
        // Check if touch is available
        if (Touchscreen.current != null)
        {
            if (showDebugLogs)
                Debug.Log($"[ForceInputSystemSetup] Touchscreen detected: {Touchscreen.current.name}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("[ForceInputSystemSetup] No Touchscreen detected! (OK if testing in editor)");
        }
        
        // Check input system backend
        if (showDebugLogs)
        {
            Debug.Log($"[ForceInputSystemSetup] Input System Backend: {InputSystem.settings?.updateMode ?? InputSettings.UpdateMode.ProcessEventsInDynamicUpdate}");
            Debug.Log($"[ForceInputSystemSetup] Enhanced Touch Enabled: {EnhancedTouchSupport.enabled}");
        }
    }
    
    private void Update()
    {
        // Debug touch info every 2 seconds
        if (showDebugLogs && Time.frameCount % 120 == 0)
        {
            int enhancedTouchCount = Touch.activeTouches.Count;
            int touchscreenCount = Touchscreen.current != null ? Touchscreen.current.touches.Count : 0;
            
            Debug.Log($"[ForceInputSystemSetup] Active touches - Enhanced: {enhancedTouchCount}, Touchscreen: {touchscreenCount}");
            
            // If touch is detected, show details
            if (enhancedTouchCount > 0)
            {
                Touch touch = Touch.activeTouches[0];
                Debug.Log($"[ForceInputSystemSetup] Touch 0: Phase={touch.phase}, Pos={touch.screenPosition}");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Don't disable Enhanced Touch - other scripts need it!
        // EnhancedTouchSupport.Disable();
    }
}
