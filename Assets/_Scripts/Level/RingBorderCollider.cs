using UnityEngine;

/// <summary>
/// Detects collisions with the ring border/frame (the solid part)
/// Attach this to the ring mesh object that has the mesh collider
/// </summary>
[RequireComponent(typeof(Collider))]
public class RingBorderCollider : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the parent RingObstacle script")]
    [SerializeField] private RingObstacle parentRing;

    [Header("Layer Setup")]
    [Tooltip("Expected layer name for obstacle detection (e.g., 'obstacle')")]
    [SerializeField] private string expectedLayerName = "obstacle";

    [Tooltip("Validate that this object is on the correct layer")]
    [SerializeField] private bool validateLayer = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private int obstacleLayer = -1;

    private void Awake()
    {
        // Auto-find parent ring if not assigned
        if (parentRing == null)
        {
            parentRing = GetComponentInParent<RingObstacle>();
        }

        if (parentRing == null)
        {
            Debug.LogError($"[RingBorderCollider] No RingObstacle found in parent! Assign manually on {gameObject.name}");
        }

        // Get obstacle layer
        obstacleLayer = LayerMask.NameToLayer(expectedLayerName);

        if (obstacleLayer == -1)
        {
            Debug.LogError($"[RingBorderCollider] Layer '{expectedLayerName}' does not exist! Create it in Tags & Layers.");
        }

        // Validate this object is on the correct layer
        if (validateLayer)
        {
            if (gameObject.layer != obstacleLayer)
            {
                Debug.LogWarning($"[RingBorderCollider] {gameObject.name} is on layer '{LayerMask.LayerToName(gameObject.layer)}' but should be on '{expectedLayerName}'!");

                if (obstacleLayer != -1)
                {
                    Debug.Log($"[RingBorderCollider] Auto-fixing: Setting {gameObject.name} to layer '{expectedLayerName}'");
                    gameObject.layer = obstacleLayer;
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[RingBorderCollider] ✅ Layer validation passed: {gameObject.name} is on '{expectedLayerName}' layer");
                }
            }
        }

        // Validate collider setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[RingBorderCollider] No collider on {gameObject.name}!");
        }
        else if (col.isTrigger)
        {
            Debug.LogWarning($"[RingBorderCollider] Collider on {gameObject.name} is a TRIGGER. It should be NON-TRIGGER for border collision!");
        }

        if (showDebugLogs)
        {
            Debug.Log($"[RingBorderCollider] Initialized on {gameObject.name}, layer: {LayerMask.LayerToName(gameObject.layer)}, parent ring: {(parentRing != null ? parentRing.name : "NULL")}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Layer-based validation (extra safety check)
        if (obstacleLayer != -1 && collision.gameObject.layer != obstacleLayer)
        {
            // Collision is not on the obstacle layer - might be environment or other objects
            if (showDebugLogs)
            {
                Debug.Log($"[RingBorderCollider] Collision with {collision.gameObject.name} on layer '{LayerMask.LayerToName(collision.gameObject.layer)}' - ignoring (not obstacle layer)");
            }
            return;
        }

        // Check if it's the player
        if (PlayerDetectionUtility.IsPlayer(collision.collider, out Transform playerTransform))
        {
            if (showDebugLogs)
            {
                Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
                Debug.Log($"[RingBorderCollider] ❌ BORDER HIT! Player collided with ring frame at {hitPoint}");
                Debug.Log($"[RingBorderCollider] Collision details: {collision.contacts.Length} contact points, relative velocity: {collision.relativeVelocity.magnitude:F2}");
            }

            // Notify parent ring about border hit
            if (parentRing != null)
            {
                parentRing.OnBorderHit(playerTransform.gameObject);
            }
            else
            {
                Debug.LogError("[RingBorderCollider] Cannot notify parent - RingObstacle is null!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the border collider in red
        Gizmos.color = Color.red;

        MeshCollider meshCol = GetComponent<MeshCollider>();
        if (meshCol != null && meshCol.sharedMesh != null)
        {
            Gizmos.DrawWireMesh(meshCol.sharedMesh, transform.position, transform.rotation, transform.lossyScale);
        }
    }
}
