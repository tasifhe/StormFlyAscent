using UnityEngine;
using System.Collections;

/// <summary>
/// Simple UI panel animation controller
/// </summary>
public class UIPanelAnimator : MonoBehaviour
{
    [Header("Animation Type")]
    [SerializeField] private AnimationType animationType = AnimationType.Scale;
    
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool playOnEnable = true;
    
    [Header("Scale Animation")]
    [SerializeField] private Vector3 startScale = Vector3.zero;
    [SerializeField] private Vector3 endScale = Vector3.one;
    
    [Header("Fade Animation")]
    [SerializeField] private float startAlpha = 0f;
    [SerializeField] private float endAlpha = 1f;
    
    [Header("Slide Animation")]
    [SerializeField] private Vector2 slideOffset = new Vector2(0, 100);
    
    public enum AnimationType
    {
        Scale,
        Fade,
        Slide,
        ScaleAndFade
    }
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Coroutine currentAnimation;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null && (animationType == AnimationType.Fade || animationType == AnimationType.ScaleAndFade))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        if (rectTransform != null)
        {
            originalPosition = rectTransform.anchoredPosition;
        }
    }
    
    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlayShowAnimation();
        }
    }
    
    /// <summary>
    /// Play the show animation
    /// </summary>
    public void PlayShowAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        gameObject.SetActive(true);
        currentAnimation = StartCoroutine(AnimateShow());
    }
    
    /// <summary>
    /// Play the hide animation
    /// </summary>
    public void PlayHideAnimation()
    {
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        
        currentAnimation = StartCoroutine(AnimateHide());
    }
    
    private IEnumerator AnimateShow()
    {
        float elapsed = 0f;
        
        // Set initial state
        switch (animationType)
        {
            case AnimationType.Scale:
                transform.localScale = startScale;
                break;
            case AnimationType.Fade:
                if (canvasGroup != null)
                    canvasGroup.alpha = startAlpha;
                break;
            case AnimationType.Slide:
                if (rectTransform != null)
                    rectTransform.anchoredPosition = originalPosition + slideOffset;
                break;
            case AnimationType.ScaleAndFade:
                transform.localScale = startScale;
                if (canvasGroup != null)
                    canvasGroup.alpha = startAlpha;
                break;
        }
        
        // Animate
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);
            
            switch (animationType)
            {
                case AnimationType.Scale:
                    transform.localScale = Vector3.Lerp(startScale, endScale, t);
                    break;
                case AnimationType.Fade:
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                    break;
                case AnimationType.Slide:
                    if (rectTransform != null)
                        rectTransform.anchoredPosition = Vector2.Lerp(originalPosition + slideOffset, originalPosition, t);
                    break;
                case AnimationType.ScaleAndFade:
                    transform.localScale = Vector3.Lerp(startScale, endScale, t);
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                    break;
            }
            
            yield return null;
        }
        
        // Set final state
        switch (animationType)
        {
            case AnimationType.Scale:
                transform.localScale = endScale;
                break;
            case AnimationType.Fade:
                if (canvasGroup != null)
                    canvasGroup.alpha = endAlpha;
                break;
            case AnimationType.Slide:
                if (rectTransform != null)
                    rectTransform.anchoredPosition = originalPosition;
                break;
            case AnimationType.ScaleAndFade:
                transform.localScale = endScale;
                if (canvasGroup != null)
                    canvasGroup.alpha = endAlpha;
                break;
        }
        
        currentAnimation = null;
    }
    
    private IEnumerator AnimateHide()
    {
        float elapsed = 0f;
        
        // Animate (reverse)
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = animationCurve.Evaluate(elapsed / animationDuration);
            
            switch (animationType)
            {
                case AnimationType.Scale:
                    transform.localScale = Vector3.Lerp(endScale, startScale, t);
                    break;
                case AnimationType.Fade:
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(endAlpha, startAlpha, t);
                    break;
                case AnimationType.Slide:
                    if (rectTransform != null)
                        rectTransform.anchoredPosition = Vector2.Lerp(originalPosition, originalPosition + slideOffset, t);
                    break;
                case AnimationType.ScaleAndFade:
                    transform.localScale = Vector3.Lerp(endScale, startScale, t);
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(endAlpha, startAlpha, t);
                    break;
            }
            
            yield return null;
        }
        
        // Set final state and disable
        gameObject.SetActive(false);
        currentAnimation = null;
    }
}
