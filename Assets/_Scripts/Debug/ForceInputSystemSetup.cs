using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
    
    [Header("Auto-fix EventSystem")]
    [SerializeField] private bool autoFixEventSystem = true;
    
    private void Awake()
    {
        // Persist this across scenes
        DontDestroyOnLoad(gameObject);
        
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
        
        // Fix EventSystem if needed
        if (autoFixEventSystem)
        {
            FixEventSystem();
        }
    }
    
    private void FixEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        
        if (eventSystem != null)
        {
            // Remove old input module
            StandaloneInputModule oldModule = eventSystem.GetComponent<StandaloneInputModule>();
            if (oldModule != null)
            {
                if (showDebugLogs)
                    Debug.LogWarning("[ForceInputSystemSetup] Removing OLD StandaloneInputModule from EventSystem!");
                DestroyImmediate(oldModule);
            }
            
            // Add new input module if missing
            InputSystemUIInputModule newModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (newModule == null)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                if (showDebugLogs)
                    Debug.Log("[ForceInputSystemSetup] Added InputSystemUIInputModule to EventSystem!");
            }
        }
    }
    
    private void Update()
    {
        // Continuously ensure Enhanced Touch stays enabled
        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
            Debug.LogWarning("[ForceInputSystemSetup] Re-enabled Enhanced Touch Support!");
        }
        
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
