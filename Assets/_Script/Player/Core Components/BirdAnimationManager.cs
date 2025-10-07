// using UnityEngine;
// using Animancer;

// [System.Serializable]
// public class BirdAnimationData
// {
//     [Header("Flying Animations")]
//     public AnimationClip flyingAnimation;
//     public AnimationClip flappingAnimation;
//     public AnimationClip glidingAnimation;
//     public AnimationClip divingAnimation;
    
//     [Header("Special Animations")]
//     public AnimationClip landingAnimation;
//     public AnimationClip crashAnimation;
//     public AnimationClip deathAnimation;
// }

// public class BirdAnimationManager : MonoBehaviour
// {
//     [Header("Animation Data")]
//     [SerializeField] private BirdAnimationData animationData;
    
//     [Header("Animation Settings")]
//     [SerializeField] private float defaultFadeDuration = 0.25f;
//     [SerializeField] private float quickFadeDuration = 0.1f;
//     [SerializeField] private float slowFadeDuration = 0.5f;
    
//     private AnimancerComponent animancer;
//     private AnimancerState currentState;
    
//     // Animation state tracking
//     public enum AnimationState
//     {
//         Flying,
//         Gliding,
//         Diving,
//         Crash,
//         Death
//     }
    
//     private AnimationState currentAnimationState;
    
//     private void Awake()
//     {
//         animancer = GetComponent<AnimancerComponent>();
//         if (animancer == null)
//         {
//             Debug.LogError("AnimancerComponent not found on " + gameObject.name);
//         }
//     }
    
//     #region Public Animation Methods
    
//     public void PlayFlying(float fadeDuration = -1f)
//     {
//         PlayAnimation(animationData.flyingAnimation, AnimationState.Flying, fadeDuration);
//     }
    
    
//     public void PlayGliding(float fadeDuration = -1f)
//     {
//         PlayAnimation(animationData.glidingAnimation, AnimationState.Gliding, fadeDuration);
//     }
    
//     public void PlayDiving(float fadeDuration = -1f)
//     {
//         PlayAnimation(animationData.divingAnimation, AnimationState.Diving, fadeDuration);
//     }
    
    
//     public void PlayCrash(float fadeDuration = -1f)
//     {
//         PlayAnimation(animationData.crashAnimation, AnimationState.Crash, fadeDuration);
//     }
    
//     public void PlayDeath(float fadeDuration = -1f)
//     {
//         PlayAnimation(animationData.deathAnimation, AnimationState.Death, fadeDuration);
//     }
    
//     #endregion
    
//     #region Private Methods
    
//     private void PlayAnimation(AnimationClip clip, AnimationState newState, float fadeDuration = -1f)
//     {
//         if (animancer == null || clip == null)
//             return;
            
//         // Use default fade duration if not specified
//         if (fadeDuration < 0f)
//             fadeDuration = GetDefaultFadeDuration(newState);
        
//         // Don't restart the same animation unless it's a looping action
//         if (currentAnimationState == newState && !ShouldRestartAnimation(newState))
//             return;
        
//         currentState = animancer.Play(clip, fadeDuration);
//         currentAnimationState = newState;
        
//         // Set animation properties based on state
//         SetAnimationProperties(newState);
//     }
    
//     private float GetDefaultFadeDuration(AnimationState state)
//     {
//         switch (state)
//         {
//             case AnimationState.Death:
//             case AnimationState.Crash:
//                 return slowFadeDuration;
//             default:
//                 return defaultFadeDuration;
//         }
//     }
    
//     private bool ShouldRestartAnimation(AnimationState state)
//     {
//         // Restart these animations even if they're already playing
//         switch (state)
//         {

//                 return true;
//             default:
//                 return false;
//         }
//     }

    
//     #endregion
    
//     #region Utility Methods
    
//     public bool IsPlaying(AnimationState state)
//     {
//         return currentAnimationState == state && currentState != null && currentState.IsPlaying;
//     }
    
//     public AnimationState GetCurrentAnimationState()
//     {
//         return currentAnimationState;
//     }
    
//     public float GetCurrentAnimationTime()
//     {
//         return currentState?.Time ?? 0f;
//     }
    
//     public float GetCurrentAnimationLength()
//     {
//         return currentState?.Length ?? 0f;
//     }
    
//     public void SetAnimationSpeed(float speed)
//     {
//         if (currentState != null)
//             currentState.Speed = speed;
//     }
    
//     public void StopAllAnimations()
//     {
//         if (animancer != null)
//             animancer.Stop();
//     }
    
//     #endregion
    
//     #region Public Properties for Inspector Access
    
//     public BirdAnimationData AnimationData
//     {
//         get { return animationData; }
//         set { animationData = value; }
//     }
    
//     #endregion
// }