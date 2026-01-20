using UnityEngine;

public class CoreMovementController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DynamicJoystick joystick;
    [SerializeField] private Transform playerTransform;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Position Limits")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = 0f;
    [SerializeField] private float maxY = 10f;

    [Header("Rotation Settings")]
    [SerializeField] private float pitchAngle = 30f;
    [SerializeField] private float rollAngle = 30f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Gyro Settings")]
    [SerializeField] private float gyroSensitivity = 2.0f;

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }

    private void Update()
    {
        if (joystick == null || playerTransform == null) return;

        // 1. Get Joystick Input
        float joyX = joystick.Horizontal;
        float joyY = joystick.Vertical;

        // 2. Get Gyro Input (if enabled)
        float gyroX = 0f;
        float gyroY = 0f;

        if (SettingsManager.IsGyroControlEnabled())
        {
            // Use acceleration for tilt control
            // Landscape mode: X axis tilt is acceleration.x
            // DISABLED Vertical Gyro (Y) as it causes "drift down" issues due to gravity/holding angle
            gyroX = Input.acceleration.x * gyroSensitivity;
            gyroY = 0f; // Input.acceleration.y * gyroSensitivity; 
        }

        // 3. Combine Inputs
        HorizontalInput = Mathf.Clamp(joyX + gyroX, -1f, 1f);
        VerticalInput = Mathf.Clamp(joyY + gyroY, -1f, 1f);

        // 4. Apply Movement
        float moveX = HorizontalInput * moveSpeed * Time.deltaTime;
        float moveY = VerticalInput * moveSpeed * Time.deltaTime;

        Vector3 newPosition = playerTransform.localPosition;
        newPosition.x = Mathf.Clamp(newPosition.x + moveX, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y + moveY, minY, maxY);

        playerTransform.localPosition = newPosition;

        // 5. Apply Rotation
        float targetPitch = -VerticalInput * pitchAngle;
        float targetRoll = -HorizontalInput * rollAngle;

        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);
        playerTransform.localRotation = Quaternion.Lerp(playerTransform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
