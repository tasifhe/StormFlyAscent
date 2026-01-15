using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;

/// <summary>
/// Professional async loading screen manager with smooth animations and progress tracking
/// </summary>
public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform spinnerTransform;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private Ease fadeEase = Ease.InOutQuad;
    [SerializeField] private float progressSmoothness = 5f;
    [SerializeField] private float spinnerRotationSpeed = 360f;

    [Header("Progress Bar Settings")]
    [SerializeField] private Gradient progressBarGradient;
    [SerializeField] private Image progressBarFill;
    [SerializeField] private bool animateProgressBarColor = true;

    [Header("Loading Tips")]
    [SerializeField]
    private string[] loadingTips = new string[]
    {
        "Use joystick to steer your bird left and right",
        "Tap the screen to boost your speed",
        "Collect coins to unlock new bird skins",
        "The bird alternates between flapping and gliding",
        "Avoid obstacles in the storm!",
        "Higher altitude increases your speed when diving",
        "Practice smooth movements for better control",
        "Keep an eye on upcoming obstacles",
        "Perfect timing leads to perfect flight",
        "The storm gets more intense as you climb higher"
    };

    [Header("Advanced Settings")]
    [SerializeField] private bool showDetailedProgress = true;
    [SerializeField] private float minimumDisplayTime = 1.5f;
    [SerializeField] private float completionHoldTime = 0.3f;

    // Private state
    private float currentProgress = 0f;
    private float targetProgress = 0f;
    private bool isLoading = false;
    private Coroutine spinnerCoroutine;
    private Tween progressTween;

    private static LoadingScreenManager instance;

    public static LoadingScreenManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<LoadingScreenManager>(FindObjectsInactive.Include);
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Force finding the instance even if disabled
        if (instance == null)
        {
            instance = this;
            transform.SetParent(null); // Detach from parent to ensure DontDestroyOnLoad works cleanly
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        InitializeLoadingScreen();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        // Clean up tweens
        DOTween.Kill(this);
        progressTween?.Kill();
    }

    /// <summary>
    /// Initialize loading screen components
    /// </summary>
    private void InitializeLoadingScreen()
    {
        // 1. Ensure Canvas properties for visibility
        // Since we detach from parent, we must ensure we have a Canvas on this object
        Canvas canvas = GetComponent<Canvas>();

        if (canvas == null)
        {
            // If no canvas exists on this object, add required components to make it a self-sufficient UI root
            Debug.Log("[LoadingScreenManager] Adding missing Canvas components after parent detachment");
            canvas = gameObject.AddComponent<Canvas>();

            // Add and configure Canvas Scaler if missing
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null) scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // Add Graphic Raycaster if missing
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();
        }

        if (canvas != null)
        {
            // Forces the loading screen to render on top of EVERYTHING
            canvas.sortingOrder = 32000;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // CRITICAL: Reset RectTransform to fill the screen after detaching or creating Canvas
            // When detaching a UI element, its RectTransform often keeps relative offsets that make it tiny or off-screen
            RectTransform rect = GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero; // Full stretch
                rect.localScale = Vector3.one;

                // Ensure Z position is 0 just in case
                Vector3 pos = rect.localPosition;
                pos.z = 0;
                rect.localPosition = pos;
            }
        }

        if (loadingCanvasGroup == null)
        {
            loadingCanvasGroup = GetComponent<CanvasGroup>();
            if (loadingCanvasGroup == null)
                loadingCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Start hidden but active so it can be enabled/disabled via script correctly
        loadingCanvasGroup.alpha = 0f;
        loadingCanvasGroup.blocksRaycasts = false;
        // Do NOT disable gameObject here, as it causes issues if enabled via script later
        // gameObject.SetActive(false); 

        // Initialize progress bar
        if (progressBar != null)
        {
            progressBar.value = 0f;
            progressBar.minValue = 0f;
            progressBar.maxValue = 1f;
        }

        // Setup gradient for progress bar
        if (progressBarGradient == null && progressBarFill != null)
        {
            progressBarGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(Color.red, 0f);
            colorKeys[1] = new GradientColorKey(Color.yellow, 0.5f);
            colorKeys[2] = new GradientColorKey(Color.green, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(1f, 1f);

            progressBarGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    /// <summary>
    /// Show loading screen with fade-in animation
    /// </summary>
    public async Task ShowAsync()
    {
        isLoading = true;
        gameObject.SetActive(true);

        // Get canvas group reference if needed
        if (loadingCanvasGroup == null) loadingCanvasGroup = GetComponent<CanvasGroup>();

        // Reset progress
        currentProgress = 0f;
        targetProgress = 0f;
        UpdateProgressUI(0f);

        // Show random tip
        ShowRandomTip();

        // Start spinner animation
        if (spinnerTransform != null)
        {
            if (spinnerCoroutine != null)
                StopCoroutine(spinnerCoroutine);
            spinnerCoroutine = StartCoroutine(AnimateSpinner());
        }

        // Fade in animation
        if (loadingCanvasGroup != null)
        {
            loadingCanvasGroup.blocksRaycasts = true;

            // Kill old tweens
            loadingCanvasGroup.DOKill();

            // Start from transparent and fade to visible
            loadingCanvasGroup.alpha = 0f;
            await loadingCanvasGroup.DOFade(1f, fadeInDuration)
                .SetEase(fadeEase)
                .SetUpdate(true)
                .AsyncWaitForCompletion();
        }
    }

    /// <summary>
    /// Hide loading screen with fade-out animation
    /// </summary>
    public async Task HideAsync()
    {
        if (!isLoading) return;

        // Hold at 100% for a moment
        await Task.Delay((int)(completionHoldTime * 1000));

        // Fade out
        await loadingCanvasGroup.DOFade(0f, fadeOutDuration)
            .SetEase(fadeEase)
            .SetUpdate(true)
            .AsyncWaitForCompletion();

        // Stop spinner
        if (spinnerCoroutine != null)
        {
            StopCoroutine(spinnerCoroutine);
            spinnerCoroutine = null;
        }

        loadingCanvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        isLoading = false;
    }

    /// <summary>
    /// Update loading progress (0-1)
    /// </summary>
    public void SetProgress(float progress, string status = null)
    {
        targetProgress = Mathf.Clamp01(progress);

        if (!string.IsNullOrEmpty(status))
        {
            SetStatus(status);
        }

        // Smooth progress animation
        if (progressTween != null && progressTween.IsActive())
        {
            progressTween.Kill();
        }

        progressTween = DOTween.To(
            () => currentProgress,
            x =>
            {
                currentProgress = x;
                UpdateProgressUI(x);
            },
            targetProgress,
            0.3f
        ).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    /// <summary>
    /// Set loading status text
    /// </summary>
    public void SetStatus(string status)
    {
        if (statusText != null)
        {
            statusText.text = status;

            // Animate text appearance
            statusText.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 5, 0.5f).SetUpdate(true);
        }
    }

    /// <summary>
    /// Update progress UI elements
    /// </summary>
    private void UpdateProgressUI(float progress)
    {
        // Update progress bar
        if (progressBar != null)
        {
            progressBar.value = progress;
        }

        // Update progress bar color
        if (animateProgressBarColor && progressBarFill != null && progressBarGradient != null)
        {
            progressBarFill.color = progressBarGradient.Evaluate(progress);
        }

        // Update progress text
        if (progressText != null)
        {
            int percentage = Mathf.RoundToInt(progress * 100);
            progressText.text = $"{percentage}%";
        }
    }

    /// <summary>
    /// Show random loading tip
    /// </summary>
    private void ShowRandomTip()
    {
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];

            // Animate tip appearance
            tipText.alpha = 0f;
            tipText.DOFade(1f, 0.5f).SetUpdate(true);
        }
    }

    /// <summary>
    /// Animate spinner rotation
    /// </summary>
    private IEnumerator AnimateSpinner()
    {
        if (spinnerTransform == null) yield break;

        while (true)
        {
            spinnerTransform.Rotate(0f, 0f, -spinnerRotationSpeed * Time.unscaledDeltaTime);
            yield return null;
        }
    }

    /// <summary>
    /// Complete loading process with smooth finish
    /// </summary>
    public async Task CompleteLoadingAsync()
    {
        // Ensure we reach 100%
        SetProgress(1f, "Complete!");
        await Task.Delay(300);

        // Hide the loading screen
        await HideAsync();
    }

    #region Utility Methods

    /// <summary>
    /// Check if currently loading
    /// </summary>
    public bool IsLoading => isLoading;

    /// <summary>
    /// Get current progress value
    /// </summary>
    public float CurrentProgress => currentProgress;

    #endregion
}
