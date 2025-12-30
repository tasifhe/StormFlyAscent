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

    public float HorizontalInput { get; private set; }
    public float VerticalInput { get; private set; }

    private void Update()
    {
        if (joystick == null || playerTransform == null) return;

        HorizontalInput = joystick.Horizontal;
        VerticalInput = joystick.Vertical;

        float moveX = HorizontalInput * moveSpeed * Time.deltaTime;
        float moveY = VerticalInput * moveSpeed * Time.deltaTime;

        Vector3 newPosition = playerTransform.localPosition;
        newPosition.x = Mathf.Clamp(newPosition.x + moveX, minX, maxX);
        newPosition.y = Mathf.Clamp(newPosition.y + moveY, minY, maxY);

        playerTransform.localPosition = newPosition;

        float targetPitch = -joystick.Vertical * pitchAngle;
        float targetRoll = -joystick.Horizontal * rollAngle;

        Quaternion targetRotation = Quaternion.Euler(targetPitch, 0f, targetRoll);
        playerTransform.localRotation = Quaternion.Lerp(playerTransform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}
