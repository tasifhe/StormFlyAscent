using UnityEngine;

/// <summary>
/// Handles lateral (left/right) movement with momentum and velocity
/// Provides smooth acceleration, deceleration, and drift mechanics
/// </summary>
public class LateralMovementController
{
    // Configuration
    private float moveSpeed;
    private float maxDistance;
    private float responsiveness;
    private float airDrag;
    
    // State
    private float currentPosition = 0f;
    private float velocity = 0f;
    
    // Reference to path follower for offset updates
    private BirdPathFollower pathFollower;
    
    public LateralMovementController(
        BirdPathFollower pathFollower,
        float moveSpeed,
        float maxDistance,
        float responsiveness,
        float airDrag)
    {
        this.pathFollower = pathFollower;
        this.moveSpeed = moveSpeed;
        this.maxDistance = maxDistance;
        this.responsiveness = responsiveness;
        this.airDrag = airDrag;
    }
    
    /// <summary>
    /// Get current lateral position
    /// </summary>
    public float CurrentPosition => currentPosition;
    
    /// <summary>
    /// Get current lateral velocity
    /// </summary>
    public float Velocity => velocity;
    
    /// <summary>
    /// Update lateral movement based on input
    /// Call this in FixedUpdate
    /// </summary>
    /// <param name="horizontalInput">Input value (-1 to 1)</param>
    public void UpdateMovement(float horizontalInput)
    {
        if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            // Calculate desired velocity
            float targetVelocity = horizontalInput * moveSpeed;
            
            // Smoothly accelerate towards target velocity
            velocity = Mathf.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * responsiveness);
            
            // Apply movement
            currentPosition += velocity * Time.fixedDeltaTime;
            currentPosition = Mathf.Clamp(currentPosition, -maxDistance, maxDistance);
        }
        else
        {
            // No input - apply smooth deceleration using air drag
            velocity = Mathf.Lerp(velocity, 0f, Time.fixedDeltaTime * airDrag * 2f);
            
            // Continue drifting with remaining momentum
            currentPosition += velocity * Time.fixedDeltaTime;
            currentPosition = Mathf.Clamp(currentPosition, -maxDistance, maxDistance);
        }
        
        // Update path follower offset
        if (pathFollower != null)
        {
            pathFollower.SetLateralOffset(currentPosition);
        }
    }
    
    /// <summary>
    /// Reset position and velocity (useful when entering state)
    /// </summary>
    public void Reset()
    {
        currentPosition = 0f;
        velocity = 0f;
    }
    
    /// <summary>
    /// Set new configuration values
    /// </summary>
    public void UpdateConfig(float newMoveSpeed, float newMaxDistance, float newResponsiveness, float newAirDrag)
    {
        moveSpeed = newMoveSpeed;
        maxDistance = newMaxDistance;
        responsiveness = newResponsiveness;
        airDrag = newAirDrag;
    }
}
