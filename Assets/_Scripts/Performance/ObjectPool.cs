using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic Object Pooling System for mobile optimization
/// Prevents garbage collection spikes from Instantiate/Destroy calls
/// Usage: ObjectPool.Instance.Get(prefab) and ObjectPool.Instance.Return(obj)
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    
    [Header("Pool Settings")]
    [SerializeField] private int defaultPoolSize = 20;
    [SerializeField] private bool allowPoolExpansion = true;
    [SerializeField] private Transform poolParent;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;
    
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
    private Dictionary<GameObject, string> activeLookup = new Dictionary<GameObject, string>();
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (poolParent == null)
            {
                poolParent = new GameObject("PooledObjects").transform;
                poolParent.SetParent(transform);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Pre-warm a pool with a specific number of objects
    /// Call this in Start() for objects you know you'll need
    /// </summary>
    public void WarmPool(GameObject prefab, int count)
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] Cannot warm pool with null prefab!");
            return;
        }
        
        string key = prefab.name;
        
        if (!pools.ContainsKey(key))
        {
            pools[key] = new Queue<GameObject>();
            prefabLookup[key] = prefab;
        }
        
        for (int i = 0; i < count; i++)
        {
            GameObject obj = CreateNewObject(prefab, key);
            obj.SetActive(false);
            pools[key].Enqueue(obj);
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"[ObjectPool] Warmed pool '{key}' with {count} objects");
        }
    }
    
    /// <summary>
    /// Get an object from the pool (or create new if pool is empty)
    /// </summary>
    public GameObject Get(GameObject prefab)
    {
        return Get(prefab, Vector3.zero, Quaternion.identity);
    }
    
    /// <summary>
    /// Get an object from the pool with position and rotation
    /// </summary>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[ObjectPool] Cannot get null prefab!");
            return null;
        }
        
        string key = prefab.name;
        
        // Create pool if it doesn't exist
        if (!pools.ContainsKey(key))
        {
            pools[key] = new Queue<GameObject>();
            prefabLookup[key] = prefab;
            
            if (showDebugLogs)
            {
                Debug.Log($"[ObjectPool] Created new pool for '{key}'");
            }
        }
        
        GameObject obj;
        
        // Get from pool or create new
        if (pools[key].Count > 0)
        {
            obj = pools[key].Dequeue();
        }
        else
        {
            if (allowPoolExpansion)
            {
                obj = CreateNewObject(prefab, key);
                
                if (showDebugLogs)
                {
                    Debug.Log($"[ObjectPool] Pool '{key}' expanded (created new object)");
                }
            }
            else
            {
                Debug.LogWarning($"[ObjectPool] Pool '{key}' is empty and expansion is disabled!");
                return null;
            }
        }
        
        // Setup object
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        
        // Track active object
        activeLookup[obj] = key;
        
        return obj;
    }
    
    /// <summary>
    /// Return an object to the pool
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("[ObjectPool] Attempting to return null object!");
            return;
        }
        
        // Check if object is from pool
        if (!activeLookup.ContainsKey(obj))
        {
            Debug.LogWarning($"[ObjectPool] Object '{obj.name}' was not taken from pool!");
            Destroy(obj);
            return;
        }
        
        string key = activeLookup[obj];
        activeLookup.Remove(obj);
        
        // Reset object
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        
        // Return to pool
        pools[key].Enqueue(obj);
    }
    
    /// <summary>
    /// Return an object to pool after a delay
    /// </summary>
    public void ReturnAfterDelay(GameObject obj, float delay)
    {
        StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
    }
    
    private System.Collections.IEnumerator ReturnAfterDelayCoroutine(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(obj);
    }
    
    /// <summary>
    /// Clear a specific pool
    /// </summary>
    public void ClearPool(string poolKey)
    {
        if (pools.ContainsKey(poolKey))
        {
            while (pools[poolKey].Count > 0)
            {
                GameObject obj = pools[poolKey].Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            
            pools.Remove(poolKey);
            prefabLookup.Remove(poolKey);
            
            if (showDebugLogs)
            {
                Debug.Log($"[ObjectPool] Cleared pool '{poolKey}'");
            }
        }
    }
    
    /// <summary>
    /// Clear all pools
    /// </summary>
    public void ClearAllPools()
    {
        foreach (var poolKey in pools.Keys)
        {
            while (pools[poolKey].Count > 0)
            {
                GameObject obj = pools[poolKey].Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        
        pools.Clear();
        prefabLookup.Clear();
        activeLookup.Clear();
        
        if (showDebugLogs)
        {
            Debug.Log("[ObjectPool] Cleared all pools");
        }
    }
    
    /// <summary>
    /// Get pool statistics for debugging
    /// </summary>
    public void PrintPoolStats()
    {
        Debug.Log("=== OBJECT POOL STATS ===");
        foreach (var kvp in pools)
        {
            Debug.Log($"Pool '{kvp.Key}': {kvp.Value.Count} available objects");
        }
        Debug.Log($"Total Active Objects: {activeLookup.Count}");
        Debug.Log("========================");
    }
    
    private GameObject CreateNewObject(GameObject prefab, string key)
    {
        GameObject obj = Instantiate(prefab, poolParent);
        obj.name = key; // Remove "(Clone)" from name
        return obj;
    }
}
