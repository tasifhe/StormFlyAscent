using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages boost button UI state and visual feedback
/// Shows cooldown, disables button when not ready
/// </summary>
public class ButtonBoostUI : MonoBehaviour
{
    [Header("Button References")]
    [Tooltip("The boost button component")]
    public Button boostButton;
    
    [Header("Visual Feedback (Optional)")]
    [Tooltip("Image that fills to show cooldown progress")]
    public Image cooldownOverlay;
    
    [Tooltip("Text showing cooldown time remaining")]
    public TextMeshProUGUI cooldownText;
    
    [Tooltip("Image that pulses when boost is ready")]
    public Image readyIndicator;
    
    [Header("Colors")]
    public Color readyColor = Color.green;
    public Color cooldownColor = Color.red;
    public Color boostingColor = Color.yellow;
    
    [Header("Animation")]
    [Tooltip("Pulse speed when boost is ready")]
    public float pulseSpeed = 2f;
    
    private BoostSystem boostSystem;
    private Image buttonImage;
    private float cooldownDuration = 0.8f; // Default, will be updated
    
    void Start()
    {
        // Find the boost system
        boostSystem = FindFirstObjectByType<BoostSystem>();
        
        if (boostSystem == null)
        {
            Debug.LogWarning("BoostSystem not found! ButtonBoostUI won't work.");
        }
        
        // Get button image for color changes
        if (boostButton != null)
        {
            buttonImage = boostButton.GetComponent<Image>();
        }
        
        // Get cooldown duration from Character
        Character character = FindFirstObjectByType<Character>();
        if (character != null)
        {
            cooldownDuration = character.flapCooldown;
        }
    }
    
    void Update()
    {
        if (boostSystem == null) return;
        
        UpdateButtonState();
        UpdateCooldownVisuals();
        UpdateReadyIndicator();
    }
    
    /// <summary>
    /// Enable/disable button based on boost state
    /// </summary>
    private void UpdateButtonState()
    {
        if (boostButton == null) return;
        
        // Disable button during cooldown or while boosting
        boostButton.interactable = !boostSystem.IsOnCooldown && !boostSystem.IsBoosting;
        
        // Change button color based on state
        if (buttonImage != null)
        {
            if (boostSystem.IsBoosting)
            {
                buttonImage.color = boostingColor;
            }
            else if (boostSystem.IsOnCooldown)
            {
                buttonImage.color = cooldownColor;
            }
            else
            {
                buttonImage.color = readyColor;
            }
        }
    }
    
    /// <summary>
    /// Update cooldown overlay and text
    /// </summary>
    private void UpdateCooldownVisuals()
    {
        // Update cooldown fill overlay
        if (cooldownOverlay != null)
        {
            if (boostSystem.IsOnCooldown)
            {
                cooldownOverlay.fillAmount = boostSystem.CooldownRemaining / cooldownDuration;
                cooldownOverlay.gameObject.SetActive(true);
            }
            else
            {
                cooldownOverlay.fillAmount = 0f;
                cooldownOverlay.gameObject.SetActive(false);
            }
        }
        
        // Update cooldown text
        if (cooldownText != null)
        {
            if (boostSystem.IsOnCooldown)
            {
                cooldownText.text = $"{boostSystem.CooldownRemaining:F1}s";
                cooldownText.gameObject.SetActive(true);
            }
            else if (boostSystem.IsBoosting)
            {
                cooldownText.text = "BOOST!";
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Pulse the ready indicator when boost is available
    /// </summary>
    private void UpdateReadyIndicator()
    {
        if (readyIndicator == null) return;
        
        if (!boostSystem.IsOnCooldown && !boostSystem.IsBoosting)
        {
            // Pulse effect when ready
            float pulse = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            readyIndicator.color = new Color(readyColor.r, readyColor.g, readyColor.b, pulse);
            readyIndicator.gameObject.SetActive(true);
        }
        else
        {
            readyIndicator.gameObject.SetActive(false);
        }
    }
}
