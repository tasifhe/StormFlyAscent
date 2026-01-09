using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages the Game Over UI display with restart and main menu buttons
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Settings")]
    [SerializeField] private string gameOverMessage = "GAME OVER";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    [Header("Animation Settings")]
    [SerializeField] private AnimationType animationType = AnimationType.ScaleAndFade;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float delayBeforeShow = 0.3f;
    [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    public enum AnimationType
    {
        Fade,
        Scale,
        ScaleAndFade,
        SlideFromTop,
        SlideFromBottom
    }
    
    private void Awake()
    {
        // Get or add CanvasGroup for fading
        if (canvasGroup == null && gameOverPanel != null)
        {
            canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            }
        }
        
        // Get RectTransform
        if (panelRectTransform == null && gameOverPanel != null)
        {
            panelRectTransform = gameOverPanel.GetComponent<RectTransform>();
        }
    }
    
    private void Start()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
        
        // Subscribe to ObstacleManager
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnGameOver += ShowGameOver;
        }
    }
    
    private void OnEnable()
    {
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnGameOver += ShowGameOver;
        }
    }
    
    private void OnDisable()
    {
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.OnGameOver -= ShowGameOver;
        }
        
        // Remove button listeners
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClicked);
        }
    }
    
    private void ShowGameOver()
    {
        StartCoroutine(ShowGameOverAnimated());
    }
    
    private IEnumerator ShowGameOverAnimated()
    {
        // Wait a bit before showing
        yield return new WaitForSecondsRealtime(delayBeforeShow);
        
        // Activate panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // Set initial state based on animation type
        SetupInitialState();
        
        // Update text content
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage;
        }
        
        if (finalScoreText != null && ObstacleManager.Instance != null)
        {
            var stats = ObstacleManager.Instance.GetStats();
            finalScoreText.text = $"Final Score: {stats.score}\nPassed: {stats.success}/{stats.total} Obstacles";
        }
        
        // Animate
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = animationCurve.Evaluate(elapsed / animationDuration);
            
            UpdateAnimation(progress);
            
            yield return null;
        }
        
        // Ensure final state
        UpdateAnimation(1f);
    }
    
    private void SetupInitialState()
    {
        switch (animationType)
        {
            case AnimationType.Fade:
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                break;
                
            case AnimationType.Scale:
                if (panelRectTransform != null) panelRectTransform.localScale = Vector3.zero;
                break;
                
            case AnimationType.ScaleAndFade:
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                if (panelRectTransform != null) panelRectTransform.localScale = Vector3.zero;
                break;
                
            case AnimationType.SlideFromTop:
                if (panelRectTransform != null)
                {
                    Vector2 pos = panelRectTransform.anchoredPosition;
                    panelRectTransform.anchoredPosition = new Vector2(pos.x, Screen.height);
                }
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                break;
                
            case AnimationType.SlideFromBottom:
                if (panelRectTransform != null)
                {
                    Vector2 pos = panelRectTransform.anchoredPosition;
                    panelRectTransform.anchoredPosition = new Vector2(pos.x, -Screen.height);
                }
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                break;
        }
    }
    
    private void UpdateAnimation(float progress)
    {
        switch (animationType)
        {
            case AnimationType.Fade:
                if (canvasGroup != null) canvasGroup.alpha = progress;
                break;
                
            case AnimationType.Scale:
                if (panelRectTransform != null)
                {
                    float scale = Mathf.LerpUnclamped(0f, 1.1f, progress);
                    if (progress > 0.8f)
                    {
                        scale = Mathf.Lerp(1.1f, 1f, (progress - 0.8f) / 0.2f);
                    }
                    panelRectTransform.localScale = Vector3.one * scale;
                }
                break;
                
            case AnimationType.ScaleAndFade:
                if (canvasGroup != null) canvasGroup.alpha = progress;
                if (panelRectTransform != null)
                {
                    float scale = Mathf.LerpUnclamped(0f, 1.1f, progress);
                    if (progress > 0.8f)
                    {
                        scale = Mathf.Lerp(1.1f, 1f, (progress - 0.8f) / 0.2f);
                    }
                    panelRectTransform.localScale = Vector3.one * scale;
                }
                break;
                
            case AnimationType.SlideFromTop:
                if (canvasGroup != null) canvasGroup.alpha = progress;
                if (panelRectTransform != null)
                {
                    float yPos = Mathf.Lerp(Screen.height, 0f, progress);
                    Vector2 pos = panelRectTransform.anchoredPosition;
                    panelRectTransform.anchoredPosition = new Vector2(pos.x, yPos);
                }
                break;
                
            case AnimationType.SlideFromBottom:
                if (canvasGroup != null) canvasGroup.alpha = progress;
                if (panelRectTransform != null)
                {
                    float yPos = Mathf.Lerp(-Screen.height, 0f, progress);
                    Vector2 pos = panelRectTransform.anchoredPosition;
                    panelRectTransform.anchoredPosition = new Vector2(pos.x, yPos);
                }
                break;
        }
    }
    
    private void OnRestartButtonClicked()
    {
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.RestartGame();
        }
    }
    
    private void OnMainMenuButtonClicked()
    {
        if (ObstacleManager.Instance != null)
        {
            ObstacleManager.Instance.LoadMainMenu(mainMenuSceneName);
        }
    }
}
