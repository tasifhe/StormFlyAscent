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

    [Header("Settings")]
    [SerializeField] private float maxWaitTime = 5f;

    private bool hasStarted = false;

    private void Awake()
    {
        // Auto-find character
        if (character == null)
        {
            character = FindFirstObjectByType<Character>();
            if (character == null)
            {
                Debug.LogError("GameStartManager: Character not found!");
                enabled = false;
                return;
            }
        }

        // Disable character until level is ready
        character.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(WaitForLevelAndStart());
    }

    private IEnumerator WaitForLevelAndStart()
    {
        Debug.Log("GameStartManager: Waiting for level...");

        float waitStart = Time.time;

        // Wait for LevelGenerator instance
        while (LevelGenerator.instance == null)
        {
            if (Time.time - waitStart > maxWaitTime)
            {
                Debug.LogWarning("LevelGenerator timeout, starting anyway");
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
                    Debug.LogWarning("Level not ready, starting anyway");
                    break;
                }
                yield return null;
            }
        }

        // Extra frame to ensure everything is set up
        yield return new WaitForEndOfFrame();

        // Start the game
        StartGame();
    }

    private void StartGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        // Enable and initialize character
        if (character != null)
        {
            character.enabled = true;
            character.InitializeCharacter();
            Debug.Log("✅ Character started!");
        }

        Debug.Log("✅ Game started!");
    }

    public bool HasStarted => hasStarted;
}
