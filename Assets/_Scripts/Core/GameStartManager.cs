using UnityEngine;
using Dreamteck.Forever;
using System.Collections;

/// <summary>
/// Manages game initialization - waits for level to be ready before starting character
/// </summary>
public class GameStartManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Character character;
    [SerializeField] private CountdownController countdownController;

    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 5f;

    private bool hasStarted = false;

    private void Awake()
    {
        // PAUSE THE GAME IMMEDIATELY - before anything can move
        Time.timeScale = 0f;
        Debug.Log("[GameStartManager] Game paused immediately on scene load");

        // Auto-find character (OPTIONAL - game can work without it)
        if (character == null)
        {
            character = FindFirstObjectByType<Character>();
            if (character == null)
            {
                Debug.LogWarning("[GameStartManager] Character component not found - continuing without it");
            }
        }

        // Auto-find countdown controller
        if (countdownController == null)
        {
            countdownController = FindFirstObjectByType<CountdownController>();
        }

        // Disable ALL character components to prevent ANY movement
        if (character != null)
        {
            // Disable Character script
            character.enabled = false;

            // Disable Path Follower (Movement)
            var follower = character.GetComponent<CharacterPathFollower>();
            if (follower != null)
            {
                follower.enabled = false;
                Debug.Log("[GameStartManager] Disabled CharacterPathFollower");
            }

            // Disable Input
            var input = character.GetComponent<CharacterInput>();
            if (input != null)
            {
                input.enabled = false;
                Debug.Log("[GameStartManager] Disabled CharacterInput");
            }

            // Disable Animation (Optional, keeps it in T-pose if disabled, maybe keep enabled for idle?)
            // Keeping animation enabled usually looks better (idle loop)

            Debug.Log("[GameStartManager] Character components disabled until game starts");
        }
    }

    private void Start()
    {
        StartCoroutine(WaitForLevelAndStart());
    }

    private IEnumerator WaitForLevelAndStart()
    {
        Debug.Log("[GameStartManager] Waiting for level...");

        float waitStart = Time.time;

        // Wait for LevelGenerator instance
        while (LevelGenerator.instance == null)
        {
            if (Time.time - waitStart > maxWaitTime)
            {
                Debug.LogWarning("[GameStartManager] LevelGenerator timeout, starting anyway");
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
                    Debug.LogWarning("[GameStartManager] Level not ready, starting anyway");
                    break;
                }
                yield return null;
            }
        }

        // Single frame to ensure everything is initialized
        yield return null;

        // Game is already paused from Awake - start countdown immediately
        Debug.Log("[GameStartManager] Level ready, starting countdown");

        // Start countdown if available, otherwise start game directly
        if (countdownController != null)
        {
            countdownController.OnCountdownComplete += OnCountdownFinished;
            countdownController.StartCountdown();
        }
        else
        {
            Debug.LogWarning("[GameStartManager] CountdownController not found, starting game directly");
            Time.timeScale = 1f; // Resume if no countdown
            StartGame();
        }
    }

    private void OnCountdownFinished()
    {
        // Unsubscribe from event
        if (countdownController != null)
        {
            countdownController.OnCountdownComplete -= OnCountdownFinished;
        }

        StartGame();
    }

    private void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Enable and initialize character (if it exists)
        if (character != null)
        {
            // Enable Path Follower first
            var follower = character.GetComponent<CharacterPathFollower>();
            if (follower != null) follower.enabled = true;

            // Enable Input
            var input = character.GetComponent<CharacterInput>();
            if (input != null) input.enabled = true;

            // Finally enable Character and Initialize
            character.enabled = true;
            character.InitializeCharacter();
            Debug.Log("[GameStartManager] ✅ Character started!");
        }
        else
        {
            Debug.Log("[GameStartManager] ✅ Game started (no Character component)");
        }

        Debug.Log("[GameStartManager] ✅ Game started!");
    }

    public bool HasStarted => hasStarted;
}
