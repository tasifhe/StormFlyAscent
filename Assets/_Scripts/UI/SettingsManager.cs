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
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private AudioMixer audioMixer; // Optional

    [Header("Graphics Settings")]
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Control Settings")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TextMeshProUGUI sensitivityText;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Toggle vibrationToggle; // Legacy toggle support

    [Header("Image-Based Toggles")]
    [SerializeField] private ImageToggle gyroControlToggle;
    [SerializeField] private ImageToggle vibrationImageToggle;

    [Header("Advanced Settings")]
    [SerializeField] private GameObject advancedSettingsPanel;
    [SerializeField] private Button advancedSettingsButton;
    [SerializeField] private Button advancedSettingsCloseButton;

    [Header("PlayerPrefs Keys")]
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string QUALITY_KEY = "Quality";
    private const string SENSITIVITY_KEY = "Sensitivity";
    private const string INVERT_Y_KEY = "InvertY";
    private const string VIBRATION_KEY = "Vibration";
    private const string GYRO_CONTROL_KEY = "GyroControl";

    private MainMenuManager mainMenuManager;

    private void Awake()
    {
        mainMenuManager = FindFirstObjectByType<MainMenuManager>();

        // Setup listeners
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);

        // Audio listeners
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

        // Graphics listeners
        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);

        // Control listeners
        if (sensitivitySlider != null)
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        if (invertYToggle != null)
            invertYToggle.onValueChanged.AddListener(OnInvertYToggled);
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationToggled);

        // Image-based toggle listeners
        if (gyroControlToggle != null)
            gyroControlToggle.onValueChanged.AddListener(OnGyroControlToggled);
        if (vibrationImageToggle != null)
            vibrationImageToggle.onValueChanged.AddListener(OnVibrationToggled);

        // Advanced settings listeners
        if (advancedSettingsButton != null)
            advancedSettingsButton.onClick.AddListener(OpenAdvancedSettings);
        if (advancedSettingsCloseButton != null)
            advancedSettingsCloseButton.onClick.AddListener(CloseAdvancedSettings);
    }

    private void Start()
    {
        LoadSettings();
        PopulateQualityDropdown();
    }

    #region Load/Save Settings

    private void LoadSettings()
    {
        // Load audio settings
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;

        // Load graphics settings
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());

        if (qualityDropdown != null) qualityDropdown.value = quality;

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

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        ApplyAudioSettings();
    }

    private void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        ApplyAudioSettings();
    }

    private void ApplyAudioSettings()
    {
        // If using AudioMixer
        if (audioMixer != null)
        {
            float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
            float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(musicVolume, 0.0001f)) * 20);
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(sfxVolume, 0.0001f)) * 20);
        }
        else
        {
            // Fallback to AudioListener if no mixer
            float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
            AudioListener.volume = musicVolume;
        }
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



    private void OnQualityChanged(int index)
    {
        PlayerPrefs.SetInt(QUALITY_KEY, index);
        QualitySettings.SetQualityLevel(index);
        Debug.Log("Quality set to: " + QualitySettings.names[index]);
    }



    private void ApplyGraphicsSettings()
    {
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        QualitySettings.SetQualityLevel(quality);
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

    /// <summary>
    /// Get current music volume
    /// </summary>
    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
    }

    /// <summary>
    /// Get current SFX volume
    /// </summary>
    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = value;
        OnMusicVolumeChanged(value);
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = value;
        OnSFXVolumeChanged(value);
    }

    /// <summary>
    /// Set vibration enabled/disabled
    /// </summary>
    public void SetVibration(bool enabled)
    {
        PlayerPrefs.SetInt(VIBRATION_KEY, enabled ? 1 : 0);
        if (vibrationToggle != null)
            vibrationToggle.isOn = enabled;
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

    #region Advanced Settings

    /// <summary>
    /// Open the advanced settings panel
    /// </summary>
    public void OpenAdvancedSettings()
    {
        if (advancedSettingsPanel != null)
        {
            advancedSettingsPanel.SetActive(true);
            Debug.Log("Advanced Settings opened");
        }
    }

    /// <summary>
    /// Close the advanced settings panel
    /// </summary>
    public void CloseAdvancedSettings()
    {
        if (advancedSettingsPanel != null)
        {
            advancedSettingsPanel.SetActive(false);
            Debug.Log("Advanced Settings closed");
        }
    }

    #endregion

    #region Gyro Control

    /// <summary>
    /// Called when gyro control toggle is changed
    /// </summary>
    private void OnGyroControlToggled(bool enabled)
    {
        PlayerPrefs.SetInt(GYRO_CONTROL_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();

        // Enable/disable gyroscope
#if UNITY_ANDROID || UNITY_IOS
        if (SystemInfo.supportsGyroscope)
        {
            Input.gyro.enabled = enabled;
        }
#endif

        Debug.Log("Gyro Control: " + enabled);
    }

    /// <summary>
    /// Public method to check if gyro control is enabled
    /// </summary>
    public static bool IsGyroControlEnabled()
    {
        return PlayerPrefs.GetInt(GYRO_CONTROL_KEY, 0) == 1;
    }

    #endregion
}
