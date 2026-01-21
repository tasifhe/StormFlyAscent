using UnityEngine;

/// <summary>
/// Automatically plays designated music when this scene starts
/// Place this in your Main Menu or Level scenes
/// </summary>
public class SceneMusicController : MonoBehaviour
{
    [Header("Music Settings")]
    [Tooltip("The music clip to play for this scene")]
    [SerializeField] private AudioClip sceneMusic;

    [Tooltip("Crossfade duration in seconds")]
    [SerializeField] private float fadeDuration = 1.0f;

    [Tooltip("Should the music loop?")]
    [SerializeField] private bool loopMusic = true;

    [Tooltip("Start playing immediately on Start?")]
    [SerializeField] private bool playOnStart = true;

    private void Start()
    {
        if (playOnStart && sceneMusic != null)
        {
            // Use a slight delay to ensure AudioManager is ready if it's the very first scene
            // though script execution order usually handles this, a coroutine is safer
            StartCoroutine(PlayMusicRoutine());
        }
    }

    private System.Collections.IEnumerator PlayMusicRoutine()
    {
        // Wait one frame to ensure singletons are initialized
        yield return null;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(sceneMusic, true, loopMusic);
            Debug.Log($"[SceneMusic] Playing: {sceneMusic.name}");
        }
        else
        {
            Debug.LogWarning("[SceneMusic] AudioManager not found! Make sure 'GameManagers' is in the scene.");
        }
    }

    public void PlayNow()
    {
        if (AudioManager.Instance != null && sceneMusic != null)
        {
            AudioManager.Instance.PlayMusic(sceneMusic, true, loopMusic);
        }
    }
}
