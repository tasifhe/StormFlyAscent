using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;
using System.Collections;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Manages the "Tap to Start" splash screen that appears when the game loads
/// </summary>
public class SplashScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject splashPanel;
    [SerializeField] private TextMeshProUGUI tapToStartText;
    [SerializeField] private Image fadeImage;
    
    [Header("Animation Settings")]
    [SerializeField] private float textPulseSpeed = 1.5f;
    [SerializeField] private float textMinAlpha = 0.3f;
    [SerializeField] private float textMaxAlpha = 1f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onTapDetected;
    
    private bool hasStarted = false;
    private bool isTransitioning = false;
    
    private void Start()
    {
        // Enable Enhanced Touch Support for New Input System
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable(); // For testing in editor
        
        if (splashPanel != null)
            splashPanel.SetActive(true);
            
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
        
        StartCoroutine(PulseTextAnimation());
    }
    
    private void Update()
    {
        if (hasStarted || isTransitioning)
            return;
        
        // DEBUG: Log touch info every 60 frames
        if (Time.frameCount % 60 == 0)
        {
            int touchCount = Touch.activeTouches.Count;
            int touchscreenCount = Touchscreen.current != null ? Touchscreen.current.touches.Count : 0;
            int oldTouchCount = Input.touchCount; // Old Input System (Unity Remote)
            Debug.Log($"[SplashScreen] EnhancedTouch: {touchCount}, Touchscreen: {touchscreenCount}, Old Input.touchCount: {oldTouchCount}");
        }
        
        // Detect tap/click input - MULTIPLE METHODS for compatibility
        bool inputDetected = false;
        
        // METHOD 1: Enhanced Touch (mobile) - New Input System
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            Debug.Log($"[SplashScreen] EnhancedTouch detected! Phase: {touch.phase}");
            
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                Debug.Log("[SplashScreen] EnhancedTouch BEGAN - triggering!");
                inputDetected = true;
            }
        }
        
        // METHOD 2: Direct Touchscreen (fallback for Android)
        if (!inputDetected && Touchscreen.current != null)
        {
            var touches = Touchscreen.current.touches;
            for (int i = 0; i < touches.Count; i++)
            {
                var touchControl = touches[i];
                if (touchControl.press.wasPressedThisFrame)
                {
                    Debug.Log("[SplashScreen] Touchscreen.current touch detected - triggering!");
                    inputDetected = true;
                    break;
                }
            }
        }
        
        // METHOD 3: Old Input System (Unity Remote compatibility)
        if (!inputDetected && Input.touchCount > 0)
        {
            UnityEngine.Touch oldTouch = Input.GetTouch(0);
            Debug.Log($"[SplashScreen] Old Input.GetTouch detected! Phase: {oldTouch.phase}");
            
            if (oldTouch.phase == UnityEngine.TouchPhase.Began)
            {
                Debug.Log("[SplashScreen] Old Input Touch BEGAN - triggering!");
                inputDetected = true;
            }
        }
        
        // Mouse input (testing) - New Input System
        if (!inputDetected && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("[SplashScreen] Mouse click detected - triggering!");
            inputDetected = true;
        }
        
        // Keyboard input (testing) - New Input System
        if (!inputDetected && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            Debug.Log("[SplashScreen] Keyboard press detected - triggering!");
            inputDetected = true;
        }
        
        // Gamepad input - New Input System
        if (!inputDetected && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("[SplashScreen] Gamepad button detected - triggering!");
            inputDetected = true;
        }
        
        if (inputDetected)
        {
            Debug.Log("[SplashScreen] Input detected! Calling OnTapDetected()");
            OnTapDetected();
        }
    }
    
    private IEnumerator PulseTextAnimation()
    {
        if (tapToStartText == null)
            yield break;
            
        while (!hasStarted)
        {
            // Pulse alpha using sine wave
            float alpha = Mathf.Lerp(textMinAlpha, textMaxAlpha, 
                (Mathf.Sin(Time.time * textPulseSpeed) + 1f) * 0.5f);
            
            Color c = tapToStartText.color;
            c.a = alpha;
            tapToStartText.color = c;
            
            yield return null;
        }
    }
    
    private void OnTapDetected()
    {
        if (hasStarted)
            return;
            
        hasStarted = true;
        isTransitioning = true;
        
        Debug.Log("Tap detected! Transitioning to main menu...");
        
        // Invoke event
        onTapDetected?.Invoke();
        
        // Start transition
        StartCoroutine(TransitionToMainMenu());
    }
    
    private IEnumerator TransitionToMainMenu()
    {
        // Stop text animation
        if (tapToStartText != null)
        {
            Color c = tapToStartText.color;
            c.a = 0f;
            tapToStartText.color = c;
        }
        
        // Fade out
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = new Color(0, 0, 0, 1f);
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeOutDuration);
                yield return null;
            }
        }
        
        // Hide splash panel
        if (splashPanel != null)
            splashPanel.SetActive(false);
        
        // Notify main menu manager to show
        MainMenuManager mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        
        // Fade back in
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = new Color(0, 0, 0, 0f);
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeOutDuration);
                yield return null;
            }
        }
        
        isTransitioning = false;
    }
    
    // Public method to skip splash (for testing)
    public void SkipToMainMenu()
    {
        if (!hasStarted)
            OnTapDetected();
    }
}
