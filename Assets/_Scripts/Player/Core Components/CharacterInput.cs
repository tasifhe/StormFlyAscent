using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// CharacterInput - Simple input handler for joystick and keyboard
/// </summary>
[RequireComponent(typeof(Character))]
public class CharacterInput : MonoBehaviour
{
    [Header("Input References")]
    public DynamicJoystick joystick;

    // Current input values
    private Vector2 currentInput;

    private void Awake()
    {
        // Auto-find joystick if not assigned
        if (joystick == null)
        {
            joystick = FindFirstObjectByType<DynamicJoystick>();
        }
    }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        currentInput = Vector2.zero;

        // 1. Joystick input (priority for mobile)
        if (joystick != null)
        {
            Vector2 joystickInput = new Vector2(joystick.Horizontal, joystick.Vertical);
            if (joystickInput.sqrMagnitude > 0.01f)
            {
                currentInput = joystickInput;
                return;
            }
        }

        // 2. Keyboard input (WASD/Arrows)
        if (Keyboard.current != null)
        {
            Vector2 keyboardInput = Vector2.zero;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                keyboardInput.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                keyboardInput.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                keyboardInput.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                keyboardInput.x += 1f;

            if (keyboardInput.sqrMagnitude > 0.01f)
            {
                currentInput = keyboardInput.normalized;
            }
        }
    }

    public Vector2 GetInput()
    {
        return currentInput;
    }

    public float GetHorizontal()
    {
        return currentInput.x;
    }

    public float GetVertical()
    {
        return currentInput.y;
    }
}
