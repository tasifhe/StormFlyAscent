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
        FreezeBird();
    }
    
    private void Start()
    {
        StartCoroutine(WaitForLevelGeneratorAndStart());
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
    }
    
    /// <summary>
    /// Unfreeze and start gameplay
    /// </summary>
    private void UnfreezeBird()
    {
        if (birdCharacter != null)
            birdCharacter.InitializeCharacter();
        
        if (birdRigidbody != null)
        {
            birdRigidbody.constraints = RigidbodyConstraints.None;
            birdRigidbody.isKinematic = false;
            birdRigidbody.useGravity = true;
        }
        
        if (birdCharacter != null)
            birdCharacter.enabled = true;
        
        // Enable path following
        if (birdPathFollower != null)
        {
            birdPathFollower.follow = true;
            birdPathFollower.StartFollow();
        }
        
        // if (showDebugLogs)
        //     Debug.Log("✅ GameStartManager: Bird unfrozen, gameplay started!");
    }
    
    /// <summary>
    /// Wait for LevelGenerator to be ready, then start gameplay
    /// MenuSceneManager already handles the loading screen
    /// </summary>
    private IEnumerator WaitForLevelGeneratorAndStart()
    {
        // Wait for LevelGenerator to be ready
        while (LevelGenerator.instance == null || !LevelGenerator.instance.ready)
        {
            yield return null;
        }
        
        // if (showDebugLogs)
        //     Debug.Log("✅ GameStartManager: LevelGenerator is ready - Starting game!");
        
        // Small delay to ensure MenuSceneManager finishes its fade
        yield return new WaitForSeconds(0.2f);
        
        // Start the game
        StartGame();
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
        
        // Unfreeze the bird
        UnfreezeBird();
        
        // Fire event for other systems
        OnGameStarted();
        
        // if (showDebugLogs)
        //     Debug.Log("🎮 GAME STARTED!");
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
