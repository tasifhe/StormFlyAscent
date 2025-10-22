using UnityEngine;
using Dreamteck.Forever;
using Dreamteck.Splines;

/// <summary>
/// BirdPathFollower - Makes the bird follow Forever's generated path using Runner system
/// Uses Forever's MotionModule for smooth rotation matching path bends
/// </summary>
[RequireComponent(typeof(Character))]
public class BirdPathFollower : Runner
{
    [Header("Bird Flight Settings")]
    [Tooltip("Base forward speed of the bird")]
    public float birdSpeed = 10f;
    
    [Tooltip("Offset from the center of the path (for lane switching)")]
    public float lateralOffset = 0f;
    
    [Tooltip("Max lateral offset for steering")]
    public float maxLateralOffset = 5f;
    
    [Tooltip("Height offset above the path")]
    public float heightOffset = 2f;
    
    [Tooltip("How fast the bird adjusts to offsets")]
    public float offsetSmoothSpeed = 5f;
    
    [Header("Rotation Settings")]
    [Tooltip("Smoothness of rotation alignment with path")]
    [Range(0f, 1f)]
    public float rotationSmoothness = 0.5f;
    
    [Tooltip("Banking angle when steering left/right")]
    public float bankingAngle = 30f;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color pathColor = Color.green;
    public Color targetColor = Color.yellow;
    
    // Private variables
    private Character character;
    private float currentBankAngle = 0f;
    private float smoothedLateralOffset = 0f;
    private float targetLateralOffset = 0f;
    
    protected override void Awake()
    {
        base.Awake();
        character = GetComponent<Character>();
        
        // Configure Runner settings for bird
        physicsMode = PhysicsMode.Rigidbody;
        updateMode = UpdateMode.FixedUpdate;
        startMode = StartMode.Project; // Start by projecting bird's current position
        follow = true;
        isPlayer = true;
        
        // Set initial speed from Character
        if (character != null)
        {
            followSpeed = character.forwardSpeed;
        }
        
        // Configure motion module for smooth rotation
        _motion.applyPosition = true;    // Apply position from path
        _motion.applyRotation = true;    // Apply rotation from path
        _motion.rotationOffset = Vector3.zero; // Will add banking in OnFollow
        
        // Configure offset
        _motion.offset = Vector2.zero; // We'll update this dynamically
        _motion.useSplineSizes = false;
    }
    
    private void Start()
    {
        // Start following when ready
        if (LevelGenerator.instance != null && LevelGenerator.instance.ready)
        {
            StartFollow();
            Debug.Log("BirdPathFollower: Started following Forever path!");
        }
    }
    
    protected override void Update()
    {
        base.Update(); // Call Runner's update
        
        // Auto-start if not following yet
        if (!follow && LevelGenerator.instance != null && LevelGenerator.instance.ready)
        {
            StartFollow();
        }
        
        // Update speed from character
        if (character != null)
        {
            followSpeed = character.forwardSpeed;
        }
        
        // Smooth lateral offset
        smoothedLateralOffset = Mathf.Lerp(smoothedLateralOffset, targetLateralOffset, Time.deltaTime * offsetSmoothSpeed);
    }
    
    protected override void FixedUpdate()
    {
        base.FixedUpdate(); // Call Runner's fixed update
    }
    
    /// <summary>
    /// Override OnFollow to apply custom offsets and banking rotation
    /// </summary>
    protected override void OnFollow(SplineSample followResult)
    {
        // Calculate banking angle first
        float lateralVelocity = 0f;
        if (character != null && character.rb != null)
        {
            lateralVelocity = character.rb.linearVelocity.x;
        }
        
        // Calculate target bank angle from lateral velocity
        float targetBankAngle = -Mathf.Clamp(lateralVelocity * 2f, -bankingAngle, bankingAngle);
        
        // Smooth banking
        currentBankAngle = Mathf.Lerp(currentBankAngle, targetBankAngle, Time.fixedDeltaTime * 5f);
        
        // Update motion offsets with smooth values
        _motion.offset = new Vector2(smoothedLateralOffset, heightOffset);
        
        // Set rotation offset to include banking (roll on Z-axis)
        // The banking is applied as a rotation offset so it works WITH the path rotation
        _motion.rotationOffset = new Vector3(0f, 0f, currentBankAngle);
        
        // Apply motion using Runner's system (handles smooth rotation automatically)
        // This will align bird's Z-axis with path forward and apply banking
        base.OnFollow(followResult);
    }
    
    /// <summary>
    /// Set lateral offset (for steering/lane switching)
    /// </summary>
    public void SetLateralOffset(float offset)
    {
        targetLateralOffset = Mathf.Clamp(offset, -maxLateralOffset, maxLateralOffset);
    }
    
    /// <summary>
    /// Get current lateral offset
    /// </summary>
    public float GetLateralOffset()
    {
        return smoothedLateralOffset;
    }
    
    /// <summary>
    /// Get current position on path (0-1)
    /// </summary>
    public double GetPathPercent()
    {
        return _result.percent;
    }
    
    /// <summary>
    /// Get current segment
    /// </summary>
    public LevelSegment GetCurrentSegment()
    {
        return _segment;
    }
    
    /// <summary>
    /// Get path forward direction
    /// </summary>
    public Vector3 GetPathForward()
    {
        return _result.forward;
    }
    
    /// <summary>
    /// Get path up direction
    /// </summary>
    public Vector3 GetPathUp()
    {
        return _result.up;
    }
    
    /// <summary>
    /// Get path right direction
    /// </summary>
    public Vector3 GetPathRight()
    {
        return _result.right;
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying) return;
        
        // Draw current position on path
        Gizmos.color = pathColor;
        Gizmos.DrawWireSphere(_result.position, 0.5f);
        Gizmos.DrawLine(transform.position, _result.position);
        
        // Draw forward direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(_result.position, _result.forward * 3f);
        
        // Draw offset position
        Vector3 offsetPos = _result.position + _result.right * smoothedLateralOffset + _result.up * heightOffset;
        Gizmos.color = targetColor;
        Gizmos.DrawWireSphere(offsetPos, 0.3f);
        
        // Draw lateral offset range
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Vector3 leftLimit = _result.position + _result.right * -maxLateralOffset;
        Vector3 rightLimit = _result.position + _result.right * maxLateralOffset;
        Gizmos.DrawLine(leftLimit, rightLimit);
    }
#endif
}
