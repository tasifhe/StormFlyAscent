using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// FPS Counter with performance monitoring for mobile optimization
/// Shows FPS, frame time, memory usage, and performance warnings
/// Updated for New Input System
/// 
/// USAGE:
/// 1. Create a Canvas with TextMeshProUGUI component
/// 2. Position it wherever you want in the Canvas
/// 3. Assign it to the "FPS Text" field in Inspector
/// 4. If not assigned, will auto-create in top-right corner
/// 
/// VISIBILITY IN BUILDS:
/// - Automatically visible in builds (alwaysShowInBuilds = true)
/// - Toggle with F1 key (Editor/PC)
/// - Toggle with 3-finger tap (Mobile)
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Header("Display Settings")]
    [Tooltip("Assign your own Canvas TextMeshProUGUI here, or leave empty to auto-create")]
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private bool showFPS = true;
    [SerializeField] private bool alwaysShowInBuilds = true; // Always show FPS in builds
    
    [Header("Auto-Create Settings (only if fpsText is not assigned)")]
    [SerializeField] private bool autoCreateIfMissing = true;
    [SerializeField] private Vector2 autoCreatePosition = new Vector2(-10, -10); // Top-right corner
    [SerializeField] private TextAlignmentOptions autoCreateAlignment = TextAlignmentOptions.TopRight;
    
    [Header("Update Settings")]
    [SerializeField] private float updateInterval = 0.5f; // Update FPS text every 0.5 seconds
    
    [Header("Color Coding")]
    [SerializeField] private Color excellentColor = Color.green;   // 60+ FPS
    [SerializeField] private Color goodColor = Color.yellow;       // 30-60 FPS
    [SerializeField] private Color poorColor = Color.red;          // < 30 FPS
    
    [Header("Advanced Info")]
    [SerializeField] private bool showDetailedInfo = true;
    [SerializeField] private bool showMemoryInfo = true;
    
    // FPS calculation
    private float fps;
    private float deltaTime;
    private float accumulatedTime;
    private int frames;
    
    // Performance tracking
    private float minFPS = float.MaxValue;
    private float maxFPS = 0f;
    private float avgFPS = 0f;
    
    private void Start()
    {
        // Force show FPS in builds if enabled
        #if !UNITY_EDITOR
        if (alwaysShowInBuilds)
        {
            showFPS = true;
        }
        #endif
        
        // Create FPS text if not assigned and auto-create is enabled
        if (fpsText == null && autoCreateIfMissing && showFPS)
        {
            CreateFPSText();
        }
        
        // If fpsText is assigned, just make sure it's active
        if (fpsText != null)
        {
            fpsText.gameObject.SetActive(showFPS);
        }
        
        // Don't destroy on scene load
        DontDestroyOnLoad(gameObject);
        
        if (fpsText != null)
        {
            Debug.Log("[FPSCounter] Initialized - Press F1 to toggle display");
        }
        else
        {
            Debug.LogWarning("[FPSCounter] No TextMeshProUGUI assigned and auto-create is disabled!");
        }
    }
    
    private void Update()
    {
        // Toggle display with F1 key (New Input System)
        if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
        {
            showFPS = !showFPS;
            if (fpsText != null)
                fpsText.gameObject.SetActive(showFPS);
        }
        
        // Toggle with 3-finger tap on mobile (for builds)
        #if !UNITY_EDITOR
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 3)
        {
            bool allBegan = true;
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
                {
                    allBegan = false;
                    break;
                }
            }
            
            if (allBegan)
            {
                showFPS = !showFPS;
                if (fpsText != null)
                    fpsText.gameObject.SetActive(showFPS);
            }
        }
        #endif
        
        if (!showFPS || fpsText == null)
            return;
        
        // Calculate FPS
        deltaTime = Time.unscaledDeltaTime;
        accumulatedTime += deltaTime;
        frames++;
        
        // Update FPS display at intervals
        if (accumulatedTime >= updateInterval)
        {
            fps = frames / accumulatedTime;
            
            // Track min/max/avg
            if (fps < minFPS) minFPS = fps;
            if (fps > maxFPS) maxFPS = fps;
            avgFPS = (avgFPS + fps) / 2f;
            
            UpdateFPSDisplay();
            
            // Reset counters
            accumulatedTime = 0f;
            frames = 0;
        }
    }
    
    private void UpdateFPSDisplay()
    {
        if (fpsText == null) return;
        
        // Build display text
        string displayText = "";
        
        // Main FPS
        displayText += $"<size=24><b>FPS: {Mathf.RoundToInt(fps)}</b></size>\n";
        
        if (showDetailedInfo)
        {
            // Frame time in milliseconds
            float frameTime = deltaTime * 1000f;
            displayText += $"<size=14>Frame: {frameTime:F1}ms</size>\n";
            
            // Min/Max/Avg
            displayText += $"<size=12>Min: {Mathf.RoundToInt(minFPS)} | Max: {Mathf.RoundToInt(maxFPS)} | Avg: {Mathf.RoundToInt(avgFPS)}</size>\n";
        }
        
        if (showMemoryInfo)
        {
            // Memory usage
            float memoryUsedMB = (System.GC.GetTotalMemory(false) / 1048576f);
            displayText += $"<size=12>Memory: {memoryUsedMB:F1} MB</size>\n";
        }
        
        // Color code based on performance
        if (fps >= 60f)
            fpsText.color = excellentColor;
        else if (fps >= 30f)
            fpsText.color = goodColor;
        else
            fpsText.color = poorColor;
        
        fpsText.text = displayText;
    }
    
    private void CreateFPSText()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("FPS_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Always on top
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        DontDestroyOnLoad(canvasObj);
        
        // Create FPS text
        GameObject textObj = new GameObject("FPS_Text");
        textObj.transform.SetParent(canvas.transform, false);
        
        fpsText = textObj.AddComponent<TextMeshProUGUI>();
        fpsText.fontSize = 18;
        fpsText.color = Color.white;
        fpsText.alignment = autoCreateAlignment;
        fpsText.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        
        // Add shadow for better readability
        fpsText.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
        
        // Position based on settings
        RectTransform rectTransform = fpsText.GetComponent<RectTransform>();
        
        // Set anchors based on alignment
        if (autoCreateAlignment == TextAlignmentOptions.TopRight || autoCreateAlignment == TextAlignmentOptions.Right || autoCreateAlignment == TextAlignmentOptions.BottomRight)
        {
            rectTransform.anchorMin = new Vector2(1, 1);
            rectTransform.anchorMax = new Vector2(1, 1);
            rectTransform.pivot = new Vector2(1, 1);
        }
        else if (autoCreateAlignment == TextAlignmentOptions.TopLeft || autoCreateAlignment == TextAlignmentOptions.Left || autoCreateAlignment == TextAlignmentOptions.BottomLeft)
        {
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
        }
        else // Center
        {
            rectTransform.anchorMin = new Vector2(0.5f, 1);
            rectTransform.anchorMax = new Vector2(0.5f, 1);
            rectTransform.pivot = new Vector2(0.5f, 1);
        }
        
        rectTransform.anchoredPosition = autoCreatePosition;
        rectTransform.sizeDelta = new Vector2(250, 150);
        
        Debug.Log($"[FPSCounter] FPS display auto-created at position {autoCreatePosition}");
    }
    
    /// <summary>
    /// Public method to toggle FPS display from other scripts
    /// </summary>
    public void ToggleDisplay()
    {
        showFPS = !showFPS;
        if (fpsText != null)
            fpsText.gameObject.SetActive(showFPS);
    }
    
    /// <summary>
    /// Reset min/max/avg statistics
    /// </summary>
    public void ResetStats()
    {
        minFPS = float.MaxValue;
        maxFPS = 0f;
        avgFPS = 0f;
    }
}
