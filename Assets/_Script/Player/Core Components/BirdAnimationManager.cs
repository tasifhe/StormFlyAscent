using UnityEngine;
using Animancer;

[System.Serializable]
public class BirdAnimationData
{
    [Header("Flying Animations")]
    public AnimationClip flyingAnimation;
    public AnimationClip flappingAnimation;
    public AnimationClip glidingAnimation;
    public AnimationClip divingAnimation;
    
    [Header("Special Animations")]
    public AnimationClip landingAnimation;
    public AnimationClip crashAnimation;
    public AnimationClip deathAnimation;
}

public class BirdAnimationManager : MonoBehaviour
{
    [Header("Animation Mode")]
    [Tooltip("Use Animancer for code-driven animations, or Animator for traditional state machine")]
    [SerializeField] private bool useAnimancer = false;
    
    [Header("Animation Data")]
    [SerializeField] private BirdAnimationData animationData;
    
    [Header("Animation Settings")]
    [SerializeField] private float defaultFadeDuration = 0.25f;
    [SerializeField] private float quickFadeDuration = 0.1f;
    [SerializeField] private float slowFadeDuration = 0.5f;
    
    [Header("Speed-Based Animation")]
    [Tooltip("Adjust animation speed based on bird velocity (AC-style)")]
    [SerializeField] private bool useSpeedScaling = true;
    [SerializeField] private float minAnimationSpeed = 0.8f;
    [SerializeField] private float maxAnimationSpeed = 1.5f;
    
    private AnimancerComponent animancer;
    private Animator animator;
    private AnimancerState currentState;
    
    // Speed tracking for animation
    private Character character;
    private float currentAnimationSpeed = 1f;
    
    // Animation state tracking
    public enum AnimationState
    {
        Flying,
        Gliding,
        Diving,
        Crash,
        Death
    }
    
    private AnimationState currentAnimationState;
    
    private void Awake()
    {
        // Try to get both components
        animancer = GetComponent<AnimancerComponent>();
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
        
        if (useAnimancer && animancer == null)
        {
            Debug.LogError("AnimancerComponent not found on " + gameObject.name);
        }
        
        if (!useAnimancer && animator == null)
        {
            Debug.LogError("❌ Animator not found on " + gameObject.name);
        }
        else if (!useAnimancer && animator != null)
        {
            Debug.Log($"✓ Animator found on {gameObject.name} - Mode: {(useAnimancer ? "Animancer" : "Animator")}");
        }
    }
    
    private void Start()
    {
        // Start with flying animation immediately
        if (useAnimancer && animationData.flyingAnimation != null)
        {
            PlayFlying(0f); // 0 fade = instant start
            Debug.Log("Starting with flying animation (Animancer)");
        }
        else if (!useAnimancer && animator != null)
        {
            // Set Animator to flying state
            SetAnimatorState("Flying", true);
            Debug.Log("Starting with flying animation (Animator)");
        }
    }
    
    private void Update()
    {
        // Update animation speed based on bird velocity (AC-style)
        if (useSpeedScaling && character != null && character.rb != null)
        {
            float speed = character.rb.linearVelocity.magnitude;
            float normalizedSpeed = Mathf.InverseLerp(5f, 20f, speed); // Map speed range
            currentAnimationSpeed = Mathf.Lerp(minAnimationSpeed, maxAnimationSpeed, normalizedSpeed);
            
            // Apply animation speed
            if (useAnimancer && currentState != null)
            {
                currentState.Speed = currentAnimationSpeed;
            }
            else if (animator != null)
            {
                animator.speed = currentAnimationSpeed;
            }
        }
    }
    
    #region Public Animation Methods
    
    public void PlayFlying(float fadeDuration = -1f)
    {
        if (useAnimancer)
        {
            PlayAnimation(animationData.flyingAnimation, AnimationState.Flying, fadeDuration);
        }
        else
        {
            Debug.Log("PlayFlying called - Setting Animator parameters");
            SetAnimatorState("Flying", true);
            SetAnimatorState("Diving", false);
            SetAnimatorState("Gliding", false);
        }
    }
    
    public void PlayGliding(float fadeDuration = -1f)
    {
        if (useAnimancer)
        {
            PlayAnimation(animationData.glidingAnimation, AnimationState.Gliding, fadeDuration);
        }
        else
        {
            Debug.Log("PlayGliding called - Setting Animator parameters");
            SetAnimatorState("Flying", false);
            SetAnimatorState("Diving", false);
            SetAnimatorState("Gliding", true);
        }
    }
    
    /// <summary>
    /// Play flapping animation (for boost)
    /// </summary>
    public void PlayFlapping(float fadeDuration = -1f)
    {
        Debug.Log($"🦅 PlayFlapping called! useAnimancer: {useAnimancer}, animator: {(animator != null ? "Found" : "NULL")}");
        
        if (useAnimancer)
        {
            // Use flapping animation if available, otherwise use flying
            AnimationClip clipToPlay = animationData.flappingAnimation != null 
                ? animationData.flappingAnimation 
                : animationData.flyingAnimation;
            PlayAnimation(clipToPlay, AnimationState.Flying, fadeDuration);
        }
        else
        {
            // For Animator: ONLY trigger the flap - don't change bool states
            Debug.Log("📢 Triggering Flap animation via Animator");
            
            // Check if animator has the parameter
            if (animator != null)
            {
                bool hasFlap = false;
                foreach (var param in animator.parameters)
                {
                    if (param.name == "Flap")
                    {
                        hasFlap = true;
                        Debug.Log("✓ 'Flap' trigger parameter found in Animator");
                        break;
                    }
                }
                
                if (!hasFlap)
                {
                    Debug.LogError("❌ 'Flap' trigger parameter NOT FOUND in Animator! Please add it.");
                    Debug.LogError("   Go to Animator window → Parameters → + → Trigger → Name it 'Flap'");
                }
                else
                {
                    // ONLY set the trigger - let the Animator handle the transition
                    SetAnimatorTrigger("Flap");
                }
            }
            
            // DON'T set bool states here - they might interfere with the trigger
            // The Flap trigger should handle the transition to Flapping state
        }
    }
    
    public void PlayDiving(float fadeDuration = -1f)
    {
        if (useAnimancer)
        {
            PlayAnimation(animationData.divingAnimation, AnimationState.Diving, fadeDuration);
        }
        else
        {
            Debug.Log("PlayDiving called - Setting Animator parameters");
            SetAnimatorState("Flying", false);
            SetAnimatorState("Diving", true);
            SetAnimatorState("Gliding", false);
        }
    }
    
    public void PlayCrash(float fadeDuration = -1f)
    {
        if (useAnimancer)
        {
            PlayAnimation(animationData.crashAnimation, AnimationState.Crash, fadeDuration);
        }
        else
        {
            SetAnimatorTrigger("Crash");
        }
    }
    
    public void PlayDeath(float fadeDuration = -1f)
    {
        if (useAnimancer)
        {
            PlayAnimation(animationData.deathAnimation, AnimationState.Death, fadeDuration);
        }
        else
        {
            SetAnimatorTrigger("Death");
        }
    }
    
    #endregion
    
    #region Private Methods - Animancer
    
    private void PlayAnimation(AnimationClip clip, AnimationState newState, float fadeDuration = -1f)
    {
        if (!useAnimancer || animancer == null || clip == null)
            return;
            
        // Use default fade duration if not specified
        if (fadeDuration < 0f)
            fadeDuration = GetDefaultFadeDuration(newState);
        
        // Don't restart the same animation unless it's a looping action
        if (currentAnimationState == newState && !ShouldRestartAnimation(newState))
            return;
        
        currentState = animancer.Play(clip, fadeDuration);
        currentAnimationState = newState;
        
        Debug.Log($"Playing animation: {newState} - Clip: {clip.name} - Fade: {fadeDuration}s");
        
        // Set animation properties based on state
        SetAnimationProperties(newState);
    }
    
    private float GetDefaultFadeDuration(AnimationState state)
    {
        switch (state)
        {
            case AnimationState.Death:
            case AnimationState.Crash:
                return slowFadeDuration;
            default:
                return defaultFadeDuration;
        }
    }
    
    private bool ShouldRestartAnimation(AnimationState state)
    {
        // For now, don't restart animations automatically
        // Add specific cases here if needed
        return false;
    }
    
    private void SetAnimationProperties(AnimationState state)
    {
        if (currentState == null) return;
        
        // Note: IsLooping is read-only in newer Animancer versions
        // Set the looping property on the AnimationClip itself in the Inspector
        // Animation properties are handled by the clip settings
    }
    
    #endregion
    
    #region Private Methods - Animator
    
    private void SetAnimatorState(string paramName, bool value)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetBool(paramName, value);
            Debug.Log($"Animator parameter set: {paramName} = {value}");
        }
        else
        {
            Debug.LogWarning($"Cannot set animator parameter {paramName} - Animator is null or inactive!");
        }
    }
    
    private void SetAnimatorTrigger(string triggerName)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            // Log current state
            var currentStateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"📊 Current Animator State: {currentStateInfo.shortNameHash} (normalized time: {currentStateInfo.normalizedTime})");
            
            // Reset trigger first to ensure it can be triggered again
            animator.ResetTrigger(triggerName);
            // Now set the trigger
            animator.SetTrigger(triggerName);
            Debug.Log($"✓ Animator Trigger SET: {triggerName}");
            
            // Log all active parameters
            Debug.Log("Current Animator Parameters:");
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Bool)
                    Debug.Log($"   {param.name} (Bool): {animator.GetBool(param.name)}");
                else if (param.type == AnimatorControllerParameterType.Trigger)
                    Debug.Log($"   {param.name} (Trigger): {animator.GetBool(param.name)}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ Cannot set trigger '{triggerName}' - Animator is null or inactive!");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    public bool IsPlaying(AnimationState state)
    {
        if (useAnimancer)
        {
            return currentAnimationState == state && currentState != null && currentState.IsPlaying;
        }
        else
        {
            return currentAnimationState == state;
        }
    }
    
    public AnimationState GetCurrentAnimationState()
    {
        return currentAnimationState;
    }
    
    public float GetCurrentAnimationTime()
    {
        if (useAnimancer)
        {
            return currentState?.Time ?? 0f;
        }
        return 0f;
    }
    
    public float GetCurrentAnimationLength()
    {
        if (useAnimancer)
        {
            return currentState?.Length ?? 0f;
        }
        return 0f;
    }
    
    public void SetAnimationSpeed(float speed)
    {
        if (useAnimancer && currentState != null)
        {
            currentState.Speed = speed;
        }
        else if (animator != null)
        {
            animator.speed = speed;
        }
    }
    
    public void StopAllAnimations()
    {
        if (useAnimancer && animancer != null)
        {
            animancer.Stop();
        }
    }
    
    #endregion
    
    #region Public Properties for Inspector Access
    
    public BirdAnimationData AnimationData
    {
        get { return animationData; }
        set { animationData = value; }
    }
    
    #endregion
}
