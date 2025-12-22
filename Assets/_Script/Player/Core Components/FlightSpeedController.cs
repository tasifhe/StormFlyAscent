using UnityEngine;

/// <summary>
/// Manages speed multipliers for different flight states
/// Separates speed logic from state logic
/// </summary>
public class FlightSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float normalSpeedMultiplier = 1f;
    [SerializeField] private float glideSpeedMultiplier = 0.85f;
    [SerializeField] private float diveSpeedMultiplier = 1.5f;
    
    [Header("Altitude Influence")]
    [SerializeField] private float altitudeSpeedInfluence = 1f;
    
    private Character character;
    private BirdPathFollower pathFollower;
    private BoostSystem boostSystem;
    
    private float currentSpeedMultiplier = 1f;
    private float previousAltitude = 0f;
    
    public enum FlightMode
    {
        Normal,
        Gliding,
        Diving
    }
    
    private FlightMode currentMode = FlightMode.Normal;
    
    private void Awake()
    {
        character = GetComponent<Character>();
        pathFollower = GetComponent<BirdPathFollower>();
        boostSystem = GetComponent<BoostSystem>();
    }
    
    private void Start()
    {
        previousAltitude = transform.position.y;
    }
    
    /// <summary>
    /// Update speed based on current flight mode
    /// Call this from state's LogicUpdate
    /// </summary>
    public void UpdateSpeed(FlightMode mode)
    {
        currentMode = mode;
        
        // Calculate altitude influence
        float currentAltitude = transform.position.y;
        float altitudeChange = (currentAltitude - previousAltitude) / Time.deltaTime;
        previousAltitude = currentAltitude;
        
        // Get base multiplier for current mode
        float targetMultiplier = GetSpeedMultiplierForMode(mode);
        
        // Add altitude influence (descending = faster, climbing = slower)
        float altitudeSpeedMod = -altitudeChange * altitudeSpeedInfluence * 0.01f;
        targetMultiplier += altitudeSpeedMod;
        
        // Get boost multiplier if boosting
        if (boostSystem != null && boostSystem.IsBoosting)
        {
            targetMultiplier = boostSystem.GetCurrentSpeedMultiplier();
        }
        
        // Clamp to reasonable range
        targetMultiplier = Mathf.Clamp(targetMultiplier, 0.5f, 5.0f);
        
        // Apply instantly if boosting, smoothly otherwise
        if (boostSystem != null && boostSystem.IsBoosting)
        {
            currentSpeedMultiplier = targetMultiplier;
        }
        else
        {
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetMultiplier, Time.deltaTime * 2f);
        }
        
        // Apply to path follower
        if (pathFollower != null && character != null)
        {
            pathFollower.followSpeed = character.forwardSpeed * currentSpeedMultiplier;
        }
    }
    
    /// <summary>
    /// Get speed multiplier for a specific flight mode
    /// </summary>
    private float GetSpeedMultiplierForMode(FlightMode mode)
    {
        switch (mode)
        {
            case FlightMode.Gliding:
                return glideSpeedMultiplier;
            case FlightMode.Diving:
                return diveSpeedMultiplier;
            default:
                return normalSpeedMultiplier;
        }
    }
    
    /// <summary>
    /// Force set speed (for special cases)
    /// </summary>
    public void SetSpeed(float speed)
    {
        if (pathFollower != null)
        {
            pathFollower.followSpeed = speed;
        }
    }
    
    // Public properties
    public float CurrentSpeedMultiplier => currentSpeedMultiplier;
    public FlightMode CurrentMode => currentMode;
}
