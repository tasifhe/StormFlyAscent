using UnityEngine;

/// <summary>
/// Handles vertical (up/down) movement with momentum and velocity
/// Provides smooth acceleration, deceleration, and drift mechanics
/// </summary>
public class VerticalMovementController
{
    // Configuration
    private float verticalSpeed;
    private float maxOffset;
    private float responsiveness;
    private float airDrag;
    
    // State
    private float currentOffset = 0f;
    private float velocity = 0f;
    
    // Reference to path follower for offset updates
    private BirdPathFollower pathFollower;
    
    public VerticalMovementController(
        BirdPathFollower pathFollower,
        float verticalSpeed,
        float maxOffset,
        float responsiveness,
        float airDrag)
    {
        this.pathFollower = pathFollower;
        this.verticalSpeed = verticalSpeed;
        this.maxOffset = maxOffset;
        this.responsiveness = responsiveness;
        this.airDrag = airDrag;
    }
    
    /// <summary>
    /// Get current vertical offset
    /// </summary>
    public float CurrentOffset => currentOffset;
    
    /// <summary>
    /// Get current vertical velocity
    /// </summary>
    public float Velocity => velocity;
    
    /// <summary>
    /// Update vertical movement based on input
    /// Call this in FixedUpdate
    /// </summary>
    /// <param name="verticalInput">Input value (-1 to 1)</param>
    public void UpdateMovement(float verticalInput)
    {
        if (Mathf.Abs(verticalInput) > 0.01f)
        {
            // Calculate desired velocity
            float targetVelocity = verticalInput * verticalSpeed;
            
            // Smoothly accelerate towards target velocity
            velocity = Mathf.Lerp(velocity, targetVelocity, Time.fixedDeltaTime * responsiveness);
            
            // Apply movement
            currentOffset += velocity * Time.fixedDeltaTime;
            currentOffset = Mathf.Clamp(currentOffset, -maxOffset, maxOffset);
        }
        else
        {
            // No input - apply smooth deceleration using air drag
            velocity = Mathf.Lerp(velocity, 0f, Time.fixedDeltaTime * airDrag * 2f);
            
            // Continue drifting with remaining momentum
            currentOffset += velocity * Time.fixedDeltaTime;
            currentOffset = Mathf.Clamp(currentOffset, -maxOffset, maxOffset);
        }
        
        // Update path follower offset
        if (pathFollower != null)
        {
            pathFollower.heightOffset = currentOffset;
        }
    }
    
    /// <summary>
    /// Reset offset and velocity (useful when entering state)
    /// </summary>
    public void Reset()
    {
        currentOffset = 0f;
        velocity = 0f;
    }
    
    /// <summary>
    /// Set new configuration values
    /// </summary>
    public void UpdateConfig(float newVerticalSpeed, float newMaxOffset, float newResponsiveness, float newAirDrag)
    {
        verticalSpeed = newVerticalSpeed;
        maxOffset = newMaxOffset;
        responsiveness = newResponsiveness;
        airDrag = newAirDrag;
    }
}
