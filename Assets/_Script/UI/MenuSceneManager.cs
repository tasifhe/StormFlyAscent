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
    [SerializeField] private float minimumLoadingTime = 1.5f; // Minimum time to show loading panel
    [SerializeField] private float additionalDisplayTime = 0.5f; // Extra time to show panel after loading completes
    [SerializeField] private bool keepLoadingPanelForLevelGen = true; // Keep panel visible for level generation
    
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
        Debug.Log($"=== OnSceneLoaded: {scene.name}, isLoading={isLoading}, waitingForLevelGen={waitingForLevelGen} ===");
        
        // If we loaded the main menu scene, re-cache the menu manager
        if (scene.name == mainMenuSceneName)
        {
            StartCoroutine(CacheMenuManagerDelayed());
        }
        
        // If we're waiting for the game scene to load and this is it, continue loading sequence
        if (waitingForLevelGen && scene.name == gameSceneName)
        {
            Debug.Log("=== Game scene loaded, continuing loading sequence ===");
            waitingForLevelGen = false;
            StartCoroutine(ContinueGameSceneLoading());
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
    
    /// <summary>
    /// Continue loading sequence after game scene loads
    /// </summary>
    private IEnumerator ContinueGameSceneLoading()
    {
        Debug.Log("=== ContinueGameSceneLoading STARTED ===");
        
        // Wait for scene to fully initialize
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        // Re-validate loading panel reference
        if (!ValidateLoadingPanel())
        {
            Debug.LogError("Loading panel lost after scene load! Trying to recover...");
            if (!RecoverLoadingPanelReference())
            {
                Debug.LogError("Failed to recover loading panel reference!");
                isLoading = false;
                yield break;
            }
        }
        
        // Ensure Canvas is on top
        Canvas loadingCanvas = loadingPanel.GetComponentInParent<Canvas>();
        if (loadingCanvas != null)
        {
            loadingCanvas.sortingOrder = 1000;
            Debug.Log($"Set loading canvas sorting order to {loadingCanvas.sortingOrder}");
        }
        
        // Update progress
        UpdateLoadingUI(0.6f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Scene Loaded... 60%";
        
        yield return new WaitForSeconds(0.3f);
        
        // Now wait for level generator
        Debug.Log("=== Starting WaitForLevelGenerator from scene loaded event ===");
        yield return StartCoroutine(WaitForLevelGenerator());
        
        Debug.Log("=== ContinueGameSceneLoading COMPLETED ===");
    }
    
    /// <summary>
    /// Validate that loading panel reference is still valid
    /// </summary>
    private bool ValidateLoadingPanel()
    {
        if (loadingPanel == null)
            return false;
        
        if (!loadingPanel.activeInHierarchy)
        {
            Debug.LogWarning("Loading panel became inactive! Re-activating...");
            loadingPanel.SetActive(true);
        }
        
        return true;
    }
    
    /// <summary>
    /// Try to recover loading panel reference if it was lost
    /// </summary>
    private bool RecoverLoadingPanelReference()
    {
        // Search for loading panel by name
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            if ((obj.name == "Loading Panel" || obj.name == "Loading Pannel") && obj.scene.name == null)
            {
                loadingPanel = obj;
                Debug.Log($"Recovered loading panel reference: {obj.name}");
                loadingPanel.SetActive(true);
                
                // Also try to recover other references
                if (loadingProgressBar == null)
                    loadingProgressBar = loadingPanel.GetComponentInChildren<Slider>();
                if (loadingPercentageText == null || loadingTipText == null)
                {
                    var texts = loadingPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>();
                    if (texts.Length > 0 && loadingPercentageText == null)
                        loadingPercentageText = texts[0];
                    if (texts.Length > 1 && loadingTipText == null)
                        loadingTipText = texts[1];
                }
                if (fadeImage == null)
                    fadeImage = loadingPanel.GetComponentInChildren<Image>();
                
                return true;
            }
        }
        
        return false;
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
            Debug.LogError("Loading panel reference is NULL at start! Trying to recover...");
            if (!RecoverLoadingPanelReference())
            {
                Debug.LogError("Cannot start loading without panel reference!");
                yield break;
            }
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
        while (asyncLoad.progress < 0.9f || (Time.time - loadStartTime) < minimumLoadingTime)
        {
            float progress = asyncLoad.progress / 0.9f;
            UpdateLoadingUI(progress * 0.5f); // 0-50%
            if (loadingPercentageText != null)
                loadingPercentageText.text = $"Loading... {(int)(progress * 50)}%";
            yield return null;
        }
        
        // Scene loaded to 90%
        UpdateLoadingUI(0.5f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Loading... 50%";
        
        yield return new WaitForSeconds(0.3f);
        
        // Check if this is the game scene that needs level generator
        bool isGameScene = sceneName == gameSceneName;
        
        Debug.Log($"Scene activation ready. isGameScene: {isGameScene}, keepLoadingPanelForLevelGen: {keepLoadingPanelForLevelGen}");
        
        if (isGameScene && keepLoadingPanelForLevelGen)
        {
            Debug.Log("=== Taking GAME SCENE loading path (with LevelGenerator wait) ===");
            // Show activating
            UpdateLoadingUI(0.55f);
            if (loadingPercentageText != null)
                loadingPercentageText.text = "Activating... 55%";
            
            // Set flag so OnSceneLoaded knows to continue the sequence
            waitingForLevelGen = true;
            targetSceneName = sceneName;
            
            // Activate scene - OnSceneLoaded will continue the loading sequence
            Debug.Log("Activating game scene, OnSceneLoaded event will continue loading...");
            asyncLoad.allowSceneActivation = true;
            
            // DON'T wait for isDone here - just exit the coroutine
            // The OnSceneLoaded event will handle the rest
            Debug.Log("=== LoadSceneAsync complete, waiting for OnSceneLoaded event ===");
        }
        else
        {
            Debug.Log("=== Taking NORMAL SCENE loading path (simple fade) ===");
            // Normal scene - just activate
            UpdateLoadingUI(1f);
            if (loadingPercentageText != null)
                loadingPercentageText.text = "Ready!";
            asyncLoad.allowSceneActivation = true;
            
            // Wait for activation
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Smooth fade out and hide
            yield return new WaitForSeconds(0.2f);
            
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
            
            Debug.Log("=== LoadSceneAsync COMPLETED (normal path) ===");
        }
    }
    
    /// <summary>
    /// Wait for level generator to be ready before revealing gameplay
    /// </summary>
    private IEnumerator WaitForLevelGenerator()
    {
        Debug.Log("=== WaitForLevelGenerator STARTED ===");
        UpdateLoadingUI(0.6f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Initializing... 60%";
        
        Debug.Log("Waiting for LevelGenerator and BirdController to be ready...");
        
        float totalWaitStart = Time.time;
        float maxWaitTime = 8f; // Maximum 8 seconds total wait
        
        // Wait a frame for scene to settle
        yield return new WaitForEndOfFrame();
        
        // Wait for LevelGenerator instance with shorter timeout
        float waitStart = Time.time;
        while (LevelGenerator.instance == null)
        {
            // Check if we've exceeded total wait time
            if (Time.time - totalWaitStart > maxWaitTime)
            {
                Debug.LogWarning("Exceeded max wait time, proceeding anyway");
                break;
            }
            
            if (Time.time - waitStart > 2f)
            {
                Debug.LogWarning("LevelGenerator instance not found after 2 seconds, but continuing...");
                break;
            }
            yield return null;
        }
        
        // If no LevelGenerator found, just wait a bit and finish
        if (LevelGenerator.instance == null)
        {
            Debug.LogWarning("No LevelGenerator found - completing loading anyway");
            
            UpdateLoadingUI(0.8f);
            if (loadingPercentageText != null)
                loadingPercentageText.text = "Almost Ready... 80%";
            
            yield return new WaitForSeconds(1f);
            
            // Skip to completion
            yield return StartCoroutine(FinishLoading());
            yield break;
        }
        
        Debug.Log($"Found LevelGenerator instance, current ready state: {LevelGenerator.instance.ready}");
        
        UpdateLoadingUI(0.65f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Found Level Generator... 65%";
        
        yield return new WaitForSeconds(0.2f);
        
        // Wait for level to be ready with timeout
        waitStart = Time.time;
        while (!LevelGenerator.instance.ready)
        {
            // Check total elapsed time
            float totalElapsed = Time.time - totalWaitStart;
            if (totalElapsed > maxWaitTime)
            {
                Debug.LogWarning("Max wait time exceeded, finishing loading");
                break;
            }
            
            // Show smooth progress 65-80% over max 5 seconds
            float elapsed = Time.time - waitStart;
            float progress = 0.65f + (Mathf.Min(elapsed / 5f, 1f) * 0.15f);
            UpdateLoadingUI(progress);
            
            if (loadingPercentageText != null)
                loadingPercentageText.text = $"Preparing Level... {(int)(progress * 100)}%";
            
            yield return null;
            
            // Shorter timeout for ready check
            if (elapsed > 5f)
            {
                Debug.LogWarning("LevelGenerator not ready after 5 seconds, proceeding anyway");
                break;
            }
        }
        
        Debug.Log("LevelGenerator ready, now waiting for BirdController...");
        
        UpdateLoadingUI(0.8f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Finding Bird Controller... 80%";
        
        // Wait for Character (bird) to be ready
        Character birdCharacter = null;
        waitStart = Time.time;
        while (birdCharacter == null)
        {
            birdCharacter = FindFirstObjectByType<Character>();
            
            float totalElapsed = Time.time - totalWaitStart;
            if (totalElapsed > maxWaitTime)
            {
                Debug.LogWarning("Max wait exceeded for Bird Character, finishing anyway");
                break;
            }
            
            if (Time.time - waitStart > 2f)
            {
                Debug.LogWarning("Bird Character not found after 2 seconds, but continuing...");
                break;
            }
            
            yield return null;
        }
        
        if (birdCharacter != null)
        {
            Debug.Log("Found Bird Character, waiting for initialization...");
            UpdateLoadingUI(0.85f);
            if (loadingPercentageText != null)
                loadingPercentageText.text = "Initializing Bird... 85%";
            
            // Give bird character a moment to initialize
            yield return new WaitForSeconds(0.3f);
            
            UpdateLoadingUI(0.9f);
            if (loadingPercentageText != null)
                loadingPercentageText.text = "Ready to fly... 90%";
        }
        else
        {
            Debug.LogWarning("Bird Character not found, but proceeding to finish loading");
        }
        
        Debug.Log("Level and Bird ready, finishing loading sequence");
        
        // Finish loading sequence
        Debug.Log("=== Calling FinishLoading ===");
        yield return StartCoroutine(FinishLoading());
        Debug.Log("=== WaitForLevelGenerator COMPLETED ===");
    }
    
    /// <summary>
    /// Complete the loading sequence and reveal gameplay
    /// </summary>
    private IEnumerator FinishLoading()
    {
        Debug.Log("=== FinishLoading STARTED ===");
        
        // Level ready - show completion
        UpdateLoadingUI(0.95f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Ready... 95%";
        
        Debug.Log("FinishLoading: Showing 95%");
        
        yield return new WaitForSeconds(0.3f);
        
        Debug.Log("FinishLoading: First wait complete, showing 100%");
        UpdateLoadingUI(1f);
        if (loadingPercentageText != null)
            loadingPercentageText.text = "Starting... 100%";
        
        yield return new WaitForSeconds(additionalDisplayTime);
        
        Debug.Log("FinishLoading: Second wait complete, starting fade");
        
        // Smooth fade from black with easing
        if (fadeImage != null)
        {
            float elapsed = 0f;
            Color startColor = new Color(0, 0, 0, 1f);
            Color targetColor = new Color(0, 0, 0, 0f);
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                if (fadeImage != null)
                {
                    // Smoothstep easing for polished fade
                    float t = Mathf.Clamp01(elapsed / fadeDuration);
                    t = t * t * (3f - 2f * t);
                    fadeImage.color = Color.Lerp(startColor, targetColor, t);
                }
                yield return null;
            }
            
            if (fadeImage != null)
            {
                fadeImage.color = targetColor;
                fadeImage.gameObject.SetActive(false);
            }
        }
        
        Debug.Log("FinishLoading: Fade complete, hiding loading panel");
        
        // Hide loading panel with multiple attempts
        HideLoadingPanel();
        
        isLoading = false;
        waitingForLevelGen = false;
        
        Debug.Log("=== FinishLoading COMPLETED - gameplay revealed! ===");
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
