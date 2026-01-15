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
    private bool hitBorder = false; // Track if player hit the ring border

    protected override void Awake()
    {
        base.Awake();

        if (ringCenter == null)
        {
            ringCenter = transform;
        }
    }

    /// <summary>
    /// Called by RingBorderCollider when player hits the ring border
    /// </summary>
    public void OnBorderHit(GameObject player)
    {
        if (hasBeenTriggered) return; // Already processed

        Debug.Log($"[RingObstacle] ❌ BORDER HIT detected! Player hit the ring frame.");

        hitBorder = true;
        hasBeenTriggered = true;

        // Immediate failure
        OnFail();
    }

    // Override to use better entry/exit tracking
    protected override void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;

        if (PlayerDetectionUtility.IsPlayer(other, out Transform playerTransform))
        {
            // Only set entry if not already inside (prevents double-entry)
            if (!playerIsInsideRing)
            {
                playerIsInsideRing = true;
                entryPosition = playerTransform.position;
                Debug.Log($"[RingObstacle] Player ENTERED ring at {entryPosition}");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Safety net: Only if we somehow missed OnTriggerEnter
        if (!hasBeenTriggered && !playerIsInsideRing)
        {
            if (PlayerDetectionUtility.IsPlayer(other, out Transform playerTransform))
            {
                playerIsInsideRing = true;
                entryPosition = playerTransform.position;
                Debug.LogWarning($"[RingObstacle] Entry position set via TriggerStay (missed Enter event)");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (PlayerDetectionUtility.IsPlayer(other, out Transform playerTransform))
        {
            Vector3 exitPosition = playerTransform.position;
            Debug.Log($"[RingObstacle] Player EXITED ring at {exitPosition} (Inside={playerIsInsideRing}, Triggered={hasBeenTriggered}, HitBorder={hitBorder})");

            if (playerIsInsideRing && !hasBeenTriggered)
            {
                hasBeenTriggered = true;

                // Check if player hit the border
                if (hitBorder)
                {
                    Debug.Log($"[RingObstacle] Border was hit - already failed, skipping accuracy check");
                    // Already called OnFail() in OnBorderHit
                }
                else
                {
                    // No border hit - check accuracy
                    CheckRingAccuracy(exitPosition);
                }
            }
            else if (!playerIsInsideRing)
            {
                Debug.LogWarning($"[RingObstacle] Exit without proper entry detected!");
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

        if (accuracyScore >= accuracyThreshold)
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
        hitBorder = false; // Reset border hit flag

        if (ringGlow != null)
        {
            ringGlow.SetActive(false);
        }
    }
}
