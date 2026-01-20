using UnityEngine;

/// <summary>
/// Manages haptic/vibration feedback for mobile devices
/// Provides different vibration patterns for different impacts
/// </summary>
public class HapticFeedback : MonoBehaviour
{
    public static HapticFeedback Instance { get; private set; }

    [Header("Vibration Settings")]
    [SerializeField] private bool enableVibration = true;

    // Vibration durations in milliseconds
    private const long LIGHT_VIBRATION = 50;
    private const long MEDIUM_VIBRATION = 100;
    private const long HEAVY_VIBRATION = 200;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Load vibration settings
        enableVibration = SettingsManager.IsVibrationEnabled();
    }

    /// <summary>
    /// Light vibration for minor impacts
    /// </summary>
    public void VibrateLight()
    {
        if (!enableVibration) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif

        Debug.Log("[HapticFeedback] Light vibration triggered");
    }

    /// <summary>
    /// Medium vibration for obstacle hits
    /// </summary>
    public void VibrateMedium()
    {
        if (!enableVibration) return;

#if UNITY_ANDROID
        // Android supports duration control
        AndroidVibrate(MEDIUM_VIBRATION);
#elif UNITY_IOS
        // iOS uses standard vibration
        Handheld.Vibrate();
#endif

        Debug.Log("[HapticFeedback] Medium vibration triggered");
    }

    /// <summary>
    /// Heavy vibration for crashes/game over
    /// </summary>
    public void VibrateHeavy()
    {
        if (!enableVibration) return;

#if UNITY_ANDROID
        AndroidVibrate(HEAVY_VIBRATION);
#elif UNITY_IOS
        Handheld.Vibrate();
#endif

        Debug.Log("[HapticFeedback] Heavy vibration triggered");
    }

    /// <summary>
    /// Double vibration pattern for special events
    /// </summary>
    public void VibrateDouble()
    {
        if (!enableVibration) return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
        Invoke(nameof(DelayedVibrate), 0.1f);
#endif
    }

    private void DelayedVibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// Android-specific vibration with duration control
    /// </summary>
    private void AndroidVibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                using (AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                {
                    vibrator.Call("vibrate", milliseconds);
                }
            }
        }
#endif
    }

    /// <summary>
    /// Enable or disable vibration
    /// </summary>
    public void SetVibrationEnabled(bool enabled)
    {
        enableVibration = enabled;
    }
}
