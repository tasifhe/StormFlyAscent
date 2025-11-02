using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Adds visual feedback effects to UI buttons
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation")]
    [SerializeField] private bool useScaleEffect = true;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float scaleSpeed = 10f;
    
    [Header("Color Animation")]
    [SerializeField] private bool useColorEffect = false;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color pressColor = Color.gray;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSound;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Image buttonImage;
    private Button button;
    private AudioSource audioSource;
    private bool isPressed = false;
    private bool isHovered = false;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        // Get or create audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }
    
    private void Update()
    {
        // Smooth scale animation
        if (useScaleEffect)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovered = true;
        
        if (useScaleEffect)
        {
            targetScale = originalScale * hoverScale;
        }
        
        if (useColorEffect && buttonImage != null)
        {
            buttonImage.color = hoverColor;
        }
        
        PlaySound(hoverSound);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        
        if (useScaleEffect && !isPressed)
        {
            targetScale = originalScale;
        }
        
        if (useColorEffect && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        
        if (useScaleEffect)
        {
            targetScale = originalScale * pressScale;
        }
        
        if (useColorEffect && buttonImage != null)
        {
            buttonImage.color = pressColor;
        }
        
        PlaySound(clickSound);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        
        if (useScaleEffect)
        {
            targetScale = isHovered ? originalScale * hoverScale : originalScale;
        }
        
        if (useColorEffect && buttonImage != null)
        {
            buttonImage.color = isHovered ? hoverColor : normalColor;
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void OnDisable()
    {
        // Reset to original state when disabled
        transform.localScale = originalScale;
        if (useColorEffect && buttonImage != null)
        {
            buttonImage.color = normalColor;
        }
    }
}
