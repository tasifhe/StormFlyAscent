using UnityEngine;

public class FlyingCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float horizontalSpeed = 8f;
    [SerializeField] private float verticalSpeed = 8f;
    [SerializeField] private float smoothTime = 0.1f;
    
    [Header("Movement Limits")]
    [SerializeField] private float horizontalLimit = 5f;
    [SerializeField] private float verticalLimitMin = 0f;
    [SerializeField] private float verticalLimitMax = 10f;
    
    [Header("Tilt Settings")]
    [SerializeField] private float tiltAngle = 20f;
    [SerializeField] private float tiltSpeed = 5f;
    
    // Private variables
    private Vector3 targetPos;
    private Vector3 velocity = Vector3.zero;
    private float currentTiltX = 0f;
    private float currentTiltZ = 0f;

    // Input variables (will be replaced by mobile input later)
    private float horizontalInput;
    private float verticalInput;
    
    void Start()
    {
    // Initialize target position to current position
    targetPos = transform.position;
    }
    
    void Update()
    {
    HandleInput();
    MoveCharacter();
    ApplyTilt();
    }
    
    void HandleInput()
    {
    // PC Controls - replace this method for mobile later
    horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Arrow Keys
    verticalInput = Input.GetAxisRaw("Vertical");     // W/S or Arrow Keys
    }
    
    void MoveCharacter()
    {
    // Constant forward movement (Z axis)
    float forwardMove = forwardSpeed * Time.deltaTime;
    Vector3 pos = transform.position;
    pos.z += forwardMove;

    // Calculate target X/Y based on input (no drift)
    float targetX = pos.x + horizontalInput * horizontalSpeed * Time.deltaTime;
    float targetY = pos.y + verticalInput * verticalSpeed * Time.deltaTime;

    // Clamp within boundaries
    targetX = Mathf.Clamp(targetX, -horizontalLimit, horizontalLimit);
    targetY = Mathf.Clamp(targetY, verticalLimitMin, verticalLimitMax);

    // Smooth movement to target X/Y
    pos.x = Mathf.SmoothDamp(pos.x, targetX, ref velocity.x, smoothTime);
    pos.y = Mathf.SmoothDamp(pos.y, targetY, ref velocity.y, smoothTime);

    transform.position = pos;
    }
    
    void ApplyTilt()
    {
    // Calculate target tilt based on input
    float targetTiltZ = -horizontalInput * tiltAngle;
    float targetTiltX = verticalInput * tiltAngle;

    // Smooth tilt transition
    currentTiltZ = Mathf.Lerp(currentTiltZ, targetTiltZ, Time.deltaTime * tiltSpeed);
    currentTiltX = Mathf.Lerp(currentTiltX, targetTiltX, Time.deltaTime * tiltSpeed);

    // Apply rotation (keep Y rotation at 0 for endless runner)
    transform.rotation = Quaternion.Euler(currentTiltX, 0f, currentTiltZ);
    }
    
    // Public methods for mobile input (to be implemented later)
    // Public methods for mobile input (to be implemented later)
    public void SetHorizontalInput(float value)
    {
        horizontalInput = Mathf.Clamp(value, -1f, 1f);
    }

    public void SetVerticalInput(float value)
    {
        verticalInput = Mathf.Clamp(value, -1f, 1f);
    }
    
    // Getters for debugging
    public float GetForwardSpeed() => forwardSpeed;
    public Vector3 GetVelocity() => velocity;
}