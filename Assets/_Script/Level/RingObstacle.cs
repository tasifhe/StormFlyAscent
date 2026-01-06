using UnityEngine;

/// <summary>
/// Ring obstacle that player must fly through to score points
/// </summary>
public class RingObstacle : ObstacleBase
{
    [Header("Ring Settings")]
    [Tooltip("How accurate player needs to be (0-1, higher = more forgiving)")]
    [Range(0.1f, 1f)]
    public float accuracyThreshold = 0.7f;
    
    [Tooltip("Center point of the ring")]
    [SerializeField] private Transform ringCenter;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private GameObject ringGlow;
    
    private bool playerIsInsideRing = false;
    private Vector3 entryPosition;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (ringCenter == null)
        {
            ringCenter = transform;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !hasBeenTriggered)
        {
            playerIsInsideRing = true;
            if (entryPosition == Vector3.zero)
            {
                entryPosition = other.transform.position;
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playerIsInsideRing && !hasBeenTriggered)
        {
            hasBeenTriggered = true;
            CheckRingAccuracy(other.transform.position);
        }
    }
    
    protected override void HandlePlayerInteraction(GameObject player)
    {
        // Handled in OnTriggerExit for better accuracy check
    }
    
    /// <summary>
    /// Check how accurately the player flew through the ring
    /// </summary>
    private void CheckRingAccuracy(Vector3 exitPosition)
    {
        // Calculate how close to center the player flew
        Vector3 centerPoint = (entryPosition + exitPosition) / 2f;
        float distanceFromCenter = Vector3.Distance(centerPoint, ringCenter.position);
        
        // Compare to ring radius (assuming uniform scale)
        float ringRadius = transform.localScale.x / 2f;
        float accuracyScore = 1f - Mathf.Clamp01(distanceFromCenter / ringRadius);
        
        if (accuracyScore >= accuracyThreshold)
        {
            OnSuccess();
            PlaySuccessEffect();
        }
        else
        {
            OnFail();
        }
    }
    
    private void PlaySuccessEffect()
    {
        if (successParticles != null)
        {
            successParticles.Play();
        }
        
        if (ringGlow != null)
        {
            ringGlow.SetActive(true);
            // Optionally fade out the glow after a delay
            Invoke(nameof(HideGlow), 0.5f);
        }
    }
    
    private void HideGlow()
    {
        if (ringGlow != null)
        {
            ringGlow.SetActive(false);
        }
    }
    
    public override void ResetObstacle()
    {
        base.ResetObstacle();
        playerIsInsideRing = false;
        entryPosition = Vector3.zero;
        
        if (ringGlow != null)
        {
            ringGlow.SetActive(false);
        }
    }
}
