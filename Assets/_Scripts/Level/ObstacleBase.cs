using UnityEngine;

/// <summary>
/// Base class for all obstacles in the game
/// Handles collision detection and basic obstacle behavior
/// </summary>
public abstract class ObstacleBase : MonoBehaviour
{
    [Header("Obstacle Settings")]
    [Tooltip("Is this a scoring obstacle (ring to go through) or hazard")]
    public bool isScoringObstacle = true;

    [Tooltip("Points awarded when successfully passed through")]
    public int pointValue = 10;

    [Header("Visual Feedback")]
    [SerializeField] protected Material normalMaterial;
    [SerializeField] protected Material successMaterial;
    [SerializeField] protected Material failMaterial;

    [Header("Audio")]
    [SerializeField] protected AudioClip successSound;
    [SerializeField] protected AudioClip failSound;

    protected bool hasBeenTriggered = false;
    protected Renderer obstacleRenderer;

    protected virtual void Awake()
    {
        obstacleRenderer = GetComponentInChildren<Renderer>();
    }

    protected virtual void Start()
    {
        // Verify setup
        var colliders = GetComponentsInChildren<Collider>();
        int triggerCount = 0;
        foreach (var col in colliders)
        {
            if (col.isTrigger) triggerCount++;
        }

        Debug.Log($"[ObstacleBase] {gameObject.name} initialized - Triggers: {triggerCount}, ObstacleManager: {ObstacleManager.Instance != null}");

        if (triggerCount == 0)
        {
            Debug.LogError($"[ObstacleBase] {gameObject.name} has NO TRIGGER COLLIDERS! Add a collider and check 'Is Trigger'");
        }

        if (ObstacleManager.Instance == null)
        {
            Debug.LogError("[ObstacleBase] ObstacleManager.Instance is NULL! Add ObstacleManager to your scene!");
        }
    }

    /// <summary>
    /// Called when player enters the obstacle trigger
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;

        // Use centralized player detection utility
        if (PlayerDetectionUtility.IsPlayer(other, out Transform playerTransform))
        {
            Debug.Log($"[ObstacleBase] Detected player collision: {playerTransform.name}");
            hasBeenTriggered = true;
            HandlePlayerInteraction(playerTransform.gameObject);
        }
    }

    /// <summary>
    /// Handle the interaction logic when player passes through
    /// </summary>
    protected abstract void HandlePlayerInteraction(GameObject player);

    /// <summary>
    /// Play success feedback
    /// </summary>
    protected virtual void OnSuccess()
    {
        Debug.Log($"[ObstacleBase] ✓ OnSuccess() called for {gameObject.name}");

        if (successMaterial != null && obstacleRenderer != null)
        {
            obstacleRenderer.material = successMaterial;
        }

        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
        }

        // Notify scoring system
        if (ObstacleManager.Instance != null)
        {
            Debug.Log($"[ObstacleBase] Notifying ObstacleManager about success (+{pointValue} points)");
            ObstacleManager.Instance.OnObstaclePassed(this, true);
        }
        else
        {
            Debug.LogError("[ObstacleBase] Cannot notify ObstacleManager - Instance is NULL!");
        }
    }

    /// <summary>
    /// Play fail feedback
    /// </summary>
    protected virtual void OnFail()
    {
        Debug.Log($"[ObstacleBase] ✗ OnFail() called for {gameObject.name}");

        if (failMaterial != null && obstacleRenderer != null)
        {
            obstacleRenderer.material = failMaterial;
        }

        if (failSound != null)
        {
            AudioSource.PlayClipAtPoint(failSound, transform.position);
        }

        // Notify scoring system
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnObstaclePassed(this, false);
        }
    }

    /// <summary>
    /// Reset obstacle for object pooling
    /// </summary>
    public virtual void ResetObstacle()
    {
        hasBeenTriggered = false;

        if (normalMaterial != null && obstacleRenderer != null)
        {
            obstacleRenderer.material = normalMaterial;
        }
    }
}
