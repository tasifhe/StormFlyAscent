using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Main menu manager that controls navigation between menu panels
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
    
    [Header("Settings")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private float panelTransitionDuration = 0.3f;
    
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
        PlaySound(buttonClickSound);
        Debug.Log("Settings button clicked");
        
        ShowPanel(settingsPanel);
        HidePanel(mainMenuPanel);
        
        PlaySound(panelOpenSound);
    }
    
    private void OnCustomizationButtonClicked()
    {
        PlaySound(buttonClickSound);
        Debug.Log("Customization button clicked");
        
        ShowPanel(customizationPanel);
        HidePanel(mainMenuPanel);
        
        PlaySound(panelOpenSound);
    }
    
    private void OnExitButtonClicked()
    {
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
            
            // Optional: Add fade-in animation
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeInPanel(canvasGroup));
            }
        }
    }
    
    private void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            // Optional: Add fade-out animation
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeOutPanel(canvasGroup, panel));
            }
            else
            {
                panel.SetActive(false);
            }
        }
    }
    
    private IEnumerator FadeInPanel(CanvasGroup canvasGroup)
    {
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < panelTransitionDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / panelTransitionDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOutPanel(CanvasGroup canvasGroup, GameObject panel)
    {
        float elapsed = 0f;
        canvasGroup.alpha = 1f;
        
        while (elapsed < panelTransitionDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / panelTransitionDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        panel.SetActive(false);
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
