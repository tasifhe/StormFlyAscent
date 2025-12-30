using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using Dreamteck.Forever;

/// <summary>
/// Manages scene transitions and loading screens
/// </summary>
public class MenuSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "PLAYGROUND"; // Change to your gameplay scene
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TMPro.TextMeshProUGUI loadingPercentageText;
    [SerializeField] private TMPro.TextMeshProUGUI loadingTipText;
    [SerializeField] private Image fadeImage;
    
    [Header("Loading Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float minimumLoadingTime = 2.0f; // Minimum time to show loading panel (increased for proper loading)
    [SerializeField] private float additionalDisplayTime = 0.5f; // Extra time to show panel after loading completes
    [SerializeField] private float minimumHoldAtEnd = 0.5f; // Hold at 100% before fading (for visual clarity)
    [SerializeField] private bool keepLoadingPanelForLevelGen = false; // Keep panel visible for level generation (DISABLED - let level gen happen at runtime)
    
    [Header("Menu References")]
    [SerializeField] private GameObject mainMenuManager; // Reference to MainMenuManager GameObject to hide it
    private MainMenuManager cachedMenuManager; // Cached reference to avoid repeated FindFirstObjectByType calls
    
    [Header("Loading Tips")]
    [SerializeField] private string[] loadingTips = new string[]
    {
        "Tip: Use joystick to steer your bird left and right",
        "Tip: Tap the screen to boost your speed",
        "Tip: Collect coins to unlock new bird skins",
        "Tip: The bird alternates between flapping and gliding",
        "Tip: Avoid obstacles in the storm!",
        "Tip: Higher altitude increases your speed when diving",
        "Tip: Practice smooth movements for better control"
    };
    
    private bool isLoading = false;
    private bool waitingForLevelGen = false;
    private string targetSceneName = "";
    
    private void Awake()
    {
        // Persist across scenes (CRITICAL for loading panel to work)
        DontDestroyOnLoad(gameObject);
        
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Cache MainMenuManager reference early (before it gets destroyed or moved)
        if (mainMenuManager != null)
        {
            cachedMenuManager = mainMenuManager.GetComponent<MainMenuManager>();
        }
        else
        {
            cachedMenuManager = FindFirstObjectByType<MainMenuManager>();
        }
        
        // If loading panel is on a separate Canvas, persist that too
        if (loadingPanel != null)
        {
            Canvas loadingCanvas = loadingPanel.GetComponentInParent<Canvas>();
            if (loadingCanvas != null && loadingCanvas.gameObject != gameObject)
            {
                DontDestroyOnLoad(loadingCanvas.gameObject);
                Debug.Log($"Set DontDestroyOnLoad on loading canvas: {loadingCanvas.gameObject.name}");
            }
        }
        
        // Initially hide everything
        EnsureLoadingPanelHidden();
    }
    
    /// <summary>
    /// Ensure loading panel is found and hidden
    /// </summary>
    private void EnsureLoadingPanelHidden()
    {
        // Try to use the assigned reference first
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            if (fadeImage != null)
                fadeImage.gameObject.SetActive(false);
            Debug.Log("Loading panel hidden via reference");
            return;
        }
        
        // If reference is lost, try to find it
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Loading") && (obj.name.Contains("Panel") || obj.name.Contains("Pannel")))
            {
                obj.SetActive(false);
                Debug.Log($"Found and hidden loading panel by name: {obj.name}");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when a scene finishes loading
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"=== OnSceneLoaded: {scene.name} ===");
        
        // If we loaded the main menu scene, re-cache the menu manager
        if (scene.name == mainMenuSceneName)
        {
            StartCoroutine(CacheMenuManagerDelayed());
        }
    }
    
    /// <summary>
    /// Cache menu manager reference after scene loads
    /// </summary>
    private IEnumerator CacheMenuManagerDelayed()
    {
        // Wait a frame for all objects to initialize
        yield return new WaitForEndOfFrame();
        
        if (mainMenuManager != null)
        {
            cachedMenuManager = mainMenuManager.GetComponent<MainMenuManager>();
            Debug.Log("Re-cached MainMenuManager from assigned reference");
        }
        else
        {
            cachedMenuManager = FindFirstObjectByType<MainMenuManager>();
            if (cachedMenuManager != null)
            {
                Debug.Log("Re-cached MainMenuManager from scene search");
            }
        }
    }
    
    #region Public Methods
    
    /// <summary>
    /// Load the main gameplay scene
    /// </summary>
    public void LoadGameScene()
    {
        if (!isLoading)
        {
            Debug.Log($"LoadGameScene called, loading: {gameSceneName}");
            StartCoroutine(LoadSceneAsync(gameSceneName));
        }
        else
        {
            Debug.LogWarning("LoadGameScene called but already loading!");
        }
    }
    
    /// <summary>
    /// Load the main menu scene
    /// </summary>
    public void LoadMainMenuScene()
    {
        if (!isLoading)
        {
            // Clear cached menu manager when returning to menu
            cachedMenuManager = null;
            Debug.Log($"LoadMainMenuScene called, loading: {mainMenuSceneName}");
            StartCoroutine(LoadSceneAsync(mainMenuSceneName));
        }
        else
        {
            Debug.LogWarning("LoadMainMenuScene called but already loading!");
        }
    }
    
    /// <summary>
    /// Reload the current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        if (!isLoading)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            StartCoroutine(LoadSceneAsync(currentScene));
        }
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    #endregion
    
    #region Scene Loading
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        Debug.Log($"=== LoadSceneAsync STARTED for: {sceneName} ===");
        
        // Validate loading panel before starting
        if (loadingPanel == null)
        {
            Debug.LogError("Loading panel reference is NULL at start!");
            yield break;
        }
        
        isLoading = true;
        float loadStartTime = Time.time;
        
        Debug.Log($"Starting load of scene: {sceneName}");
        
        // FIRST: Hide main menu panels immediately before loading
        HideMainMenuPanels();
        
        // Show loading panel FIRST
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            Debug.Log($"Loading panel activated. Active: {loadingPanel.activeInHierarchy}");
        }
        
        // Display random loading tip
        ShowRandomLoadingTip();
        
        // Initialize at 0%
        UpdateLoadingUI(0f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Loading... 0%";
        
        // CRITICAL: Ensure fade image is opaque black and behind UI
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0, 0, 0, 1f); // Solid black
            fadeImage.transform.SetAsFirstSibling(); // Behind everything
            
            // Make sure it fills the screen
            RectTransform rt = fadeImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
            }
        }
        
        // Ensure loading UI is on top
        if (loadingProgressBar != null)
            loadingProgressBar.transform.SetAsLastSibling();
        if (loadingPercentageText != null)
            loadingPercentageText.transform.SetAsLastSibling();
        if (loadingTipText != null)
            loadingTipText.transform.SetAsLastSibling();
        
        yield return new WaitForSeconds(0.1f); // Let UI settle
        
        // Start loading scene in background
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        // Show loading progress (use configured minimum time)
        // Ensure minimum time is met for smooth loading experience
        while (asyncLoad.progress < 0.9f || (Time.time - loadStartTime) < minimumLoadingTime)
        {
            float progress = asyncLoad.progress / 0.9f;
            float timeProgress = Mathf.Clamp01((Time.time - loadStartTime) / minimumLoadingTime);
            float displayProgress = Mathf.Max(progress, timeProgress) * 0.5f; // 0-50%
            
            UpdateLoadingUI(displayProgress);
            if (loadingPercentageText != null)
                loadingPercentageText.text = $"Loading... {(int)(displayProgress * 100)}%";
            yield return null;
        }
        
        // Scene loaded to 90%
        UpdateLoadingUI(0.5f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Loading... 50%";
        
        yield return new WaitForSeconds(0.3f);
        
        // Show activating and progressing
        UpdateLoadingUI(0.6f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Activating... 60%";
        
        yield return new WaitForSeconds(0.2f);
        
        UpdateLoadingUI(0.8f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Preparing... 80%";
        
        // Activate scene
        asyncLoad.allowSceneActivation = true;
        
        // Wait for activation
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Debug.Log("Scene activated, showing final progress");
        
        // Show 100% and hold
        UpdateLoadingUI(1f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Ready... 100%";
        
        // Hold at 100% for minimum time
        Debug.Log($"Holding at 100% for {minimumHoldAtEnd} seconds...");
        yield return new WaitForSeconds(minimumHoldAtEnd);
        
        // Additional display time
        yield return new WaitForSeconds(additionalDisplayTime);
        
        Debug.Log("Starting fade out...");
        
        // Smooth fade out and hide
        if (fadeImage != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                t = t * t * (3f - 2f * t); // Smoothstep easing
                fadeImage.color = Color.Lerp(new Color(0, 0, 0, 1f), new Color(0, 0, 0, 0f), t);
                yield return null;
            }
            fadeImage.color = new Color(0, 0, 0, 0f);
            fadeImage.gameObject.SetActive(false);
        }
        
        HideLoadingPanel();
        
        isLoading = false;
        waitingForLevelGen = false;
        
        Debug.Log("=== LoadSceneAsync COMPLETED - gameplay starts immediately ===");
    }
    
    /// <summary>
    /// Hide loading panel with fallbacks
    /// </summary>
    private void HideLoadingPanel()
    {
        bool success = false;
        
        // Attempt 1: Use assigned reference
        if (loadingPanel != null)
        {
            Debug.Log($"Attempt 1: Hiding via reference. Active state: {loadingPanel.activeInHierarchy}");
            loadingPanel.SetActive(false);
            
            if (!loadingPanel.activeInHierarchy)
            {
                success = true;
                Debug.Log("SUCCESS: Loading panel hidden via reference");
            }
        }
        
        // Attempt 2: Find by name if reference failed
        if (!success)
        {
            Debug.LogWarning("Attempt 2: Reference failed, searching by name...");
            GameObject foundPanel = GameObject.Find("Loading Panel");
            if (foundPanel == null)
                foundPanel = GameObject.Find("Loading Pannel");
            
            if (foundPanel != null)
            {
                foundPanel.SetActive(false);
                Debug.Log($"SUCCESS: Hidden panel via Find: {foundPanel.name}");
                success = true;
            }
        }
        
        // Attempt 3: Search all DontDestroyOnLoad objects
        if (!success)
        {
            Debug.LogWarning("Attempt 3: Searching DontDestroyOnLoad objects...");
            GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
            foreach (GameObject obj in allObjects)
            {
                // Check if it's in DontDestroyOnLoad (scene name will be null)
                if (obj.scene.name == null && obj.name.ToLower().Contains("loading"))
                {
                    obj.SetActive(false);
                    Debug.Log($"Hidden DontDestroyOnLoad object: {obj.name}");
                    success = true;
                }
            }
        }
        
        if (!success)
        {
            Debug.LogError("FAILED: Could not hide loading panel with any method!");
        }
    }
    
    /// <summary>
    /// Hide all main menu panels to show gameplay
    /// </summary>
    private void HideMainMenuPanels()
    {
        // Only hide menu if we're in the main menu scene
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != mainMenuSceneName)
        {
            // Not in menu scene, nothing to hide
            return;
        }
        
        // Use cached reference first
        if (cachedMenuManager != null)
        {
            cachedMenuManager.HideAllPanels();
            Debug.Log("Main menu panels hidden via cached reference");
            return;
        }
        
        // Try assigned GameObject reference
        if (mainMenuManager != null)
        {
            mainMenuManager.SetActive(false);
            Debug.Log("Main menu manager disabled via reference");
            return;
        }
        
        // Last resort: Find by name in scene (not DontDestroyOnLoad)
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject obj in rootObjects)
        {
            MainMenuManager menuManager = obj.GetComponentInChildren<MainMenuManager>(true);
            if (menuManager != null)
            {
                menuManager.HideAllPanels();
                Debug.Log("Main menu manager found in scene and hidden");
                return;
            }
        }
        
        Debug.LogWarning("Could not find MainMenuManager to hide");
    }
    
    private float currentProgress = 0f;
    
    private void UpdateLoadingUI(float targetProgress)
    {
        // Smooth progress bar with immediate percentage update
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = targetProgress;
            Debug.Log($"UpdateLoadingUI: Set progress bar to {targetProgress * 100}%");
        }
        else
        {
            Debug.LogWarning("UpdateLoadingUI: loadingProgressBar is NULL!");
        }
        
        // Update percentage text
        if (loadingPercentageText != null)
        {
            loadingPercentageText.text = Mathf.RoundToInt(targetProgress * 100f) + "%";
        }
        else
        {
            Debug.LogWarning("UpdateLoadingUI: loadingPercentageText is NULL!");
        }
        
        // Force canvas update
        Canvas.ForceUpdateCanvases();
    }
    
    private void ShowRandomLoadingTip()
    {
        if (loadingTipText != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            loadingTipText.text = loadingTips[randomIndex];
        }
    }
    
    #endregion
}
