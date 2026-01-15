using UnityEngine;
using Dreamteck.Forever;
using Dreamteck.Splines;

/// <summary>
/// CharacterPathFollower - Makes the character follow Forever's generated path
/// Simplified version - just follows the path smoothly without extra movement
/// </summary>
[RequireComponent(typeof(Character))]
public class CharacterPathFollower : Runner
{
    [Header("Path Following Settings")]
    [Tooltip("Speed of movement along the path")]
    public float pathSpeed = 10f;

    [Tooltip("Base height offset above the path")]
    public float heightOffset = 2f;

    [Header("Control Settings")]
    public float horizontalSpeed = 8f;
    public float verticalSpeed = 8f;
    public float maxHorizontalOffset = 5f;
    public float maxVerticalOffset = 4f;
    public float movementSmoothTime = 0.1f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public Color pathColor = Color.green;

    // References
    private Character character;
    private Vector2 currentOffset;
    private Vector2 currentVelocity; // For SmoothDamp

    protected override void Awake()
    {
        base.Awake();
        character = GetComponent<Character>();

        // Set initial offset
        currentOffset = new Vector2(0f, 0f); // Relative to heightOffset

        // Configure Runner settings for Transform-based movement
        physicsMode = PhysicsMode.Transform;
        updateMode = UpdateMode.Update;
        startMode = StartMode.Project; // Start by projecting character's current position
        follow = false; // Don't auto-follow - wait for initialization
        isPlayer = true;

        // Set initial speed
        followSpeed = pathSpeed;

        // Configure motion module for smooth following
        _motion.applyPosition = true;    // Apply position from path
        _motion.applyRotation = true;    // Apply rotation from path
        _motion.rotationOffset = Vector3.zero;

        // Set height offset
        _motion.offset = new Vector2(0f, heightOffset);
        _motion.useSplineSizes = false;

        Debug.Log("CharacterPathFollower: Initialized and waiting to start...");
    }

    /// <summary>
    /// Start following the path (called by GameStartManager or Character)
    /// </summary>
    public void StartFollowing()
    {
        follow = true;
        followSpeed = pathSpeed;
        Debug.Log("✅ CharacterPathFollower: Started following path!");
    }

    /// <summary>
    /// Stop following the path
    /// </summary>
    public void StopFollowing()
    {
        follow = false;
        Debug.Log("⏸️ CharacterPathFollower: Stopped following path");
    }

    /// <summary>
    /// Set the movement speed
    /// </summary>
    public void SetSpeed(float speed)
    {
        followSpeed = speed;
        pathSpeed = speed;
    }

    /// <summary>
    /// Get current speed
    /// </summary>
    public float GetSpeed()
    {
        return followSpeed;
    }

    protected override void Update()
    {
        base.Update(); // Call Runner's update

        // Sync speed
        followSpeed = pathSpeed;
    }

    /// <summary>
    /// Override OnFollow to customize path following behavior
    /// </summary>
    protected override void OnFollow(SplineSample followResult)
    {
        if (character != null && follow)
        {
            // Read input from Character (populated by State Machine)
            Vector3 input = character.inputDirection;
            
            // Calculate target offset
            // Note: input.y drives vertical offset relative to heightOffset
            float targetX = Mathf.Clamp(currentOffset.x + input.x * horizontalSpeed * Time.deltaTime, -maxHorizontalOffset, maxHorizontalOffset);
            float targetY = Mathf.Clamp(currentOffset.y + input.y * verticalSpeed * Time.deltaTime, -maxVerticalOffset, maxVerticalOffset);
            
            // Apply smoothing if desired, or direct control
            if (movementSmoothTime > 0)
            {
                currentOffset.x = Mathf.SmoothDamp(currentOffset.x, targetX, ref currentVelocity.x, movementSmoothTime);
                currentOffset.y = Mathf.SmoothDamp(currentOffset.y, targetY, ref currentVelocity.y, movementSmoothTime);
            }
            else
            {
                currentOffset.x = targetX;
                currentOffset.y = targetY;
            }
            
            // Apply variables to motion module
            // X is horizontal (lateral) offset
            // Y is vertical offset (HeightOffset + CurrentYInput)
            _motion.offset = new Vector2(currentOffset.x, heightOffset + currentOffset.y);
        }
        else
        {
            // Just apply static height if not following or paused
            _motion.offset = new Vector2(currentOffset.x, heightOffset + currentOffset.y);
        }

        // Apply motion using Runner's built-in system
        base.OnFollow(followResult);
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

    /// <summary>
    /// Get current path position
    /// </summary>
    public Vector3 GetPathPosition()
    {
        return _result.position;
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

        // Draw height offset position
        Vector3 offsetPos = _result.position + _result.up * heightOffset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(offsetPos, 0.3f);
    }
#endif
}
