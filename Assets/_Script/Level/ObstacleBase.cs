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
    
    /// <summary>
    /// Called when player enters the obstacle trigger
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            hasBeenTriggered = true;
            HandlePlayerInteraction(other.gameObject);
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
        if (successMaterial != null && obstacleRenderer != null)
        {
            obstacleRenderer.material = successMaterial;
        }
        
        if (successSound != null)
        {
            AudioSource.PlayClipAtPoint(successSound, transform.position);
        }
        
        // Notify scoring system
        ObstacleManager.Instance?.OnObstaclePassed(this, true);
    }
    
    /// <summary>
    /// Play fail feedback
    /// </summary>
    protected virtual void OnFail()
    {
        if (failMaterial != null && obstacleRenderer != null)
        {
            obstacleRenderer.material = failMaterial;
        }
        
        if (failSound != null)
        {
            AudioSource.PlayClipAtPoint(failSound, transform.position);
        }
        
        // Notify scoring system
        ObstacleManager.Instance?.OnObstaclePassed(this, false);
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
