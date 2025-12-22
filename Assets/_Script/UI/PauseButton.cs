using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Simple pause button that triggers the pause menu
/// </summary>
[RequireComponent(typeof(Button))]
public class PauseButton : MonoBehaviour
{
    private Button button;
    private PauseMenuManager pauseMenuManager;
    
    [Header("Animation")]
    [SerializeField] private float scaleAmount = 1.1f;
    [SerializeField] private float scaleDuration = 0.2f;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnPauseButtonClicked);
        
        // Find pause menu manager
        pauseMenuManager = FindFirstObjectByType<PauseMenuManager>();
    }
    
    private void OnPauseButtonClicked()
    {
        // Animate button
        transform.DOScale(scaleAmount, scaleDuration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(1f, scaleDuration * 0.5f).SetEase(Ease.InQuad);
            });
        
        // Trigger pause
        if (pauseMenuManager != null)
        {
            pauseMenuManager.PauseGame();
        }
        else
        {
            Debug.LogError("PauseMenuManager not found!");
        }
    }
}
