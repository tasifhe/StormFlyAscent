using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Ensures an EventSystem exists in the scene for UI input to work
/// This is CRITICAL for joystick touch input in builds
/// Attach this to any GameObject (like GameManager or Player)
/// </summary>
[DefaultExecutionOrder(-100)] // Run before other scripts
public class EventSystemEnsurer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool createIfMissing = true;
    [SerializeField] private bool persistAcrossScenes = true;
    
    private void Awake()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        
        if (eventSystem == null && createIfMissing)
        {
            Debug.LogWarning("[EventSystemEnsurer] No EventSystem found! Creating one...");
            CreateEventSystem();
        }
        else if (eventSystem != null)
        {
            Debug.Log($"[EventSystemEnsurer] EventSystem found: {eventSystem.gameObject.name}");
            
            if (persistAcrossScenes)
            {
                DontDestroyOnLoad(eventSystem.gameObject);
            }
        }
    }
    
    private void CreateEventSystem()
    {
        GameObject eventSystemObj = new GameObject("EventSystem");
        EventSystem eventSystem = eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(eventSystemObj);
        }
        
        Debug.Log("[EventSystemEnsurer] EventSystem created successfully!");
    }
}
