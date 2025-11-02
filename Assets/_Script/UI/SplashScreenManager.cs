using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the "Tap to Start" splash screen that appears when the game loads
/// </summary>
public class SplashScreenManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject splashPanel;
    [SerializeField] private TextMeshProUGUI tapToStartText;
    [SerializeField] private Image fadeImage;
    
    [Header("Animation Settings")]
    [SerializeField] private float textPulseSpeed = 1.5f;
    [SerializeField] private float textMinAlpha = 0.3f;
    [SerializeField] private float textMaxAlpha = 1f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Events")]
    [SerializeField] private UnityEngine.Events.UnityEvent onTapDetected;
    
    private bool hasStarted = false;
    private bool isTransitioning = false;
    
    private void Start()
    {
        if (splashPanel != null)
            splashPanel.SetActive(true);
            
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }
        
        StartCoroutine(PulseTextAnimation());
    }
    
    private void Update()
    {
        if (hasStarted || isTransitioning)
            return;
        
        // Detect tap/click input
        bool inputDetected = false;
        
        // Touch input (mobile)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputDetected = true;
        }
        
        // Mouse input (testing)
        if (Input.GetMouseButtonDown(0))
        {
            inputDetected = true;
        }
        
        // Keyboard input (testing)
        if (Input.anyKeyDown)
        {
            inputDetected = true;
        }
        
        if (inputDetected)
        {
            OnTapDetected();
        }
    }
    
    private IEnumerator PulseTextAnimation()
    {
        if (tapToStartText == null)
            yield break;
            
        while (!hasStarted)
        {
            // Pulse alpha using sine wave
            float alpha = Mathf.Lerp(textMinAlpha, textMaxAlpha, 
                (Mathf.Sin(Time.time * textPulseSpeed) + 1f) * 0.5f);
            
            Color c = tapToStartText.color;
            c.a = alpha;
            tapToStartText.color = c;
            
            yield return null;
        }
    }
    
    private void OnTapDetected()
    {
        if (hasStarted)
            return;
            
        hasStarted = true;
        isTransitioning = true;
        
        Debug.Log("Tap detected! Transitioning to main menu...");
        
        // Invoke event
        onTapDetected?.Invoke();
        
        // Start transition
        StartCoroutine(TransitionToMainMenu());
    }
    
    private IEnumerator TransitionToMainMenu()
    {
        // Stop text animation
        if (tapToStartText != null)
        {
            Color c = tapToStartText.color;
            c.a = 0f;
            tapToStartText.color = c;
        }
        
        // Fade out
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = new Color(0, 0, 0, 1f);
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeOutDuration);
                yield return null;
            }
        }
        
        // Hide splash panel
        if (splashPanel != null)
            splashPanel.SetActive(false);
        
        // Notify main menu manager to show
        MainMenuManager mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        
        // Fade back in
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = fadeImage.color;
            Color targetColor = new Color(0, 0, 0, 0f);
            
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeOutDuration);
                yield return null;
            }
        }
        
        isTransitioning = false;
    }
    
    // Public method to skip splash (for testing)
    public void SkipToMainMenu()
    {
        if (!hasStarted)
            OnTapDetected();
    }
}
