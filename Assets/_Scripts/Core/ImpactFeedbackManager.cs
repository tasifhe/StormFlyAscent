using UnityEngine;
using System.Collections;

/// <summary>
/// Central manager for all impact feedback (camera shake, haptics, visual effects, audio)
/// Call this when the bird hits an obstacle
/// </summary>
public class ImpactFeedbackManager : MonoBehaviour
{
    public static ImpactFeedbackManager Instance { get; private set; }

    [Header("Impact Types")]
    [SerializeField] private ImpactSettings lightImpact;
    [SerializeField] private ImpactSettings mediumImpact;
    [SerializeField] private ImpactSettings heavyImpact;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactParticlePrefab;
    [SerializeField] private Color impactFlashColor = new Color(1f, 0.3f, 0.3f, 0.5f);
    [SerializeField] private float flashDuration = 0.1f;

    [Header("Screen Effects")]
    [SerializeField] private CanvasGroup screenFlashPanel;
    [SerializeField] private bool enableChromaticAberration = false;
    [SerializeField] private float timeSlowAmount = 0.3f;
    [SerializeField] private float timeSlowDuration = 0.15f;

    private AudioSource audioSource;

    [System.Serializable]
    public class ImpactSettings
    {
        public float cameraShakeIntensity = 1f;
        public float cameraShakeDuration = 0.3f;
        public bool triggerHaptics = true;
        public AudioClip impactSound;
        public bool enableTimeFreeze = false;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup default impact settings if not configured
        if (lightImpact == null) lightImpact = new ImpactSettings { cameraShakeIntensity = 0.5f, cameraShakeDuration = 0.2f };
        if (mediumImpact == null) mediumImpact = new ImpactSettings { cameraShakeIntensity = 1f, cameraShakeDuration = 0.3f, triggerHaptics = true };
        if (heavyImpact == null) heavyImpact = new ImpactSettings { cameraShakeIntensity = 2f, cameraShakeDuration = 0.5f, triggerHaptics = true, enableTimeFreeze = true };
    }

    /// <summary>
    /// Trigger light impact feedback
    /// </summary>
    public void TriggerLightImpact(Vector3 position)
    {
        TriggerImpact(lightImpact, position);
    }

    /// <summary>
    /// Trigger medium impact feedback (default for obstacle hits)
    /// </summary>
    public void TriggerMediumImpact(Vector3 position)
    {
        TriggerImpact(mediumImpact, position);
    }

    /// <summary>
    /// Trigger heavy impact feedback (for crashes/game over)
    /// </summary>
    public void TriggerHeavyImpact(Vector3 position)
    {
        TriggerImpact(heavyImpact, position);
    }

    private void TriggerImpact(ImpactSettings settings, Vector3 position)
    {
        if (settings == null) return;

        Debug.Log($"[ImpactFeedback] Triggering impact at {position}");

        // Camera Shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(settings.cameraShakeIntensity, settings.cameraShakeDuration);
        }

        // Haptic Feedback
        if (settings.triggerHaptics && HapticFeedback.Instance != null)
        {
            if (settings.cameraShakeIntensity >= 1.5f)
                HapticFeedback.Instance.VibrateHeavy();
            else if (settings.cameraShakeIntensity >= 0.8f)
                HapticFeedback.Instance.VibrateMedium();
            else
                HapticFeedback.Instance.VibrateLight();
        }

        // Audio
        if (settings.impactSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(settings.impactSound);
        }

        // Visual Effects
        if (impactParticlePrefab != null)
        {
            GameObject particles = Instantiate(impactParticlePrefab, position, Quaternion.identity);
            Destroy(particles, 2f);
        }

        // Screen Flash
        if (screenFlashPanel != null)
        {
            StartCoroutine(ScreenFlashEffect());
        }

        // Time Freeze
        if (settings.enableTimeFreeze)
        {
            StartCoroutine(TimeFreezeEffect());
        }
    }

    private IEnumerator ScreenFlashEffect()
    {
        if (screenFlashPanel == null) yield break;

        // Flash in
        screenFlashPanel.gameObject.SetActive(true);
        screenFlashPanel.alpha = impactFlashColor.a;

        // Fade out
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            screenFlashPanel.alpha = Mathf.Lerp(impactFlashColor.a, 0f, elapsed / flashDuration);
            yield return null;
        }

        screenFlashPanel.alpha = 0f;
        screenFlashPanel.gameObject.SetActive(false);
    }

    private IEnumerator TimeFreezeEffect()
    {
        // Slow down time briefly for dramatic effect
        float originalTimeScale = Time.timeScale;
        Time.timeScale = timeSlowAmount;

        yield return new WaitForSecondsRealtime(timeSlowDuration);

        // Resume normal time
        Time.timeScale = originalTimeScale;
    }
}
