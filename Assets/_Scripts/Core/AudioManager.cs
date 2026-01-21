using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Global Audio Manager handles Music and SFX playback
/// Persists across scenes
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [SerializeField] private float musicFadeDuration = 1.0f;

    private float masterVolume = 1.0f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup sources if missing
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    #region Music Control

    /// <summary>
    /// Play background music with crossfade
    /// </summary>
    public void PlayMusic(AudioClip musicClip, bool fade = true, bool loop = true)
    {
        if (musicClip == null) return;

        // Don't restart if already playing
        if (musicSource.clip == musicClip && musicSource.isPlaying) return;

        musicSource.loop = loop;

        if (fade)
        {
            StartCoroutine(FadeMusic(musicClip));
        }
        else
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Stop music
    /// </summary>
    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
    }

    private IEnumerator FadeMusic(AudioClip newClip)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // Fade out
        while (elapsed < musicFadeDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (musicFadeDuration / 2));
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        elapsed = 0f;
        float targetVolume = SettingsManager.Instance != null ? SettingsManager.Instance.GetMusicVolume() : 1f;

        while (elapsed < musicFadeDuration / 2)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / (musicFadeDuration / 2));
            yield return null;
        }

        musicSource.volume = targetVolume;
    }

    private IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / musicFadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume; // Reset for next play
    }

    #endregion

    #region SFX Control

    /// <summary>
    /// Play a one-shot sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        PlaySFX(clip, volumeScale, 0f);
    }

    /// <summary>
    /// Play a one-shot sound effect with pitch randomization
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale, float pitchVariance)
    {
        if (clip != null && sfxSource != null)
        {
            // Reset pitch to normal before playing
            sfxSource.pitch = 1f;

            if (pitchVariance > 0)
            {
                sfxSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            }

            sfxSource.PlayOneShot(clip, volumeScale);
        }
    }

    /// <summary>
    /// Stop all SFX
    /// </summary>
    public void StopAllSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = volume;
        }
    }

    #endregion
}
