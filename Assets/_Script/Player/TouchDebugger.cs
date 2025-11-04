using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Debug script to help diagnose touch input issues in builds (New Input System)
/// Attach this to a GameObject in your scene to see debug info on screen
/// </summary>
public class TouchDebugger : MonoBehaviour
{
    [Header("Debug Display")]
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private bool showDebugOnScreen = true;
    
    private DynamicJoystick joystick;
    private EventSystem eventSystem;
    
    private void Start()
    {
        // Enable Enhanced Touch Support
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        
        // Find joystick
        joystick = FindFirstObjectByType<DynamicJoystick>();
        
        // Find event system
        eventSystem = FindFirstObjectByType<EventSystem>();
        
        // Create debug text if needed and not assigned
        if (showDebugOnScreen && debugText == null)
        {
            CreateDebugText();
        }
        
        // Log initial status
        Debug.Log($"[TouchDebugger] Joystick found: {joystick != null}");
        Debug.Log($"[TouchDebugger] EventSystem found: {eventSystem != null}");
        Debug.Log($"[TouchDebugger] New Input System: Enabled");
        Debug.Log($"[TouchDebugger] Enhanced Touch: {EnhancedTouchSupport.enabled}");
    }
    
    private void Update()
    {
        string debugInfo = GetDebugInfo();
        
        // Display on screen
        if (showDebugOnScreen && debugText != null)
        {
            debugText.text = debugInfo;
        }
        
        // Log touch events (New Input System)
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            Debug.Log($"[TouchDebugger] Touch detected: {touch.phase} at {touch.screenPosition}");
        }
    }
    
    private string GetDebugInfo()
    {
        string info = "<b>Touch Debug Info</b>\n\n";
        
        // EventSystem status
        info += $"EventSystem: {(eventSystem != null ? "✓ Found" : "✗ MISSING!")}\n";
        if (eventSystem != null)
        {
            info += $"EventSystem Active: {eventSystem.enabled}\n";
        }
        
        // Joystick status
        info += $"\nJoystick: {(joystick != null ? "✓ Found" : "✗ MISSING!")}\n";
        if (joystick != null)
        {
            info += $"Joystick Input: ({joystick.Horizontal:F2}, {joystick.Vertical:F2})\n";
        }
        
        // Touch info (New Input System)
        info += $"\nEnhanced Touch Enabled: {EnhancedTouchSupport.enabled}\n";
        info += $"Touch Count: {Touch.activeTouches.Count}\n";
        
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            info += $"Touch Phase: {touch.phase}\n";
            info += $"Touch Pos: {touch.screenPosition}\n";
        }
        
        // Mouse (for testing) - New Input System
        if (Mouse.current != null)
        {
            info += $"\nMouse Pos: {Mouse.current.position.ReadValue()}\n";
            info += $"Mouse Button: {Mouse.current.leftButton.isPressed}\n";
        }
        
        // Keyboard (for testing) - New Input System
        if (Keyboard.current != null)
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontal += 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) vertical -= 1f;
            
            info += $"\nHorizontal: {horizontal:F2}\n";
            info += $"Vertical: {vertical:F2}\n";
            info += $"Space Key: {Keyboard.current.spaceKey.isPressed}\n";
        }
        
        // Gamepad - New Input System
        if (Gamepad.current != null)
        {
            Vector2 leftStick = Gamepad.current.leftStick.ReadValue();
            info += $"\nGamepad Connected: Yes\n";
            info += $"Left Stick: ({leftStick.x:F2}, {leftStick.y:F2})\n";
        }
        else
        {
            info += $"\nGamepad Connected: No\n";
        }
        
        return info;
    }
    
    private void CreateDebugText()
    {
        // Create a Canvas if one doesn't exist
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Debug Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create debug text object
        GameObject textObj = new GameObject("TouchDebugText");
        textObj.transform.SetParent(canvas.transform, false);
        
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 16;
        debugText.color = Color.white;
        debugText.alignment = TextAlignmentOptions.TopLeft;
        
        // Position in top-left corner
        RectTransform rectTransform = debugText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(10, -10);
        rectTransform.sizeDelta = new Vector2(400, 400);
        
        Debug.Log("[TouchDebugger] Created debug text display");
    }
}
