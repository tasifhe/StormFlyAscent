using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles all input sources for flight control
/// Supports joystick, keyboard, and gamepad simultaneously
/// </summary>
public class FlightInputHandler
{
    private DynamicJoystick joystick;
    
    public FlightInputHandler(DynamicJoystick joystick)
    {
        this.joystick = joystick;
    }
    
    /// <summary>
    /// Get combined input from all sources (joystick, keyboard, gamepad)
    /// Returns normalized input vector
    /// </summary>
    public Vector3 GetRawInput()
    {
        Vector3 rawInput = Vector3.zero;
        
        // ===== JOYSTICK INPUT (Works with BOTH touch AND mouse!) =====
        rawInput += GetJoystickInput();
        
        // ===== KEYBOARD INPUT =====
        rawInput += GetKeyboardInput();
        
        // ===== GAMEPAD INPUT =====
        rawInput += GetGamepadInput();
        
        // Clamp combined input to prevent over-steering
        if (rawInput.sqrMagnitude > 1f)
        {
            rawInput = rawInput.normalized;
        }
        
        return rawInput;
    }
    
    /// <summary>
    /// Get joystick input (touch or mouse drag)
    /// </summary>
    private Vector3 GetJoystickInput()
    {
        if (joystick == null) return Vector3.zero;
        
        Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
        if (joystickInput.sqrMagnitude > 0.01f)
        {
            return new Vector3(joystickInput.x, joystickInput.y, 0f);
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get keyboard input (WASD + Arrow keys)
    /// </summary>
    private Vector3 GetKeyboardInput()
    {
        if (Keyboard.current == null) return Vector3.zero;
        
        float horizontal = 0f;
        float vertical = 0f;
        
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) 
            horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) 
            horizontal += 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) 
            vertical += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) 
            vertical -= 1f;
        
        Vector2 keyboardInput = new Vector2(horizontal, vertical);
        if (keyboardInput.sqrMagnitude > 0.01f)
        {
            return new Vector3(keyboardInput.x, keyboardInput.y, 0f);
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Get gamepad input (left stick)
    /// </summary>
    private Vector3 GetGamepadInput()
    {
        if (Gamepad.current == null) return Vector3.zero;
        
        Vector2 stickInput = Gamepad.current.leftStick.ReadValue();
        if (stickInput.sqrMagnitude > 0.01f)
        {
            return new Vector3(stickInput.x, stickInput.y, 0f);
        }
        
        return Vector3.zero;
    }
    
    /// <summary>
    /// Update the joystick reference (useful for hot-swapping)
    /// </summary>
    public void SetJoystick(DynamicJoystick newJoystick)
    {
        joystick = newJoystick;
    }
}
