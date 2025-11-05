using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TMPro;

/// <summary>
/// Debug tool to see what inputs are being detected
/// </summary>
public class InputDebugger : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI debugText;
    
    private DynamicJoystick joystick;
    
    private void Start()
    {
        // Enable enhanced touch
        EnhancedTouchSupport.Enable();
        TouchSimulation.Enable();
        
        // Find joystick
        joystick = FindFirstObjectByType<DynamicJoystick>();
        
        // Create debug text if not assigned
        if (debugText == null)
        {
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                GameObject textObj = new GameObject("InputDebug");
                textObj.transform.SetParent(canvas.transform);
                
                RectTransform rect = textObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.anchoredPosition = new Vector2(10, -200);
                rect.sizeDelta = new Vector2(400, 400);
                
                debugText = textObj.AddComponent<TextMeshProUGUI>();
                debugText.fontSize = 18;
                debugText.color = Color.white;
                debugText.alignment = TextAlignmentOptions.TopLeft;
            }
        }
    }
    
    private void Update()
    {
        if (debugText == null) return;
        
        string debug = "=== INPUT DEBUG ===\n\n";
        
        // Touch info
        debug += $"Touch Support: {EnhancedTouchSupport.enabled}\n";
        debug += $"Active Touches: {Touch.activeTouches.Count}\n";
        
        if (Touch.activeTouches.Count > 0)
        {
            Touch touch = Touch.activeTouches[0];
            debug += $"Touch Pos: {touch.screenPosition}\n";
            debug += $"Touch Phase: {touch.phase}\n";
        }
        
        // Joystick info
        debug += $"\nJoystick Found: {joystick != null}\n";
        if (joystick != null)
        {
            debug += $"Joystick H: {joystick.Horizontal:F2}\n";
            debug += $"Joystick V: {joystick.Vertical:F2}\n";
        }
        
        // Mouse info
        if (Mouse.current != null)
        {
            debug += $"\nMouse Pos: {Mouse.current.position.ReadValue()}\n";
            debug += $"Mouse Left: {Mouse.current.leftButton.isPressed}\n";
        }
        
        // Keyboard info
        if (Keyboard.current != null)
        {
            bool anyKey = Keyboard.current.anyKey.isPressed;
            debug += $"\nKeyboard: {anyKey}\n";
            if (Keyboard.current.wKey.isPressed) debug += "W pressed\n";
            if (Keyboard.current.aKey.isPressed) debug += "A pressed\n";
            if (Keyboard.current.sKey.isPressed) debug += "S pressed\n";
            if (Keyboard.current.dKey.isPressed) debug += "D pressed\n";
            if (Keyboard.current.spaceKey.isPressed) debug += "SPACE pressed\n";
        }
        
        // Gamepad info
        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            debug += $"\nGamepad Stick: ({stick.x:F2}, {stick.y:F2})\n";
        }
        
        debugText.text = debug;
    }
    
    private void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }
}
