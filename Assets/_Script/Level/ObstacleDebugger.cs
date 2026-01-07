using UnityEngine;

/// <summary>
/// Debug helper to diagnose obstacle collision issues
/// Attach this to any obstacle to see what's colliding with it
/// </summary>
public class ObstacleDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool logAllCollisions = true;
    public bool drawDebugGizmos = true;
    public Color gizmoColor = Color.yellow;
    
    private void Start()
    {
        // Check components
        var colliders = GetComponentsInChildren<Collider>();
        Debug.Log($"[ObstacleDebugger] {gameObject.name} has {colliders.Length} collider(s)");
        
        foreach (var col in colliders)
        {
            Debug.Log($"  - Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}, GameObject: {col.gameObject.name}");
        }
        
        var obstacleBase = GetComponent<ObstacleBase>();
        if (obstacleBase == null)
        {
            Debug.LogWarning($"[ObstacleDebugger] {gameObject.name} has no ObstacleBase component!");
        }
        else
        {
            Debug.Log($"[ObstacleDebugger] {gameObject.name} has {obstacleBase.GetType().Name}");
        }
        
        // Check ObstacleManager
        if (ObstacleManager.Instance == null)
        {
            Debug.LogError("[ObstacleDebugger] ObstacleManager.Instance is NULL! Add ObstacleManager to your scene!");
        }
        else
        {
            Debug.Log("[ObstacleDebugger] ObstacleManager found successfully");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (logAllCollisions)
        {
            Debug.Log($"[ObstacleDebugger] TRIGGER ENTER: {other.gameObject.name} (Tag: {other.tag}, Layer: {LayerMask.LayerToName(other.gameObject.layer)})");
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        // Only log once per second to avoid spam
        if (logAllCollisions && Time.frameCount % 60 == 0)
        {
            Debug.Log($"[ObstacleDebugger] TRIGGER STAY: {other.gameObject.name}");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (logAllCollisions)
        {
            Debug.Log($"[ObstacleDebugger] TRIGGER EXIT: {other.gameObject.name} (Tag: {other.tag})");
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (logAllCollisions)
        {
            Debug.LogWarning($"[ObstacleDebugger] COLLISION (NOT TRIGGER): {collision.gameObject.name} - Check 'Is Trigger' on collider!");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!drawDebugGizmos) return;
        
        var colliders = GetComponentsInChildren<Collider>();
        Gizmos.color = gizmoColor;
        
        foreach (var col in colliders)
        {
            if (col.isTrigger)
            {
                // Draw trigger volume
                Gizmos.matrix = col.transform.localToWorldMatrix;
                
                if (col is BoxCollider box)
                {
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                }
                else if (col is CapsuleCollider capsule)
                {
                    Gizmos.DrawWireSphere(capsule.center, capsule.radius);
                }
            }
        }
    }
}
