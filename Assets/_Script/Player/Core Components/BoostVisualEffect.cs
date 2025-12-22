using UnityEngine;

/// <summary>
/// Creates a visual trail/particle effect when boost is activated
/// Attach this to your bird character
/// </summary>
public class BoostVisualEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private ParticleSystem boostParticles;
    [SerializeField] private TrailRenderer boostTrail;
    [SerializeField] private Light boostLight;
    
    [Header("Flash Effect")]
    [SerializeField] private Renderer birdRenderer;
    [SerializeField] private Color boostFlashColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip boostSound;
    
    private Material birdMaterial;
    private Color originalColor;
    private float flashTimer = 0f;
    private bool isFlashing = false;
    
    private void Start()
    {
        // Get bird material for flash effect
        if (birdRenderer != null)
        {
            birdMaterial = birdRenderer.material;
            originalColor = birdMaterial.color;
        }
        
        // Disable effects initially
        if (boostParticles != null)
            boostParticles.Stop();
            
        if (boostTrail != null)
            boostTrail.emitting = false;
            
        if (boostLight != null)
            boostLight.enabled = false;
            
        // Setup audio
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    private void Update()
    {
        // Handle flash effect
        if (isFlashing)
        {
            flashTimer -= Time.deltaTime;
            
            if (flashTimer <= 0f)
            {
                // Flash ended
                isFlashing = false;
                if (birdMaterial != null)
                    birdMaterial.color = originalColor;
            }
            else
            {
                // Lerp back to original color
                float t = 1f - (flashTimer / flashDuration);
                if (birdMaterial != null)
                    birdMaterial.color = Color.Lerp(boostFlashColor, originalColor, t);
            }
        }
    }
    
    /// <summary>
    /// Call this when boost is activated
    /// </summary>
    public void TriggerBoost()
    {
        Debug.Log("🎨 BoostVisualEffect: Boost triggered!");
        
        // Play particles
        if (boostParticles != null)
        {
            boostParticles.Play();
        }
        
        // Enable trail
        if (boostTrail != null)
        {
            boostTrail.emitting = true;
            boostTrail.time = 1f;
        }
        
        // Flash light
        if (boostLight != null)
        {
            boostLight.enabled = true;
            StartCoroutine(DisableLightAfterDelay(0.3f));
        }
        
        // Flash material
        if (birdMaterial != null)
        {
            isFlashing = true;
            flashTimer = flashDuration;
            birdMaterial.color = boostFlashColor;
        }
        
        // Play sound
        if (audioSource != null && boostSound != null)
        {
            audioSource.PlayOneShot(boostSound);
        }
    }
    
    /// <summary>
    /// Call this when boost ends
    /// </summary>
    public void EndBoost()
    {
        // Stop trail
        if (boostTrail != null)
        {
            boostTrail.emitting = false;
        }
    }
    
    private System.Collections.IEnumerator DisableLightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (boostLight != null)
            boostLight.enabled = false;
    }
}
