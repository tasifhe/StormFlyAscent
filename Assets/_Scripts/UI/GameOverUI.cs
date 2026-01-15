using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

/// <summary>
/// Professional Game Over UI with polished DOTween animations
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Configuration")]
    [SerializeField] private float entryDelay = 0.5f;
    [SerializeField] private float animationDuration = 0.6f;
    [SerializeField] private Ease entryEase = Ease.OutBack;
    [SerializeField] private string mainMenuSceneName = "Main Menu";
    
    [Header("Audio")]
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip scoreTickSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    private int targetScore = 0;
    
    private void Awake()
    {
        // Setup Audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Ensure components
        if (gameOverPanel == null) gameOverPanel = gameObject;
        if (canvasGroup == null) canvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
        if (panelRectTransform == null) panelRectTransform = gameOverPanel.GetComponent<RectTransform>();

        // Disable initially
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    private void Start()
    {
        // Listeners
        if (restartButton != null) restartButton.onClick.AddListener(() => OnButtonClick(restartButton, OnRestart));
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(() => OnButtonClick(mainMenuButton, OnMainMenu));
        
        // Subscribe
        if (ObstacleManager.Instance != null)
            ObstacleManager.Instance.OnGameOver += ShowGameOver;
    }
    
    private void OnDestroy()
    {
        // Cleanup
        if (ObstacleManager.Instance != null)
            ObstacleManager.Instance.OnGameOver -= ShowGameOver;
            
        DOTween.Kill(this);
        DOTween.Kill(panelRectTransform);
        DOTween.Kill(canvasGroup);
    }
    
    public void ShowGameOver()
    {
        StartCoroutine(ShowSequence());
    }
    
    private IEnumerator ShowSequence()
    {
        yield return new WaitForSecondsRealtime(entryDelay);
        
        // 1. Activate Panel
        gameOverPanel.SetActive(true);
        canvasGroup.blocksRaycasts = true;
        
        // 2. Play Sound
        if (gameOverSound) audioSource.PlayOneShot(gameOverSound);
        
        // 3. Reset State for Animation
        canvasGroup.alpha = 0f;
        panelRectTransform.localScale = Vector3.one * 0.5f; // Start small
        
        // 4. Create Sequence
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); // Ignore Time.timeScale = 0
        
        // Fade In
        seq.Join(canvasGroup.DOFade(1f, animationDuration * 0.8f).SetEase(Ease.OutQuad));
        
        // Scale Up (Bounce)
        seq.Join(panelRectTransform.DOScale(1f, animationDuration).SetEase(entryEase));
        
        // 5. Update Score Text
        if (ObstacleManager.Instance != null)
        {
            var stats = ObstacleManager.Instance.GetStats();
            targetScore = stats.score;
            
            // Check High Score
            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (targetScore > highScore)
            {
                highScore = targetScore;
                PlayerPrefs.SetInt("HighScore", highScore);
                PlayerPrefs.Save();
                if (highScoreText) highScoreText.text = "NEW HIGH SCORE!";
                
                // Pulse High Score Text
                if (highScoreText)
                {
                    seq.AppendCallback(() => {
                        highScoreText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetUpdate(true);
                    });
                }
            }
            else
            {
                if (highScoreText) highScoreText.text = $"High Score: {highScore}";
            }
            
            // Animate Score Counting
            if (finalScoreText != null)
            {
                // Start at 0
                finalScoreText.text = "Final Score: 0";
                
                // Count up effect
                seq.Append(DOTween.To(() => 0, x => {
                    finalScoreText.text = $"Final Score: {x}";
                }, targetScore, 1f).SetEase(Ease.OutExpo).SetUpdate(true));
            }
            
            if (gameOverText != null)
            {
                 // Keep "GAME OVER" or set it explicitly if it was changed in editor
                 gameOverText.text = "GAME OVER";
                 
                 // If we want to show stats, maybe append them to score or use a different text?
                 // For now, let's append stats to the final score text after the count up
                 seq.AppendCallback(() => {
                     if (finalScoreText != null)
                        finalScoreText.text = $"Final Score: {targetScore}\nObstacles: {stats.success}/{stats.total}";
                 });
            }
        }
        
        // 6. Animate Buttons In (Staggered)
        if (restartButton)
        {
            restartButton.transform.localScale = Vector3.zero;
            seq.Append(restartButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true));
        }
        
        if (mainMenuButton)
        {
            mainMenuButton.transform.localScale = Vector3.zero;
            seq.Join(mainMenuButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f).SetUpdate(true));
        }
    }
    
    /// <summary>
    /// Handles button click effects and actions
    /// </summary>
    private void OnButtonClick(Button btn, System.Action action)
    {
        if (buttonClickSound) audioSource.PlayOneShot(buttonClickSound);
        
        // Punch Effect
        btn.transform.DOKill();
        btn.transform.localScale = Vector3.one;
        btn.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.2f, 10, 1)
            .SetUpdate(true)
            .OnComplete(() => action?.Invoke());
    }
    
    private void OnRestart()
    {
        if (ObstacleManager.Instance != null)
            ObstacleManager.Instance.RestartGame();
    }
    
    private void OnMainMenu()
    {
        if (ObstacleManager.Instance != null)
            ObstacleManager.Instance.LoadMainMenu(mainMenuSceneName);
    }
}
