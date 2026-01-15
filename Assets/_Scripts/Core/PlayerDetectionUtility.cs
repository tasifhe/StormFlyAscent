using UnityEngine;

/// <summary>
/// Centralized utility for detecting player collisions in obstacles
/// Provides consistent, robust player detection across all obstacle types
/// </summary>
public static class PlayerDetectionUtility
{
    // Cached player transform for performance
    private static Transform cachedPlayer;
    private static bool hasSearchedForPlayer = false;

    /// <summary>
    /// Check if a collider belongs to the player
    /// Returns true if player detected, outputs player transform
    /// </summary>
    public static bool IsPlayer(Collider collider, out Transform playerTransform)
    {
        playerTransform = null;

        if (collider == null) return false;

        // 1. Fast path: Direct tag check
        if (collider.CompareTag("Player"))
        {
            playerTransform = collider.transform;
            CachePlayer(playerTransform);
            return true;
        }

        // 2. Check parent for Player tag (handles child colliders)
        Transform parent = collider.transform.parent;
        if (parent != null && parent.CompareTag("Player"))
        {
            playerTransform = parent;
            CachePlayer(playerTransform);
            return true;
        }

        // 3. Check for Character component (most robust)
        Character character = collider.GetComponentInParent<Character>();
        if (character != null)
        {
            playerTransform = character.transform;
            CachePlayer(playerTransform);
            return true;
        }

        // 4. Fallback: Use cached player if nearby
        if (cachedPlayer != null)
        {
            float distance = Vector3.Distance(collider.transform.position, cachedPlayer.position);
            if (distance < 15f) // Reasonable proximity check
            {
                playerTransform = cachedPlayer;
                return true;
            }
        }

        // 5. Last resort: Search for player in scene (SLOW, only once)
        if (!hasSearchedForPlayer)
        {
            hasSearchedForPlayer = true;
            Character foundCharacter = Object.FindFirstObjectByType<Character>();
            if (foundCharacter != null)
            {
                cachedPlayer = foundCharacter.transform;
                playerTransform = cachedPlayer;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Manually cache the player transform for faster lookups
    /// </summary>
    public static void CachePlayer(Transform player)
    {
        if (player != null && cachedPlayer != player)
        {
            cachedPlayer = player;
            hasSearchedForPlayer = true;
        }
    }

    /// <summary>
    /// Clear the cached player (call when reloading scenes)
    /// </summary>
    public static void ClearCache()
    {
        cachedPlayer = null;
        hasSearchedForPlayer = false;
    }

    /// <summary>
    /// Get the cached player transform (may be null)
    /// </summary>
    public static Transform GetCachedPlayer()
    {
        return cachedPlayer;
    }
}
