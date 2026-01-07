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
    
    // Override to prevent base class from triggering immediately
    protected override void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;
        
        // Check for player by tag or name (more flexible)
        if (other.CompareTag("Player") || other.gameObject.name.Contains("Bird") || other.gameObject.name.Contains("Player"))
        {
            Debug.Log($"[RingObstacle] Player ENTERED trigger: {other.gameObject.name}");
            playerIsInsideRing = true;
            entryPosition = other.transform.position;
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Just ensure we track that player is inside
        if ((other.CompareTag("Player") || other.gameObject.name.Contains("Bird") || other.gameObject.name.Contains("Player")) && !hasBeenTriggered)
        {
            if (!playerIsInsideRing)
            {
                Debug.Log($"[RingObstacle] Player is STAYING in ring: {other.gameObject.name}");
                playerIsInsideRing = true;
                if (entryPosition == Vector3.zero)
                {
                    entryPosition = other.transform.position;
                }
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player") || other.gameObject.name.Contains("Bird") || other.gameObject.name.Contains("Player")))
        {
            Debug.Log($"[RingObstacle] Player EXITED ring: {other.gameObject.name}, playerIsInsideRing={playerIsInsideRing}, hasBeenTriggered={hasBeenTriggered}");
            
            if (playerIsInsideRing && !hasBeenTriggered)
            {
                hasBeenTriggered = true;
                CheckRingAccuracy(other.transform.position);
            }
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
        Debug.Log($"[RingObstacle] === CheckRingAccuracy START ===");
        Debug.Log($"[RingObstacle] Entry Position: {entryPosition}");
        Debug.Log($"[RingObstacle] Exit Position: {exitPosition}");
        Debug.Log($"[RingObstacle] Ring Center: {ringCenter.position}");
        
        // Calculate how close to center the player flew
        Vector3 centerPoint = (entryPosition + exitPosition) / 2f;
        float distanceFromCenter = Vector3.Distance(centerPoint, ringCenter.position);
        
        // Get actual collider radius instead of transform scale
        var collider = GetComponent<Collider>();
        float ringRadius = 5f; // Default fallback
        
        if (collider is SphereCollider sphere)
        {
            ringRadius = sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        }
        else if (collider is BoxCollider box)
        {
            ringRadius = Mathf.Max(box.size.x * transform.lossyScale.x, box.size.y * transform.lossyScale.y) / 2f;
        }
        else if (collider is CapsuleCollider capsule)
        {
            ringRadius = capsule.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        }
        else
        {
            // Fallback to transform scale
            ringRadius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y) / 2f;
        }
        
        float accuracyScore = 1f - Mathf.Clamp01(distanceFromCenter / ringRadius);
        
        Debug.Log($"[RingObstacle] Distance from center: {distanceFromCenter:F2}");
        Debug.Log($"[RingObstacle] Ring radius: {ringRadius:F2}");
        Debug.Log($"[RingObstacle] Accuracy score: {accuracyScore:F2}");
        Debug.Log($"[RingObstacle] Threshold: {accuracyThreshold:F2}");
        
        // TEMPORARY: Always succeed to test if scoring works
        bool forceSuccess = true; // Set to false once scoring is confirmed working
        
        if (forceSuccess || accuracyScore >= accuracyThreshold)
        {
            Debug.Log($"[RingObstacle] ✓✓✓ SUCCESS! Calling OnSuccess() ✓✓✓");
            OnSuccess();
            PlaySuccessEffect();
        }
        else
        {
            Debug.Log($"[RingObstacle] ✗✗✗ FAILED! Calling OnFail() ✗✗✗");
            OnFail();
        }
        
        Debug.Log($"[RingObstacle] === CheckRingAccuracy END ===");
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
