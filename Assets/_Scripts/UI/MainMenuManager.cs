using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using DG.Tweening;

/// <summary>
/// Main menu manager that controls navigation between menu panels with DOTween animations
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject customizationPanel;

    [Header("Menu Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button customizationButton;
    [SerializeField] private Button exitButton;

    [Header("DOTween Animation Settings")]
    [SerializeField] private float panelFadeDuration = 0.4f;
    [SerializeField] private float panelScaleDuration = 0.5f;
    [SerializeField] private float buttonScaleDuration = 0.2f;
    [SerializeField] private Ease panelEaseIn = Ease.OutBack;
    [SerializeField] private Ease panelEaseOut = Ease.InBack;
    [SerializeField] private Vector3 panelStartScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private float buttonHoverScale = 1.1f;

    [Header("Settings")]
    [SerializeField] private bool hideOnStart = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private AudioClip panelCloseSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 0.7f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem menuParticles;
    [SerializeField] private bool enableParticleEffects = true;

    [Header("Button Animation")]
    [SerializeField] private float buttonClickScale = 0.9f;
    [SerializeField] private float buttonClickDuration = 0.15f;

    private AudioSource audioSource;
    private MenuSceneManager sceneManager;
    private bool isTransitioning = false;

    private void Awake()
    {
        // CRITICAL: Ensure EventSystem exists for touch input to work
        EnsureEventSystem();

        // Get references
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.volume = sfxVolume;

        // Get scene manager
        sceneManager = MenuSceneManager.Instance;
        if (sceneManager == null)
        {
            sceneManager = FindFirstObjectByType<MenuSceneManager>();
        }

        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (customizationButton != null)
            customizationButton.onClick.AddListener(OnCustomizationButtonClicked);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);

        // Add hover effects to all buttons (ONLY on desktop, not mobile)
#if !UNITY_ANDROID && !UNITY_IOS
        AddButtonHoverEffect(startButton);
        AddButtonHoverEffect(settingsButton);
        AddButtonHoverEffect(customizationButton);
        AddButtonHoverEffect(exitButton);
        Debug.Log("[MainMenuManager] Desktop platform - hover effects enabled");
#else
        Debug.Log("[MainMenuManager] Mobile platform - hover effects disabled for better touch responsiveness");
#endif
    }

    private void Start()
    {
        // Initialize particle effects
        if (menuParticles != null && enableParticleEffects)
        {
            menuParticles.Play();
        }

        if (hideOnStart)
        {
            // Hide all panels initially
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (customizationPanel != null) customizationPanel.SetActive(false);
        }
        else
        {
            ShowMainMenu();
        }

        // Enable buttons
        SetButtonsInteractable(true);
    }

    private void OnDestroy()
    {
        // Kill all DOTween animations on this object to prevent memory leaks
        DOTween.Kill(this);
        if (mainMenuPanel != null) DOTween.Kill(mainMenuPanel.transform);
        if (settingsPanel != null) DOTween.Kill(settingsPanel.transform);
        if (customizationPanel != null) DOTween.Kill(customizationPanel.transform);
    }

    #region Public Methods

    /// <summary>
    /// Show the main menu (called after splash screen)
    /// </summary>
    public void ShowMainMenu()
    {
        ShowPanel(mainMenuPanel);
        HidePanel(settingsPanel);
        HidePanel(customizationPanel);

        PlaySound(panelOpenSound);
    }

    /// <summary>
    /// Hide all menu panels
    /// </summary>
    public void HideAllPanels()
    {
        HidePanel(mainMenuPanel);
        HidePanel(settingsPanel);
        HidePanel(customizationPanel);
    }

    #endregion

    #region Button Callbacks

    private void OnStartButtonClicked()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        AnimateButtonClickProfessional(startButton);
        PlaySound(buttonClickSound);
        Debug.Log("Start button clicked - Loading game...");

        // Disable buttons to prevent double-click
        SetButtonsInteractable(false);

        // Trigger particle burst
        if (menuParticles != null && enableParticleEffects)
        {
            menuParticles.Emit(20);
        }

        // Use the proper async loading system
        if (sceneManager != null)
        {
            sceneManager.LoadGameScene();
        }
        else
        {
            Debug.LogError("[MainMenuManager] MenuSceneManager not found! Cannot load game.");
            SetButtonsInteractable(true);
            isTransitioning = false;
        }
    }

    private void OnSettingsButtonClicked()
    {
        AnimateButtonClick(settingsButton);
        PlaySound(buttonClickSound);
        Debug.Log("Settings button clicked");

        ShowPanel(settingsPanel);
        HidePanel(mainMenuPanel);

        PlaySound(panelOpenSound);
    }

    private void OnCustomizationButtonClicked()
    {
        AnimateButtonClick(customizationButton);
        PlaySound(buttonClickSound);
        Debug.Log("Customization button clicked");

        ShowPanel(customizationPanel);
        HidePanel(mainMenuPanel);

        PlaySound(panelOpenSound);
    }

    private void OnExitButtonClicked()
    {
        AnimateButtonClick(exitButton);
        PlaySound(buttonClickSound);
        Debug.Log("Exit button clicked");

        // Show confirmation dialog (you can implement this later)
        ExitGame();
    }

    #endregion

    #region Navigation Methods

    /// <summary>
    /// Return to main menu from any sub-panel
    /// </summary>
    public void ReturnToMainMenu()
    {
        PlaySound(buttonClickSound);
        ShowMainMenu();
        PlaySound(panelCloseSound);
    }

    #endregion

    #region Helper Methods

    private void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);

            // Get or add CanvasGroup for fade animation
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            RectTransform rectTransform = panel.GetComponent<RectTransform>();

            // Kill any existing tweens on this panel
            DOTween.Kill(canvasGroup);
            DOTween.Kill(rectTransform);

            // Animate fade in
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, panelFadeDuration).SetEase(panelEaseIn);

            // Animate scale in (from small to normal)
            if (rectTransform != null)
            {
                rectTransform.localScale = panelStartScale;
                rectTransform.DOScale(Vector3.one, panelScaleDuration).SetEase(panelEaseIn);
            }
        }
    }

    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = panel.AddComponent<CanvasGroup>();

            RectTransform rectTransform = panel.GetComponent<RectTransform>();

            // Kill any existing tweens
            DOTween.Kill(canvasGroup);
            DOTween.Kill(rectTransform);

            // Animate fade out
            canvasGroup.DOFade(0f, panelFadeDuration).SetEase(panelEaseOut);

            // Animate scale out (from normal to small)
            if (rectTransform != null)
            {
                rectTransform.DOScale(panelStartScale, panelScaleDuration)
                    .SetEase(panelEaseOut)
                    .OnComplete(() => panel.SetActive(false));
            }
            else
            {
                // If no RectTransform, just delay deactivation
                DOVirtual.DelayedCall(panelFadeDuration, () => panel.SetActive(false));
            }
        }
    }

    /// <summary>
    /// Add button hover effects (call this for each button you want to animate)
    /// WARNING: Should NOT be called on mobile - EventTriggers interfere with touch!
    /// </summary>
    public void AddButtonHoverEffect(Button button)
    {
        if (button == null) return;

#if UNITY_ANDROID || UNITY_IOS
        Debug.LogWarning("[MainMenuManager] AddButtonHoverEffect called on mobile build! This will cause touch issues!");
        return; // Don't add hover effects on mobile
#endif

        // Add event triggers for hover (desktop only)
        UnityEngine.EventSystems.EventTrigger trigger = button.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Pointer Enter (hover)
        UnityEngine.EventSystems.EventTrigger.Entry entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { AnimateButtonHover(button, true); });
        trigger.triggers.Add(entryEnter);

        // Pointer Exit (unhover)
        UnityEngine.EventSystems.EventTrigger.Entry entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { AnimateButtonHover(button, false); });
        trigger.triggers.Add(entryExit);
    }

    private void AnimateButtonHover(Button button, bool hover)
    {
        if (button == null || !button.interactable) return;

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // Kill existing tweens
        DOTween.Kill(rectTransform);

        // Play hover sound
        if (hover && buttonHoverSound != null)
        {
            PlaySound(buttonHoverSound);
        }

        // Animate scale with elastic effect
        float targetScale = hover ? buttonHoverScale : 1f;
        rectTransform.DOScale(targetScale, buttonScaleDuration)
            .SetEase(hover ? Ease.OutElastic : Ease.InOutQuad);
    }

    /// <summary>
    /// Animate button click (scale down then up)
    /// </summary>
    public void AnimateButtonClick(Button button)
    {
        if (button == null) return;

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // Kill existing tweens
        DOTween.Kill(rectTransform);

        // Punch scale effect (like a click)
        rectTransform.DOPunchScale(Vector3.one * 0.1f, buttonScaleDuration, 5, 0.5f);
    }

    /// <summary>
    /// Professional button click animation with squeeze effect
    /// </summary>
    private void AnimateButtonClickProfessional(Button button)
    {
        if (button == null) return;

        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        // Kill existing tweens
        DOTween.Kill(rectTransform);

        // Create sequence for smooth click animation
        Sequence clickSequence = DOTween.Sequence();
        clickSequence.Append(rectTransform.DOScale(buttonClickScale, buttonClickDuration)
            .SetEase(Ease.OutQuad));
        clickSequence.Append(rectTransform.DOScale(1f, buttonClickDuration)
            .SetEase(Ease.OutBack));
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (startButton != null) startButton.interactable = interactable;
        if (settingsButton != null) settingsButton.interactable = interactable;
        if (customizationButton != null) customizationButton.interactable = interactable;
        if (exitButton != null) exitButton.interactable = interactable;
    }

    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Ensures an EventSystem exists for UI input (CRITICAL for touch to work!)
    /// FIXED: Now uses InputSystemUIInputModule for New Input System
    /// </summary>
    private void EnsureEventSystem()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            Debug.LogWarning("[MainMenuManager] No EventSystem found! Creating one for touch input...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<InputSystemUIInputModule>(); // NEW INPUT SYSTEM!
            DontDestroyOnLoad(eventSystemObj);
            Debug.Log("[MainMenuManager] EventSystem created with InputSystemUIInputModule!");
        }
        else
        {
            Debug.Log("[MainMenuManager] EventSystem found: " + eventSystem.gameObject.name);

            // Check if it has the correct input module
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null &&
                eventSystem.GetComponent<StandaloneInputModule>() != null)
            {
                Debug.LogWarning("[MainMenuManager] EventSystem has OLD StandaloneInputModule! Replacing...");
                Destroy(eventSystem.GetComponent<StandaloneInputModule>());
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[MainMenuManager] InputSystemUIInputModule added!");
            }
        }
    }

    #endregion
}
