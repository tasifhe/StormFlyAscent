using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using DG.Tweening;

/// <summary>
/// Manages the in-game pause menu with settings and navigation
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button closeButton; // X button
    [SerializeField] private Button mainMenuButton;
    
    [Header("Settings Controls")]
    [SerializeField] private Toggle gyroControlToggle;
    [SerializeField] private Slider soundFxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle vibrationToggle;
    
    [Header("Animation Settings")]
    [SerializeField] private float panelFadeDuration = 0.3f;
    [SerializeField] private float panelScaleDuration = 0.4f;
    [SerializeField] private Ease panelEaseIn = Ease.OutBack;
    [SerializeField] private Ease panelEaseOut = Ease.InBack;
    
    [Header("Blur Effect (Optional)")]
    [SerializeField] private GameObject blurBackground;
    
    private bool isPaused = false;
    private MenuSceneManager sceneManager;
    private SettingsManager settingsManager;
    
    private void Awake()
    {
        // Get references
        sceneManager = FindFirstObjectByType<MenuSceneManager>();
        settingsManager = FindFirstObjectByType<SettingsManager>();
        
        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(ResumeGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        
        // Setup settings controls
        if (gyroControlToggle != null)
            gyroControlToggle.onValueChanged.AddListener(OnGyroControlToggled);
        if (soundFxSlider != null)
            soundFxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
    }
    
    private void Start()
    {
        // Hide pause menu initially
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (blurBackground != null)
            blurBackground.SetActive(false);
        
        // Load saved settings
        LoadSettings();
    }
    
    private void Update()
    {
        // Listen for Escape key or Android back button
        bool escapePressed = false;

#if ENABLE_LEGACY_INPUT_MANAGER
        escapePressed = Input.GetKeyDown(KeyCode.Escape);
#elif ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
            escapePressed = kb.escapeKey.wasPressedThisFrame;
#endif

        if (escapePressed)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    
    /// <summary>
    /// Pause the game and show pause menu
    /// </summary>
    public void PauseGame()
    {
        if (isPaused) return;
        
        isPaused = true;
        Time.timeScale = 0f; // Freeze game
        
        // Show pause menu with animation
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            
            // Blur background
            if (blurBackground != null)
                blurBackground.SetActive(true);
            
            // Animate panel entrance
            CanvasGroup canvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = pauseMenuPanel.AddComponent<CanvasGroup>();
            
            canvasGroup.alpha = 0f;
            pauseMenuPanel.transform.localScale = Vector3.one * 0.8f;
            
            canvasGroup.DOFade(1f, panelFadeDuration).SetUpdate(true);
            pauseMenuPanel.transform.DOScale(Vector3.one, panelScaleDuration).SetEase(panelEaseIn).SetUpdate(true);
        }
        
        Debug.Log("Game Paused");
    }
    
    /// <summary>
    /// Resume the game and hide pause menu
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return;
        
        // Animate panel exit
        if (pauseMenuPanel != null)
        {
            CanvasGroup canvasGroup = pauseMenuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, panelFadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    pauseMenuPanel.SetActive(false);
                    if (blurBackground != null)
                        blurBackground.SetActive(false);
                });
            }
            else
            {
                pauseMenuPanel.SetActive(false);
                if (blurBackground != null)
                    blurBackground.SetActive(false);
            }
            
            pauseMenuPanel.transform.DOScale(Vector3.one * 0.8f, panelScaleDuration).SetEase(panelEaseOut).SetUpdate(true);
        }
        
        isPaused = false;
        Time.timeScale = 1f; // Resume game
        
        Debug.Log("Game Resumed");
    }
    
    /// <summary>
    /// Return to main menu
    /// </summary>
    private void ReturnToMainMenu()
    {
        // Resume time before loading
        Time.timeScale = 1f;
        isPaused = false;
        
        // Load main menu scene
        if (sceneManager != null)
        {
            sceneManager.LoadMainMenuScene();
        }
        else
        {
            Debug.LogError("MenuSceneManager not found!");
        }
    }
    
    #region Settings
    
    private void LoadSettings()
    {
        if (settingsManager != null)
        {
            // Load from SettingsManager if available
            if (musicSlider != null)
                musicSlider.value = settingsManager.GetMusicVolume();
            if (soundFxSlider != null)
                soundFxSlider.value = settingsManager.GetSFXVolume();
            if (vibrationToggle != null)
                vibrationToggle.isOn = SettingsManager.IsVibrationEnabled();
            if (gyroControlToggle != null)
                gyroControlToggle.isOn = PlayerPrefs.GetInt("GyroControl", 0) == 1;
        }
        else
        {
            // Load from PlayerPrefs directly
            if (musicSlider != null)
                musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            if (soundFxSlider != null)
                soundFxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            if (vibrationToggle != null)
                vibrationToggle.isOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
            if (gyroControlToggle != null)
                gyroControlToggle.isOn = PlayerPrefs.GetInt("GyroControl", 0) == 1;
        }
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        if (settingsManager != null)
        {
            settingsManager.SetMusicVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat("MusicVolume", value);
            // Apply volume to audio source if available
        }
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        if (settingsManager != null)
        {
            settingsManager.SetSFXVolume(value);
        }
        else
        {
            PlayerPrefs.SetFloat("SFXVolume", value);
        }
    }
    
    private void OnVibrationToggled(bool enabled)
    {
        if (settingsManager != null)
        {
            settingsManager.SetVibration(enabled);
        }
        else
        {
            PlayerPrefs.SetInt("Vibration", enabled ? 1 : 0);
        }
    }
    
    private void OnGyroControlToggled(bool enabled)
    {
        PlayerPrefs.SetInt("GyroControl", enabled ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Gyro Control: {(enabled ? "Enabled" : "Disabled")}");
    }
    
    #endregion
    
    private void OnDestroy()
    {
        // Clean up DOTween animations
        DOTween.Kill(pauseMenuPanel);
    }
}
