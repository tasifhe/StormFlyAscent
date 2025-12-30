using UnityEngine;
using System;

/// <summary>
/// Manages automatic transitions between flying and gliding states
/// Handles timing logic for realistic flight patterns
/// </summary>
public class FlightStateManager
{
    // Configuration
    private float flyingDuration;
    private float glidingDuration;
    
    // State
    private bool isGliding = false;
    private float stateTimer = 0f;
    
    // Events for state changes
    public event Action OnStartFlying;
    public event Action OnStartGliding;
    
    public FlightStateManager(float flyingDuration, float glidingDuration)
    {
        this.flyingDuration = flyingDuration;
        this.glidingDuration = glidingDuration;
    }
    
    /// <summary>
    /// Is the bird currently gliding?
    /// </summary>
    public bool IsGliding => isGliding;
    
    /// <summary>
    /// Is the bird currently flying (flapping)?
    /// </summary>
    public bool IsFlying => !isGliding;
    
    /// <summary>
    /// Get current state timer
    /// </summary>
    public float StateTimer => stateTimer;
    
    /// <summary>
    /// Update state transitions
    /// Call this in Update (or LogicUpdate)
    /// </summary>
    /// <param name="shouldPause">If true, timer won't increment (e.g., during boost)</param>
    public void UpdateState(bool shouldPause = false)
    {
        if (shouldPause)
        {
            return;
        }
        
        stateTimer += Time.deltaTime;
        
        if (isGliding)
        {
            // Currently gliding - check if it's time to start flapping again
            if (stateTimer >= glidingDuration)
            {
                TransitionToFlying();
            }
        }
        else
        {
            // Currently flying - check if it's time to glide
            if (stateTimer >= flyingDuration)
            {
                TransitionToGliding();
            }
        }
    }
    
    /// <summary>
    /// Force transition to flying state
    /// </summary>
    public void TransitionToFlying()
    {
        if (isGliding)
        {
            isGliding = false;
            stateTimer = 0f;
            OnStartFlying?.Invoke();
            Debug.Log("Switching from Gliding to Flying");
        }
    }
    
    /// <summary>
    /// Force transition to gliding state
    /// </summary>
    public void TransitionToGliding()
    {
        if (!isGliding)
        {
            isGliding = true;
            stateTimer = 0f;
            OnStartGliding?.Invoke();
            Debug.Log("Switching from Flying to Gliding");
        }
    }
    
    /// <summary>
    /// Reset state to flying (useful when entering flight state)
    /// </summary>
    public void Reset()
    {
        isGliding = false;
        stateTimer = 0f;
    }
    
    /// <summary>
    /// Update configuration values
    /// </summary>
    public void UpdateConfig(float newFlyingDuration, float newGlidingDuration)
    {
        flyingDuration = newFlyingDuration;
        glidingDuration = newGlidingDuration;
    }
    
    /// <summary>
    /// Get the current flight mode for speed controller
    /// </summary>
    public FlightSpeedController.FlightMode GetCurrentMode()
    {
        return isGliding 
            ? FlightSpeedController.FlightMode.Gliding 
            : FlightSpeedController.FlightMode.Normal;
    }
}
