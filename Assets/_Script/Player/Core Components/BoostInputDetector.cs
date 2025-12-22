using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Centralized input detection for boost/flap
/// Can be used by any flight state
/// Supports: Touch, Mouse, Keyboard, Gamepad, and UI Button
/// MOBILE OPTIMIZED: Double-tap option, touch cooldown, screen zones
/// </summary>
public class BoostInputDetector : MonoBehaviour
{
    [Header("Mobile Optimization")]
    [Tooltip("Require double-tap to boost (prevents accidental boosts)")]
    public bool requireDoubleTap = false;
    
    [Tooltip("Max time between taps for double-tap (seconds)")]
    [Range(0.1f, 0.5f)]
    public float doubleTapWindow = 0.3f;
    
    [Tooltip("Minimum time between boost inputs (spam prevention)")]
    [Range(0f, 0.5f)]
    public float inputCooldown = 0.1f;
    
    [Tooltip("Minimum touch duration to count as valid (filters swipes)")]
    [Range(0f, 0.2f)]
    public float minTouchDuration = 0.05f;
    
    [Tooltip("Enable haptic feedback on mobile")]
    public bool enableHapticFeedback = true;
    
    [Header("Screen Zone Exclusions")]
    [Tooltip("Ignore touches in top % of screen (UI safe zone)")]
    [Range(0f, 30f)]
    public float excludeTopPercent = 15f;
    
    [Tooltip("Ignore touches in bottom % of screen (UI safe zone)")]
    [Range(0f, 30f)]
    public float excludeBottomPercent = 20f;
    
    [Tooltip("Ignore touches in left % of screen")]
    [Range(0f, 30f)]
    public float excludeLeftPercent = 10f;
    
    [Tooltip("Ignore touches in right % of screen")]
    [Range(0f, 30f)]
    public float excludeRightPercent = 10f;
    
    private DynamicJoystick joystick;
    
    // Double-tap tracking
    private float lastTapTime = -999f;
    private int tapCount = 0;
    
    // Input cooldown tracking
    private float lastInputTime = -999f;
    
    // Touch duration tracking
    private float touchStartTime = 0f;
    private bool touchHeld = false;
    
    // Event for boost input
    public event System.Action OnBoostInputDetected;
    
    /// <summary>
    /// Call this from your UI button's OnClick event
    /// </summary>
    public void OnBoostButtonPressed()
    {
        // UI button bypasses all mobile optimizations (it's intentional)
        Debug.Log("👆 BOOST INPUT from UI Button");
        TriggerBoostInput("UI Button");
    }
    
    private void Awake()
    {
        // Enable enhanced touch support
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
    }
    
    private void Start()
    {
        joystick = FindFirstObjectByType<DynamicJoystick>();
        
        if (requireDoubleTap)
        {
            Debug.Log("📱 MOBILE OPTIMIZATION: Double-tap required for boost");
        }
    }
    
    private void Update()
    {
        DetectBoostInput();
        UpdateDoubleTapTimer();
    }
    
    /// <summary>
    /// Reset tap count if double-tap window expires
    /// </summary>
    private void UpdateDoubleTapTimer()
    {
        if (requireDoubleTap && tapCount > 0)
        {
            if (Time.time - lastTapTime > doubleTapWindow)
            {
                tapCount = 0; // Reset if window expired
            }
        }
    }
    
    /// <summary>
    /// Check if position is in excluded screen zone
    /// </summary>
    private bool IsInExcludedZone(Vector2 screenPosition)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        
        // Calculate zone boundaries
        float topBoundary = screenHeight * (1f - excludeTopPercent / 100f);
        float bottomBoundary = screenHeight * (excludeBottomPercent / 100f);
        float leftBoundary = screenWidth * (excludeLeftPercent / 100f);
        float rightBoundary = screenWidth * (1f - excludeRightPercent / 100f);
        
        // Check if in excluded zones
        if (screenPosition.y > topBoundary) return true;       // Top zone
        if (screenPosition.y < bottomBoundary) return true;    // Bottom zone
        if (screenPosition.x < leftBoundary) return true;      // Left zone
        if (screenPosition.x > rightBoundary) return true;     // Right zone
        
        return false;
    }
    
    /// <summary>
    /// Check if enough time has passed since last input (spam prevention)
    /// </summary>
    private bool IsInputOnCooldown()
    {
        return (Time.time - lastInputTime) < inputCooldown;
    }
    
    /// <summary>
    /// Trigger haptic feedback on mobile devices
    /// </summary>
    private void TriggerHapticFeedback()
    {
        if (!enableHapticFeedback) return;
        
        #if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate(); // Light haptic pulse
        #endif
    }
    
    /// <summary>
    /// Final step: Trigger the boost input event
    /// </summary>
    private void TriggerBoostInput(string source)
    {
        lastInputTime = Time.time;
        TriggerHapticFeedback();
        Debug.Log($"👆 BOOST INPUT from {source}");
        OnBoostInputDetected?.Invoke();
    }
    
    /// <summary>
    /// Detect boost input from all sources
    /// MOBILE OPTIMIZED: Double-tap, cooldown, duration filter, screen zones
    /// </summary>
    private void DetectBoostInput()
    {
        bool inputDetected = false;
        string inputSource = "";

        // Check input cooldown first (spam prevention)
        if (IsInputOnCooldown())
        {
            return;
        }

        // ===== TOUCH INPUT (Mobile) =====
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            
            // Track touch duration
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStartTime = Time.time;
                touchHeld = true;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && touchHeld)
            {
                touchHeld = false;
                float touchDuration = Time.time - touchStartTime;
                
                // Check minimum duration (filter out swipes)
                if (touchDuration < minTouchDuration)
                {
                    Debug.Log($"⚠️ Touch too short ({touchDuration:F3}s < {minTouchDuration:F3}s) - filtering out swipe");
                    return;
                }
                
                // Check if touch is over joystick
                if (joystick != null && RectTransformUtility.RectangleContainsScreenPoint(
                    joystick.GetComponent<RectTransform>(), touch.screenPosition))
                {
                    return; // Ignore joystick touches
                }
                
                // Check if in excluded screen zone
                if (IsInExcludedZone(touch.screenPosition))
                {
                    Debug.Log($"⚠️ Touch in excluded zone ({touch.screenPosition}) - ignored");
                    return;
                }
                
                // Double-tap handling
                if (requireDoubleTap)
                {
                    float timeSinceLastTap = Time.time - lastTapTime;
                    
                    if (timeSinceLastTap <= doubleTapWindow)
                    {
                        // Second tap within window - BOOST!
                        tapCount = 0; // Reset
                        inputDetected = true;
                        inputSource = "Touch (Double-Tap)";
                    }
                    else
                    {
                        // First tap - wait for second
                        tapCount = 1;
                        lastTapTime = Time.time;
                        Debug.Log($"📱 First tap registered - tap again within {doubleTapWindow}s");
                        return;
                    }
                }
                else
                {
                    // Single tap mode
                    inputDetected = true;
                    inputSource = "Touch";
                }
                
                lastTapTime = Time.time;
            }
        }
        
        // ===== MOUSE INPUT (Editor/PC) =====
        if (!inputDetected && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (joystick == null || !RectTransformUtility.RectangleContainsScreenPoint(
                joystick.GetComponent<RectTransform>(), mousePos))
            {
                inputDetected = true;
                inputSource = "Mouse";
            }
        }
        
        // ===== KEYBOARD INPUT (Space bar) =====
        if (!inputDetected && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            inputDetected = true;
            inputSource = "Keyboard (Space)";
        }
        
        // ===== GAMEPAD INPUT (A/X button) =====
        if (!inputDetected && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            inputDetected = true;
            inputSource = "Gamepad (A)";
        }

        // Fire event if input detected
        if (inputDetected)
        {
            TriggerBoostInput(inputSource);
        }
    }
}
