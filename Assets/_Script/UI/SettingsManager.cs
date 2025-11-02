using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

/// <summary>
/// Manages game settings (audio, graphics, controls)
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button backButton;
    
    [Header("Audio Settings")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private TextMeshProUGUI masterVolumeText;
    [SerializeField] private TextMeshProUGUI musicVolumeText;
    [SerializeField] private TextMeshProUGUI sfxVolumeText;
    [SerializeField] private AudioMixer audioMixer; // Optional
    
    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle vsyncToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    
    [Header("Control Settings")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Toggle vibrationToggle;
    
    [Header("PlayerPrefs Keys")]
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUTE_KEY = "Mute";
    private const string QUALITY_KEY = "Quality";
    private const string VSYNC_KEY = "VSync";
    private const string SENSITIVITY_KEY = "Sensitivity";
    private const string INVERT_Y_KEY = "InvertY";
    private const string VIBRATION_KEY = "Vibration";
    
    private MainMenuManager mainMenuManager;
    
    private void Awake()
    {
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();
        
        // Setup listeners
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
        
        // Audio listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        if (muteToggle != null)
            muteToggle.onValueChanged.AddListener(OnMuteToggled);
        
        // Graphics listeners
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
        if (vsyncToggle != null)
            vsyncToggle.onValueChanged.AddListener(OnVSyncToggled);
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        
        // Control listeners
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(OnInvertYToggled);
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);
    }
    
    private void Start()
    {
        LoadSettings();
        PopulateQualityDropdown();
        PopulateResolutionDropdown();
    }
    
    #region Load/Save Settings
    
    private void LoadSettings()
    {
        // Load audio settings
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        bool mute = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (muteToggle != null) muteToggle.isOn = mute;
        
        UpdateVolumeText(masterVolumeText, masterVolume);
        UpdateVolumeText(musicVolumeText, musicVolume);
        UpdateVolumeText(sfxVolumeText, sfxVolume);
        
        // Load graphics settings
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        bool vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;
        
        if (qualityDropdown != null) qualityDropdown.value = quality;
        if (vsyncToggle != null) vsyncToggle.isOn = vsync;
        
        // Load control settings
        float sensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
        bool invertY = PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1;
        bool vibration = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
        
        if (sensitivitySlider != null) sensitivitySlider.value = sensitivity;
        if (invertYToggle != null) invertYToggle.isOn = invertY;
        if (vibrationToggle != null) vibrationToggle.isOn = vibration;
        
        UpdateSensitivityText(sensitivity);
        
        // Apply settings
        ApplyAudioSettings();
        ApplyGraphicsSettings();
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }
    
    #endregion
    
    #region Audio Settings
    
    private void OnMasterVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
        UpdateVolumeText(masterVolumeText, value);
        ApplyAudioSettings();
    }
    
    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        UpdateVolumeText(musicVolumeText, value);
        ApplyAudioSettings();
    }
    
    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        UpdateVolumeText(sfxVolumeText, value);
        ApplyAudioSettings();
    }
    
    private void OnMuteToggled(bool muted)
    {
        PlayerPrefs.SetInt(MUTE_KEY, muted ? 1 : 0);
        ApplyAudioSettings();
    }
    
    private void ApplyAudioSettings()
    {
        bool muted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        
        if (muted)
        {
            AudioListener.volume = 0f;
        }
        else
        {
            float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            AudioListener.volume = masterVolume;
        }
        
        // If using AudioMixer
        if (audioMixer != null)
        {
            float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
            float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
            float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        }
    }
    
    private void UpdateVolumeText(TextMeshProUGUI text, float value)
    {
        if (text != null)
            text.text = Mathf.RoundToInt(value * 100f) + "%";
    }
    
    #endregion
    
    #region Graphics Settings
    
    private void PopulateQualityDropdown()
    {
        if (qualityDropdown == null) return;
        
        qualityDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>(QualitySettings.names);
        qualityDropdown.AddOptions(options);
        qualityDropdown.value = QualitySettings.GetQualityLevel();
    }
    
    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        
        Resolution[] resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
    }
    
    private void OnQualityChanged(int index)
    {
        PlayerPrefs.SetInt(QUALITY_KEY, index);
        QualitySettings.SetQualityLevel(index);
        Debug.Log("Quality set to: " + QualitySettings.names[index]);
    }
    
    private void OnVSyncToggled(bool enabled)
    {
        PlayerPrefs.SetInt(VSYNC_KEY, enabled ? 1 : 0);
        QualitySettings.vSyncCount = enabled ? 1 : 0;
        Debug.Log("VSync: " + enabled);
    }
    
    private void OnResolutionChanged(int index)
    {
        Resolution[] resolutions = Screen.resolutions;
        if (index < resolutions.Length)
        {
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, Screen.fullScreen);
            Debug.Log("Resolution changed to: " + resolutions[index].width + "x" + resolutions[index].height);
        }
    }
    
    private void ApplyGraphicsSettings()
    {
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        bool vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;
        
        QualitySettings.SetQualityLevel(quality);
        QualitySettings.vSyncCount = vsync ? 1 : 0;
    }
    
    #endregion
    
    #region Control Settings
    
    private void OnSensitivityChanged(float value)
    {
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
        UpdateSensitivityText(value);
    }
    
    private void OnInvertYToggled(bool inverted)
    {
        PlayerPrefs.SetInt(INVERT_Y_KEY, inverted ? 1 : 0);
    }
    
    private void OnVibrationToggled(bool enabled)
    {
        PlayerPrefs.SetInt(VIBRATION_KEY, enabled ? 1 : 0);
        
        #if UNITY_ANDROID || UNITY_IOS
        // Mobile vibration can be controlled here
        #endif
    }
    
    private void UpdateSensitivityText(float value)
    {
        if (sensitivityText != null)
            sensitivityText.text = value.ToString("F2");
    }
    
    /// <summary>
    /// Public method to get sensitivity value
    /// </summary>
    public static float GetSensitivity()
    {
        return PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
    }
    
    /// <summary>
    /// Public method to check if Y-axis is inverted
    /// </summary>
    public static bool IsYAxisInverted()
    {
        return PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1;
    }
    
    /// <summary>
    /// Public method to check if vibration is enabled
    /// </summary>
    public static bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
    }
    
    #endregion
    
    #region Button Callbacks
    
    private void OnBackButtonClicked()
    {
        SaveSettings();
        
        if (mainMenuManager != null)
        {
            mainMenuManager.ReturnToMainMenu();
        }
    }
    
    /// <summary>
    /// Reset all settings to default
    /// </summary>
    public void ResetToDefaults()
    {
        PlayerPrefs.DeleteAll();
        LoadSettings();
        Debug.Log("Settings reset to defaults!");
    }
    
    #endregion
}
