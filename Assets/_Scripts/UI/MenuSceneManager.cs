using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

/// <summary>
/// Professional scene manager with async loading and proper error handling
/// Manages all scene transitions with loading screen integration
/// </summary>
public class MenuSceneManager : MonoBehaviour
{
    [Header("Scene Configuration")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "Level 1";

    [Header("Loading Configuration")]
    [SerializeField] private float minimumLoadTime = 1.5f;
    [SerializeField] private bool simulateProgress = false;
    [SerializeField] private float progressSimulationSpeed = 0.5f;

    [Header("References")]
    [SerializeField] private LoadingScreenManager loadingScreen;

    // State tracking
    private bool isLoading = false;
    private static MenuSceneManager instance;

    public static MenuSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<MenuSceneManager>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[MenuSceneManager] Initialized and set to DontDestroyOnLoad");
        }
        else if (instance != this)
        {
            Debug.Log("[MenuSceneManager] Duplicate instance found, destroying");
            Destroy(gameObject);
            return;
        }

        // Find or validate loading screen
        if (loadingScreen == null)
        {
            loadingScreen = LoadingScreenManager.Instance;
            if (loadingScreen == null)
            {
                Debug.LogWarning("[MenuSceneManager] LoadingScreenManager not found in scene");
            }
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    #region Public API

    /// <summary>
    /// Load the game scene asynchronously
    /// </summary>
    public async void LoadGameScene()
    {
        if (isLoading)
        {
            Debug.LogWarning("[MenuSceneManager] Already loading a scene");
            return;
        }

        await LoadSceneAsync(gameSceneName);
    }

    /// <summary>
    /// Load the main menu scene asynchronously
    /// </summary>
    public async void LoadMainMenuScene()
    {
        if (isLoading)
        {
            Debug.LogWarning("[MenuSceneManager] Already loading a scene");
            return;
        }

        await LoadSceneAsync(mainMenuSceneName);
    }

    /// <summary>
    /// Reload the current scene
    /// </summary>
    public async void ReloadCurrentScene()
    {
        if (isLoading)
        {
            Debug.LogWarning("[MenuSceneManager] Already loading a scene");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        await LoadSceneAsync(currentScene);
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[MenuSceneManager] Quitting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Check if currently loading
    /// </summary>
    public bool IsLoading => isLoading;

    #endregion

    #region Async Scene Loading

    /// <summary>
    /// Validate if scene exists in build settings
    /// </summary>
    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Load scene asynchronously with professional loading screen
    /// </summary>
    private async Task LoadSceneAsync(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[MenuSceneManager] Scene name is null or empty");
            return;
        }

        // Validate scene exists in build settings
        if (!IsSceneInBuildSettings(sceneName))
        {
            Debug.LogError($"[MenuSceneManager] Scene '{sceneName}' not found in build settings!");
            ShowErrorMessage($"Cannot load scene: {sceneName} not in build settings");
            return;
        }

        isLoading = true;
        float startTime = Time.realtimeSinceStartup;

        try
        {
            Debug.Log($"[MenuSceneManager] Starting to load scene: {sceneName}");

            // Show loading screen
            if (loadingScreen != null)
            {
                await loadingScreen.ShowAsync();
            }
            else
            {
                Debug.LogWarning("[MenuSceneManager] No loading screen available");
            }

            // Hide current menu if in main menu
            HideCurrentMenu();

            // Start async scene loading
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            asyncLoad.allowSceneActivation = false;

            // Track progress
            float progress = 0f;

            // Phase 1: Loading scene (0% - 70%)
            while (asyncLoad.progress < 0.9f)
            {
                // Real progress from Unity (LoadingScreenManager handles smoothing)
                progress = asyncLoad.progress / 0.9f * 0.7f;

                if (loadingScreen != null)
                {
                    loadingScreen.SetProgress(progress, "Loading Scene...");
                }

                await Task.Yield();
            }

            // Phase 2: Preparing assets (70% - 85%)
            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(0.7f, "Preparing Assets...");
            }

            await Task.Delay(200);

            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(0.85f, "Almost Ready...");
            }

            await Task.Delay(200);

            // Phase 3: Finalizing (85% - 95%)
            if (loadingScreen != null)
            {
                loadingScreen.SetProgress(0.95f, "Finalizing...");
            }

            // Ensure minimum load time for smooth UX
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            if (elapsedTime < minimumLoadTime)
            {
                float remainingTime = minimumLoadTime - elapsedTime;
                Debug.Log($"[MenuSceneManager] Load finished early ({elapsedTime:F2}s). Waiting additional {remainingTime:F2}s to meet minimum display time.");
                await Task.Delay((int)(remainingTime * 1000));
            }

            // Activate the scene
            asyncLoad.allowSceneActivation = true;

            // Wait for scene to fully activate
            while (!asyncLoad.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"[MenuSceneManager] Scene {sceneName} loaded successfully");

            // CRITICAL: Pause the game immediately so it doesn't start playing during loading screen fadeout
            Time.timeScale = 0f;
            Debug.Log("[MenuSceneManager] Game paused during loading screen completion");

            // Complete loading (95% - 100%)
            if (loadingScreen != null)
            {
                await loadingScreen.CompleteLoadingAsync();
            }

            // Post-load initialization
            await OnSceneLoadedAsync(sceneName);

            // IMPORTANT: Resume the game AFTER loading screen is fully hidden
            Time.timeScale = 1f;
            Debug.Log("[MenuSceneManager] Game resumed - loading complete!");


        }
        catch (Exception ex)
        {
            Debug.LogError($"[MenuSceneManager] Error loading scene {sceneName}: {ex.Message}\n{ex.StackTrace}");

            // Show user-friendly error message
            ShowErrorMessage($"Failed to load scene: {sceneName}. Please restart the game.");

            // Hide loading screen on error
            if (loadingScreen != null && loadingScreen.IsLoading)
            {
                await loadingScreen.HideAsync();
            }
        }
        finally
        {
            isLoading = false;
            Debug.Log($"[MenuSceneManager] Loading process completed");
        }
    }

    /// <summary>
    /// Called after scene is loaded
    /// </summary>
    private async Task OnSceneLoadedAsync(string sceneName)
    {
        // Wait a frame for scene to initialize
        await Task.Yield();

        // Scene-specific initialization
        if (sceneName == mainMenuSceneName)
        {
            Debug.Log("[MenuSceneManager] Main menu loaded, initializing menu systems");
            InitializeMainMenu();
        }
        else if (sceneName == gameSceneName)
        {
            Debug.Log("[MenuSceneManager] Game scene loaded, initializing gameplay");
            InitializeGameplay();
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Hide current menu panels
    /// </summary>
    private void HideCurrentMenu()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == mainMenuSceneName)
        {
            MainMenuManager menuManager = FindFirstObjectByType<MainMenuManager>();
            if (menuManager != null)
            {
                menuManager.HideAllPanels();
                Debug.Log("[MenuSceneManager] Hidden main menu panels");
            }
        }
    }

    /// <summary>
    /// Initialize main menu after loading
    /// </summary>
    private void InitializeMainMenu()
    {
        MainMenuManager menuManager = FindFirstObjectByType<MainMenuManager>();
        if (menuManager != null)
        {
            menuManager.ShowMainMenu();
        }
    }

    /// <summary>
    /// Initialize gameplay after loading
    /// </summary>
    private void InitializeGameplay()
    {
        // Resume time in case it was paused
        Time.timeScale = 1f;

        // Additional gameplay initialization can go here
    }

    /// <summary>
    /// Show error message to user (basic implementation)
    /// TODO: Create proper error UI panel
    /// </summary>
    private void ShowErrorMessage(string message)
    {
        Debug.LogError($"[MenuSceneManager] ERROR: {message}");
        // For now, just log. In future, show error UI panel
        // Reset loading state so user can retry
        isLoading = false;
    }

    #endregion
}
