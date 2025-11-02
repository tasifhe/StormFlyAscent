using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages scene transitions and loading screens
/// </summary>
public class MenuSceneManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    [SerializeField] private string gameSceneName = "SampleScene"; // Change to your gameplay scene
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Slider loadingProgressBar;
    [SerializeField] private TMPro.TextMeshProUGUI loadingPercentageText;
    [SerializeField] private TMPro.TextMeshProUGUI loadingTipText;
    [SerializeField] private Image fadeImage;
    
    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadingTime = 1f;
    [SerializeField] private float fadeDuration = 0.5f;
    
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
    
    private void Awake()
    {
        // Make sure loading panel is hidden on start
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
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
            StartCoroutine(LoadSceneAsync(gameSceneName));
        }
    }
    
    /// <summary>
    /// Load the main menu scene
    /// </summary>
    public void LoadMainMenuScene()
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(mainMenuSceneName));
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
        isLoading = true;
        
        Debug.Log("Loading scene: " + sceneName);
        
        // Show loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
        
        // Display random loading tip
        ShowRandomLoadingTip();
        
        // Fade to black
        yield return StartCoroutine(FadeToBlack());
        
        // Start loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        
        float startTime = Time.time;
        float progress = 0f;
        
        // Wait for scene to load (with minimum loading time)
        while (!asyncLoad.isDone)
        {
            // Calculate progress (0.9 is max before allowing scene activation)
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Ensure minimum loading time
            float elapsedTime = Time.time - startTime;
            if (elapsedTime < minimumLoadingTime)
            {
                progress = Mathf.Min(progress, elapsedTime / minimumLoadingTime);
            }
            
            // Update UI
            UpdateLoadingUI(progress);
            
            // Check if loading is complete and minimum time has passed
            if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadingTime)
            {
                // Show 100% briefly
                UpdateLoadingUI(1f);
                yield return new WaitForSeconds(0.3f);
                
                // Activate the scene
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Fade from black
        yield return StartCoroutine(FadeFromBlack());
        
        // Hide loading panel
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        
        isLoading = false;
        
        Debug.Log("Scene loaded: " + sceneName);
    }
    
    private void UpdateLoadingUI(float progress)
    {
        // Update progress bar
        if (loadingProgressBar != null)
        {
            loadingProgressBar.value = progress;
        }
        
        // Update percentage text
        if (loadingPercentageText != null)
        {
            loadingPercentageText.text = Mathf.RoundToInt(progress * 100f) + "%";
        }
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
    
    #region Fade Effects
    
    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
            yield break;
        
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 1f);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
    
    private IEnumerator FadeFromBlack()
    {
        if (fadeImage == null)
            yield break;
        
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(0, 0, 0, 0f);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeImage.color = targetColor;
    }
    
    #endregion
    
    #region Singleton Pattern (Optional)
    
    private static MenuSceneManager instance;
    
    public static MenuSceneManager Instance
    {
        get { return instance; }
    }
    
    private void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    #endregion
}
