using UnityEngine;
using Dreamteck.Forever;
using System.Collections;

/// <summary>
/// Manages game initialization - freezes bird until level is ready
/// Works in coordination with MenuSceneManager for loading screen
/// </summary>
public class GameStartManager : MonoBehaviour
{
    [Header("References (Auto-found if empty)")]
    [SerializeField] private Character birdCharacter;
    [SerializeField] private Rigidbody birdRigidbody;
    [SerializeField] private BirdPathFollower birdPathFollower;
    
    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 10f;
    
    private bool hasStarted = false;
    
    private void Awake()
    {
        FindReferences();
        // Freeze bird until level path is ready
        FreezeBird();
    }
    
    private void Start()
    {
        // Wait for level generator to be ready before starting
        StartCoroutine(WaitForLevelAndStart());
    }
    
    /// <summary>
    /// Wait for LevelGenerator to create the path before starting bird
    /// </summary>
    private IEnumerator WaitForLevelAndStart()
    {
        Debug.Log("GameStartManager: Waiting for LevelGenerator to be ready...");
        
        float waitStart = Time.time;
        float maxWaitTime = 5f;
        
        // Wait for LevelGenerator instance
        while (LevelGenerator.instance == null)
        {
            if (Time.time - waitStart > maxWaitTime)
            {
                Debug.LogWarning("LevelGenerator not found after timeout, starting anyway");
                break;
            }
            yield return null;
        }
        
        // Wait for level to be ready
        if (LevelGenerator.instance != null)
        {
            waitStart = Time.time;
            while (!LevelGenerator.instance.ready)
            {
                if (Time.time - waitStart > maxWaitTime)
                {
                    Debug.LogWarning("LevelGenerator not ready after timeout, starting anyway");
                    break;
                }
                yield return null;
            }
            
            Debug.Log("✅ LevelGenerator is ready!");
        }
        
        // Wait for path follower to initialize and project onto path
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Ensure bird is positioned on the path before unfreezing
        if (birdPathFollower != null && LevelGenerator.instance != null && LevelGenerator.instance.ready)
        {
            // Force bird to project onto path in StartMode.Project
            Debug.Log("Projecting bird onto path...");
            yield return new WaitForSeconds(0.2f);
        }
        
        // Now start the game
        StartGame();
    }
    
    /// <summary>
    /// Auto-find bird references
    /// </summary>
    private void FindReferences()
    {
        if (birdCharacter == null)
            birdCharacter = FindFirstObjectByType<Character>();
        
        if (birdCharacter != null)
        {
            if (birdRigidbody == null)
                birdRigidbody = birdCharacter.GetComponent<Rigidbody>();
            
            if (birdPathFollower == null)
                birdPathFollower = birdCharacter.GetComponent<BirdPathFollower>();
        }
        
        // Validate
        if (birdCharacter == null || birdRigidbody == null || birdPathFollower == null)
        {
            Debug.LogError("GameStartManager: Missing bird components!");
            enabled = false;
        }
    }
    
    /// <summary>
    /// Freeze bird until level ready
    /// </summary>
    private void FreezeBird()
    {
        if (birdRigidbody != null)
        {
            birdRigidbody.constraints = RigidbodyConstraints.FreezeAll;
            birdRigidbody.isKinematic = true;
            birdRigidbody.useGravity = false;
        }
        
        if (birdPathFollower != null)
            birdPathFollower.follow = false;
        
        if (birdCharacter != null)
            birdCharacter.enabled = false;
        
        Debug.Log("🧊 Bird frozen, waiting for level...");
    }
    
    /// <summary>
    /// Unfreeze bird and start gameplay
    /// </summary>
    private void UnfreezeBird()
    {
        if (birdCharacter != null)
        {
            birdCharacter.enabled = true;
            birdCharacter.InitializeCharacter();
        }
        
        if (birdRigidbody != null)
        {
            birdRigidbody.constraints = RigidbodyConstraints.None;
            birdRigidbody.isKinematic = false;
            birdRigidbody.useGravity = true;
        }
        
        // Enable path following
        if (birdPathFollower != null)
        {
            birdPathFollower.follow = true;
            birdPathFollower.StartFollow();
        }
        
        Debug.Log("✅ GameStartManager: Bird unfrozen and started!");
    }
    
    /// <summary>
    /// Start the gameplay
    /// </summary>
    private void StartGame()
    {
        if (hasStarted)
        {
            Debug.LogWarning("⚠️ Game already started!");
            return;
        }
        
        hasStarted = true;
        
        // Unfreeze and start the bird (level is ready)
        UnfreezeBird();
        
        // Fire event for other systems
        OnGameStarted();
        
        Debug.Log("🎮 GAME STARTED - Bird on path and ready!");
    }
    
    /// <summary>
    /// Called when game starts - override or subscribe to this
    /// </summary>
    protected virtual void OnGameStarted()
    {
        // You can fire events here for UI, audio, etc.
    }
    
    /// <summary>
    /// Public property to check if game has started
    /// </summary>
    public bool HasStarted => hasStarted;
}
