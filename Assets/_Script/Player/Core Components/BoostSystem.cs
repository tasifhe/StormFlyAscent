using UnityEngine;

/// <summary>
/// Handles boost mechanics independently of flight states
/// Can be used by any state (Flying, Gliding, Diving, etc.)
/// MOBILE OPTIMIZED: Quality settings, performance modes
/// </summary>
public class BoostSystem : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostSpeedMultiplier = 3.5f;
    [SerializeField] private float boostDuration = 0.5f;
    [SerializeField] private float boostCooldown = 0.8f;
    [SerializeField] private float boostUpwardLift = 25f;
    
    [Header("Mobile Performance")]
    [Tooltip("Reduce visual effects on low-end devices")]
    public bool lowPerformanceMode = false;
    
    [Tooltip("Disable physics forces in low performance mode")]
    public bool disablePhysicsInLowMode = false;
    
    [Tooltip("Auto-detect device performance and adjust")]
    public bool autoDetectPerformance = true;
    
    [Header("References")]
    private Character character;
    private BirdPathFollower pathFollower;
    private BirdAnimationManager animationManager;
    private BoostVisualEffect visualEffect;
    
    // Boost state
    private bool isBoosting = false;
    private float boostTimer = 0f;
    private float cooldownTimer = 0f;
    private float baseSpeed = 10f;
    
    // Events
    public event System.Action OnBoostStarted;
    public event System.Action OnBoostEnded;
    
    private void Awake()
    {
        character = GetComponent<Character>();
        pathFollower = GetComponent<BirdPathFollower>();
        animationManager = GetComponent<BirdAnimationManager>();
        visualEffect = GetComponent<BoostVisualEffect>();
        
        // Auto-detect performance level
        if (autoDetectPerformance)
        {
            DetectDevicePerformance();
        }
    }
    
    /// <summary>
    /// Auto-detect device performance and adjust settings
    /// </summary>
    private void DetectDevicePerformance()
    {
        // Check system memory
        int systemMemoryMB = SystemInfo.systemMemorySize;
        
        // Check graphics memory
        int graphicsMemoryMB = SystemInfo.graphicsMemorySize;
        
        // Check processor count
        int processorCount = SystemInfo.processorCount;
        
        // Determine performance tier
        bool isLowEnd = systemMemoryMB < 2048 || graphicsMemoryMB < 512 || processorCount < 4;
        
        if (isLowEnd)
        {
            lowPerformanceMode = true;
            disablePhysicsInLowMode = true;
            Debug.Log($"📱 LOW-END DEVICE DETECTED:\n" +
                      $"   RAM: {systemMemoryMB}MB\n" +
                      $"   VRAM: {graphicsMemoryMB}MB\n" +
                      $"   CPU Cores: {processorCount}\n" +
                      $"   => Low performance mode ENABLED");
        }
        else
        {
            Debug.Log($"📱 DEVICE PERFORMANCE:\n" +
                      $"   RAM: {systemMemoryMB}MB\n" +
                      $"   VRAM: {graphicsMemoryMB}MB\n" +
                      $"   CPU Cores: {processorCount}\n" +
                      $"   => Normal performance mode");
        }
    }
    
    private void Update()
    {
        // Update timers
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0f)
            {
                EndBoost();
            }
        }
        
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Attempt to activate boost
    /// </summary>
    public bool TryActivateBoost()
    {
        if (cooldownTimer > 0f)
        {
            Debug.Log($"❌ Boost on cooldown! Wait {cooldownTimer:F2}s more");
            return false;
        }
        
        if (isBoosting)
        {
            Debug.Log("❌ Already boosting!");
            return false;
        }
        
        ActivateBoost();
        return true;
    }
    
    /// <summary>
    /// Force activate boost (internal)
    /// </summary>
    private void ActivateBoost()
    {
        isBoosting = true;
        boostTimer = boostDuration;
        cooldownTimer = boostCooldown;
        
        // Store base speed
        if (character != null)
        {
            baseSpeed = character.forwardSpeed;
        }
        
        // Apply speed boost
        ApplySpeedBoost();
        
        // Play animation (always play, it's cheap)
        if (animationManager != null)
        {
            animationManager.PlayFlapping(0.05f);
        }
        
        // Trigger visual effects (skip in low performance mode)
        if (!lowPerformanceMode && visualEffect != null)
        {
            visualEffect.TriggerBoost();
        }
        
        // Trigger event
        OnBoostStarted?.Invoke();
        
        Debug.Log($"🦅 BOOST ACTIVATED! {(lowPerformanceMode ? "[LOW PERF MODE]" : "")}\n" +
                  $"   Base Speed: {baseSpeed}\n" +
                  $"   Multiplier: {boostSpeedMultiplier}x\n" +
                  $"   Target Speed: {baseSpeed * boostSpeedMultiplier}\n" +
                  $"   Duration: {boostDuration}s");
    }
    
    /// <summary>
    /// Apply the speed boost to pathFollower
    /// </summary>
    private void ApplySpeedBoost()
    {
        if (pathFollower != null)
        {
            float boostedSpeed = baseSpeed * boostSpeedMultiplier;
            pathFollower.followSpeed = boostedSpeed;
        }
    }
    
    /// <summary>
    /// End the boost
    /// </summary>
    private void EndBoost()
    {
        isBoosting = false;
        
        // Reset to normal speed
        if (pathFollower != null && character != null)
        {
            pathFollower.followSpeed = character.forwardSpeed;
        }
        
        // End visual effects (skip in low performance mode)
        if (!lowPerformanceMode && visualEffect != null)
        {
            visualEffect.EndBoost();
        }
        
        // Trigger event
        OnBoostEnded?.Invoke();
        
        Debug.Log("✓ Boost ended - Returned to normal speed");
    }
    
    /// <summary>
    /// Apply upward lift force (called from FixedUpdate)
    /// Mobile Optimized: Can be disabled on low-end devices
    /// </summary>
    public void ApplyBoostPhysics()
    {
        // Skip physics in low performance mode if enabled
        if (lowPerformanceMode && disablePhysicsInLowMode)
        {
            return;
        }
        
        if (isBoosting && character != null && character.rb != null)
        {
            character.rb.AddForce(Vector3.up * boostUpwardLift, ForceMode.Force);
        }
    }
    
    /// <summary>
    /// Get current boost speed multiplier (for states to use)
    /// </summary>
    public float GetCurrentSpeedMultiplier()
    {
        return isBoosting ? boostSpeedMultiplier : 1f;
    }
    
    // Public properties
    public bool IsBoosting => isBoosting;
    public float CooldownRemaining => cooldownTimer;
    public float BoostTimeRemaining => boostTimer;
    public bool IsOnCooldown => cooldownTimer > 0f;
}
