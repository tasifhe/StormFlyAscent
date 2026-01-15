using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// Manages smooth transitions between menu panels with professional effects
/// </summary>
public class MenuTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private Image transitionOverlay;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Ease transitionEaseIn = Ease.InOutQuad;
    [SerializeField] private Ease transitionEaseOut = Ease.InOutQuad;
    
    [Header("Transition Effects")]
    [SerializeField] private TransitionType defaultTransition = TransitionType.Fade;
    [SerializeField] private Color transitionColor = Color.black;
    [SerializeField] private bool useRadialWipe = false;
    [SerializeField] private Material radialWipeMaterial;
    
    [Header("Loading Text")]
    [SerializeField] private TextMeshProUGUI transitionText;
    [SerializeField] private bool showTransitionText = true;
    [SerializeField] private string defaultTransitionMessage = "Loading...";
    
    private static MenuTransitionManager instance;
    private bool isTransitioning = false;
    
    public enum TransitionType
    {
        Fade,
        Wipe,
        Zoom,
        Slide
    }
    
    public static MenuTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MenuTransitionManager>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeTransitionOverlay();
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
        
        DOTween.Kill(this);
    }
    
    /// <summary>
    /// Initialize transition overlay
    /// </summary>
    private void InitializeTransitionOverlay()
    {
        if (transitionOverlay == null)
        {
            // Create transition overlay if not assigned
            GameObject overlayObj = new GameObject("TransitionOverlay");
            overlayObj.transform.SetParent(transform, false);
            
            Canvas canvas = overlayObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            overlayObj.AddComponent<CanvasScaler>();
            overlayObj.AddComponent<GraphicRaycaster>();
            
            GameObject imageObj = new GameObject("Overlay");
            imageObj.transform.SetParent(overlayObj.transform, false);
            
            transitionOverlay = imageObj.AddComponent<Image>();
            transitionOverlay.color = transitionColor;
            
            RectTransform rt = transitionOverlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            
            // Create text if enabled
            if (showTransitionText && transitionText == null)
            {
                GameObject textObj = new GameObject("TransitionText");
                textObj.transform.SetParent(overlayObj.transform, false);
                
                transitionText = textObj.AddComponent<TextMeshProUGUI>();
                transitionText.text = defaultTransitionMessage;
                transitionText.fontSize = 48;
                transitionText.alignment = TextAlignmentOptions.Center;
                transitionText.color = Color.white;
                
                RectTransform textRt = textObj.GetComponent<RectTransform>();
                textRt.anchorMin = new Vector2(0.5f, 0.5f);
                textRt.anchorMax = new Vector2(0.5f, 0.5f);
                textRt.sizeDelta = new Vector2(800, 100);
                textRt.anchoredPosition = Vector2.zero;
            }
        }
        
        // Start hidden
        if (transitionOverlay != null)
        {
            transitionOverlay.color = new Color(transitionColor.r, transitionColor.g, transitionColor.b, 0f);
            transitionOverlay.raycastTarget = false;
        }
        
        if (transitionText != null)
        {
            transitionText.alpha = 0f;
        }
    }
    
    #region Public API
    
    /// <summary>
    /// Execute a transition effect
    /// </summary>
    public async Task TransitionAsync(TransitionType type = TransitionType.Fade, string message = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[MenuTransitionManager] Already transitioning");
            return;
        }
        
        isTransitioning = true;
        
        // Set message
        if (showTransitionText && transitionText != null && !string.IsNullOrEmpty(message))
        {
            transitionText.text = message;
        }
        
        // Transition in
        await TransitionIn(type);
        
        // Hold briefly
        await Task.Delay(100);
        
        // Transition out
        await TransitionOut(type);
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Fade in transition
    /// </summary>
    public async Task FadeInAsync(float duration = -1f)
    {
        float dur = duration > 0 ? duration : transitionDuration;
        
        if (transitionOverlay != null)
        {
            transitionOverlay.raycastTarget = true;
            await transitionOverlay.DOFade(1f, dur)
                .SetEase(transitionEaseIn)
                .SetUpdate(true)
                .AsyncWaitForCompletion();
        }
        
        if (showTransitionText && transitionText != null)
        {
            transitionText.DOFade(1f, dur * 0.5f).SetUpdate(true);
        }
    }
    
    /// <summary>
    /// Fade out transition
    /// </summary>
    public async Task FadeOutAsync(float duration = -1f)
    {
        float dur = duration > 0 ? duration : transitionDuration;
        
        if (showTransitionText && transitionText != null)
        {
            transitionText.DOFade(0f, dur * 0.5f).SetUpdate(true);
        }
        
        if (transitionOverlay != null)
        {
            await transitionOverlay.DOFade(0f, dur)
                .SetEase(transitionEaseOut)
                .SetUpdate(true)
                .AsyncWaitForCompletion();
            
            transitionOverlay.raycastTarget = false;
        }
    }
    
    /// <summary>
    /// Quick fade (for loading screens)
    /// </summary>
    public async Task QuickFadeAsync()
    {
        await FadeInAsync(0.2f);
        await Task.Delay(50);
        await FadeOutAsync(0.2f);
    }
    
    #endregion
    
    #region Transition Effects
    
    private async Task TransitionIn(TransitionType type)
    {
        switch (type)
        {
            case TransitionType.Fade:
                await FadeInAsync();
                break;
                
            case TransitionType.Wipe:
                await WipeInAsync();
                break;
                
            case TransitionType.Zoom:
                await ZoomInAsync();
                break;
                
            case TransitionType.Slide:
                await SlideInAsync();
                break;
        }
    }
    
    private async Task TransitionOut(TransitionType type)
    {
        switch (type)
        {
            case TransitionType.Fade:
                await FadeOutAsync();
                break;
                
            case TransitionType.Wipe:
                await WipeOutAsync();
                break;
                
            case TransitionType.Zoom:
                await ZoomOutAsync();
                break;
                
            case TransitionType.Slide:
                await SlideOutAsync();
                break;
        }
    }
    
    private async Task WipeInAsync()
    {
        if (transitionOverlay == null) return;
        
        RectTransform rt = transitionOverlay.rectTransform;
        transitionOverlay.color = transitionColor;
        transitionOverlay.raycastTarget = true;
        
        // Start from left
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 1);
        rt.sizeDelta = Vector2.zero;
        
        await rt.DOAnchorMax(Vector2.one, transitionDuration)
            .SetEase(transitionEaseIn)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
    
    private async Task WipeOutAsync()
    {
        if (transitionOverlay == null) return;
        
        RectTransform rt = transitionOverlay.rectTransform;
        
        // Wipe to right
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        
        await rt.DOAnchorMin(Vector2.one, transitionDuration)
            .SetEase(transitionEaseOut)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
        
        // Reset
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        transitionOverlay.raycastTarget = false;
    }
    
    private async Task ZoomInAsync()
    {
        if (transitionOverlay == null) return;
        
        RectTransform rt = transitionOverlay.rectTransform;
        transitionOverlay.color = transitionColor;
        transitionOverlay.raycastTarget = true;
        
        rt.localScale = Vector3.zero;
        
        await rt.DOScale(Vector3.one, transitionDuration)
            .SetEase(transitionEaseIn)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
    
    private async Task ZoomOutAsync()
    {
        if (transitionOverlay == null) return;
        
        RectTransform rt = transitionOverlay.rectTransform;
        
        await rt.DOScale(Vector3.zero, transitionDuration)
            .SetEase(transitionEaseOut)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
        
        rt.localScale = Vector3.one;
        transitionOverlay.raycastTarget = false;
    }
    
    private async Task SlideInAsync()
    {
        if (transitionOverlay == null) return;
        
        RectTransform rt = transitionOverlay.rectTransform;
        transitionOverlay.color = transitionColor;
        transitionOverlay.raycastTarget = true;
        
        // Start from top
        Vector2 startPos = new Vector2(0, Screen.height);
        rt.anchoredPosition = startPos;
        
        await rt.DOAnchorPos(Vector2.zero, transitionDuration)
            .SetEase(transitionEaseIn)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
    
    private async Task SlideOutAsync()
    {
        if (transitionOverlay == null) return;
        
        RectTransform rt = transitionOverlay.rectTransform;
        
        // Slide to bottom
        Vector2 endPos = new Vector2(0, -Screen.height);
        
        await rt.DOAnchorPos(endPos, transitionDuration)
            .SetEase(transitionEaseOut)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
        
        rt.anchoredPosition = Vector2.zero;
        transitionOverlay.raycastTarget = false;
    }
    
    #endregion
    
    /// <summary>
    /// Check if currently transitioning
    /// </summary>
    public bool IsTransitioning => isTransitioning;
}