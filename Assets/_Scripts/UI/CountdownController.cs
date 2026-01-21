using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controls the countdown UI panel that shows "3, 2, 1, GO!" before gameplay starts
/// </summary>
public class CountdownController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Settings")]
    [SerializeField] private float countdownDuration = 1.0f;
    [Tooltip("Text to show after countdown completes")]
    [SerializeField] private string goText = "GO!";
    [SerializeField] private float goTextDuration = 0.5f;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip countdownBeep;
    [SerializeField] private AudioClip goBeep;

    // Events
    public System.Action OnCountdownComplete;

    private void Awake()
    {
        Debug.Log("[CountdownController] Awake() called");

        // Hide panel by default
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
            Debug.Log("[CountdownController] ✓ Panel reference found and hidden");
        }
        else
        {
            Debug.LogError("[CountdownController] ✗ Countdown Panel is NOT assigned! Please assign it in the Inspector.");
        }

        if (countdownText == null)
        {
            Debug.LogError("[CountdownController] ✗ Countdown Text is NOT assigned! Please assign it in the Inspector.");
        }
        else
        {
            Debug.Log("[CountdownController] ✓ Text reference found");
        }
    }

    /// <summary>
    /// Starts the countdown sequence
    /// </summary>
    public void StartCountdown()
    {
        Debug.Log("[CountdownController] StartCountdown() called!");

        if (countdownPanel == null)
        {
            Debug.LogError("[CountdownController] Cannot start countdown - Panel reference is missing!");
            return;
        }

        if (countdownText == null)
        {
            Debug.LogError("[CountdownController] Cannot start countdown - Text reference is missing!");
            return;
        }

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        Debug.Log("[CountdownController] Countdown routine started!");

        // Show panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
            Debug.Log("[CountdownController] Panel shown!");
        }

        // Game should already be paused by GameStartManager
        Debug.Log("[CountdownController] Starting countdown (game is paused)");

        // Countdown from 3 to 1
        for (int i = 3; i > 0; i--)
        {
            Debug.Log($"[CountdownController] Showing: {i}");

            if (countdownText != null)
            {
                countdownText.text = i.ToString();
            }

            // Play countdown beep
            if (countdownBeep != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(countdownBeep);
            }

            // Wait for countdown duration (using unscaled time)
            float elapsed = 0f;
            while (elapsed < countdownDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // Show "GO!"
        Debug.Log($"[CountdownController] Showing: {goText}");
        if (countdownText != null)
        {
            countdownText.text = goText;
        }

        // Play go beep
        if (goBeep != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(goBeep);
        }

        // Wait for GO text duration - game is STILL PAUSED here
        float goElapsed = 0f;
        while (goElapsed < goTextDuration)
        {
            goElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Hide panel
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
            Debug.Log("[CountdownController] Panel hidden");
        }

        // Notify listeners FIRST
        Debug.Log("[CountdownController] Notifying listeners (game still paused)...");
        OnCountdownComplete?.Invoke();

        // Small delay to ensure listeners finish processing
        yield return new WaitForSecondsRealtime(0.1f);

        // NOW resume the game - this is the LAST thing we do
        Time.timeScale = 1f;
        Debug.Log("[CountdownController] ✅ GAME RESUMED! Let's play!");
    }
}
