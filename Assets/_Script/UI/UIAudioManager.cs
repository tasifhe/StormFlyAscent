using UnityEngine;

/// <summary>
/// Simple audio manager for UI sounds
/// Singleton pattern for easy access from anywhere
/// </summary>
public class UIAudioManager : MonoBehaviour
{
    [Header("UI Sound Effects")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioClip panelOpenSound;
    [SerializeField] private AudioClip panelCloseSound;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;
    
    [Header("Settings")]
    [SerializeField] private float defaultVolume = 1f;
    
    private AudioSource audioSource;
    private static UIAudioManager instance;
    
    public static UIAudioManager Instance
    {
        get { return instance; }
    }
    
    private void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.volume = defaultVolume;
    }
    
    #region Public Methods
    
    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }
    
    public void PlayButtonHover()
    {
        PlaySound(buttonHoverSound);
    }
    
    public void PlayPanelOpen()
    {
        PlaySound(panelOpenSound);
    }
    
    public void PlayPanelClose()
    {
        PlaySound(panelCloseSound);
    }
    
    public void PlaySuccess()
    {
        PlaySound(successSound);
    }
    
    public void PlayError()
    {
        PlaySound(errorSound);
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void PlaySound(AudioClip clip, float volumeScale)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumeScale);
        }
    }
    
    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
        }
    }
    
    #endregion
}
