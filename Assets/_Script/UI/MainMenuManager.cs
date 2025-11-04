using UnityEngine;
using UnityEngine.UI;
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
    
    private AudioSource audioSource;
    private MenuSceneManager sceneManager;
    
    private void Awake()
    {
        // Get references
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        sceneManager = FindFirstObjectByType<MenuSceneManager>();
        
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (customizationButton != null)
            customizationButton.onClick.AddListener(OnCustomizationButtonClicked);
        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
        
        // Add hover effects to all buttons
        AddButtonHoverEffect(startButton);
        AddButtonHoverEffect(settingsButton);
        AddButtonHoverEffect(customizationButton);
        AddButtonHoverEffect(exitButton);
    }
    
    private void Start()
    {
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
        AnimateButtonClick(startButton);
        PlaySound(buttonClickSound);
        Debug.Log("Start button clicked - Loading game...");
        
        // Disable buttons to prevent double-click
        SetButtonsInteractable(false);
        
        // Start game through scene manager
        if (sceneManager != null)
        {
            sceneManager.LoadGameScene();
        }
        else
        {
            Debug.LogError("MenuSceneManager not found! Cannot load game scene.");
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
    /// </summary>
    public void AddButtonHoverEffect(Button button)
    {
        if (button == null) return;
        
        // Add event triggers for hover
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
        if (button == null) return;
        
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // Kill existing tweens
        DOTween.Kill(rectTransform);
        
        // Animate scale
        float targetScale = hover ? buttonHoverScale : 1f;
        rectTransform.DOScale(targetScale, buttonScaleDuration).SetEase(Ease.OutQuad);
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
    
    #endregion
}
