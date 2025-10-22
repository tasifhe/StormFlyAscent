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
    
    private AnimancerComponent animancer;
    private Animator animator;
    private AnimancerState currentState;
    
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
        
        if (useAnimancer && animancer == null)
        {
            Debug.LogError("AnimancerComponent not found on " + gameObject.name);
        }
        
        if (!useAnimancer && animator == null)
        {
            Debug.LogError("Animator not found on " + gameObject.name);
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
    
    #region Public Animation Methods
    
    public void PlayFlying(float fadeDuration = -1f)
    {
        if (useAnimancer)
        {
            PlayAnimation(animationData.flyingAnimation, AnimationState.Flying, fadeDuration);
        }
        else
        {
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
            SetAnimatorState("Flying", false);
            SetAnimatorState("Diving", false);
            SetAnimatorState("Gliding", true);
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
        }
    }
    
    private void SetAnimatorTrigger(string triggerName)
    {
        if (animator != null && animator.isActiveAndEnabled)
        {
            animator.SetTrigger(triggerName);
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
