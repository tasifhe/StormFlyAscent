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

        // Disable character until countdown is complete (if character exists)
        if (character != null)
        {
            character.enabled = false;
            Debug.Log("[GameStartManager] Character disabled until game starts");
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

        // Single frame to ensure everything is set up
        yield return new WaitForEndOfFrame();

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
