using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Editor utility to help set up the professional menu system
/// </summary>
public class MenuSystemSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool createLoadingCanvas = true;
    [SerializeField] private bool createManagers = true;
    [SerializeField] private bool setupMainMenu = false;
    
    [Header("Style Settings")]
    [SerializeField] private Color loadingBackgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    [SerializeField] private Color progressBarColor = new Color(0.2f, 0.8f, 0.3f, 1f);
    [SerializeField] private Font customFont;
    
    [ContextMenu("Setup Menu System")]
    public void SetupMenuSystem()
    {
        if (createLoadingCanvas)
        {
            CreateLoadingCanvas();
        }
        
        if (createManagers)
        {
            CreateManagers();
        }
        
        if (setupMainMenu)
        {
            SetupMainMenuReferences();
        }
        
        Debug.Log("[MenuSystemSetup] Setup complete! Check the hierarchy for new objects.");
    }
    
    private void CreateLoadingCanvas()
    {
        // Check if already exists
        GameObject existing = GameObject.Find("LoadingCanvas");
        if (existing != null)
        {
            Debug.LogWarning("[MenuSystemSetup] LoadingCanvas already exists!");
            return;
        }
        
        // Create canvas
        GameObject canvasObj = new GameObject("LoadingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Add LoadingScreenManager
        LoadingScreenManager loadingManager = canvasObj.AddComponent<LoadingScreenManager>();
        
        // Create background panel
        GameObject panelObj = new GameObject("LoadingPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = loadingBackgroundColor;
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        
        CanvasGroup panelGroup = panelObj.AddComponent<CanvasGroup>();
        
        // Create progress bar
        GameObject progressBarObj = new GameObject("ProgressBar");
        progressBarObj.transform.SetParent(panelObj.transform, false);
        
        Slider slider = progressBarObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
        
        RectTransform sliderRect = progressBarObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.2f, 0.25f);
        sliderRect.anchorMax = new Vector2(0.8f, 0.3f);
        sliderRect.sizeDelta = Vector2.zero;
        
        // Progress bar background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(progressBarObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        
        // Progress bar fill
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(progressBarObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;
        
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = progressBarColor;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;
        
        // Create progress text
        GameObject progressTextObj = new GameObject("ProgressText");
        progressTextObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
        progressText.text = "0%";
        progressText.fontSize = 48;
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.color = Color.white;
        
        RectTransform progressTextRect = progressTextObj.GetComponent<RectTransform>();
        progressTextRect.anchorMin = new Vector2(0.5f, 0.3f);
        progressTextRect.anchorMax = new Vector2(0.5f, 0.35f);
        progressTextRect.sizeDelta = new Vector2(300, 100);
        
        // Create status text
        GameObject statusTextObj = new GameObject("StatusText");
        statusTextObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI statusText = statusTextObj.AddComponent<TextMeshProUGUI>();
        statusText.text = "Loading...";
        statusText.fontSize = 36;
        statusText.alignment = TextAlignmentOptions.Center;
        statusText.color = Color.white;
        
        RectTransform statusTextRect = statusTextObj.GetComponent<RectTransform>();
        statusTextRect.anchorMin = new Vector2(0.5f, 0.4f);
        statusTextRect.anchorMax = new Vector2(0.5f, 0.45f);
        statusTextRect.sizeDelta = new Vector2(600, 100);
        
        // Create tip text
        GameObject tipTextObj = new GameObject("TipText");
        tipTextObj.transform.SetParent(panelObj.transform, false);
        TextMeshProUGUI tipText = tipTextObj.AddComponent<TextMeshProUGUI>();
        tipText.text = "Loading tip...";
        tipText.fontSize = 24;
        tipText.alignment = TextAlignmentOptions.Center;
        tipText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        RectTransform tipTextRect = tipTextObj.GetComponent<RectTransform>();
        tipTextRect.anchorMin = new Vector2(0.1f, 0.05f);
        tipTextRect.anchorMax = new Vector2(0.9f, 0.15f);
        tipTextRect.sizeDelta = Vector2.zero;
        
        // Create spinner
        GameObject spinnerObj = new GameObject("Spinner");
        spinnerObj.transform.SetParent(panelObj.transform, false);
        Image spinnerImage = spinnerObj.AddComponent<Image>();
        spinnerImage.color = Color.white;
        
        RectTransform spinnerRect = spinnerObj.GetComponent<RectTransform>();
        spinnerRect.anchorMin = new Vector2(0.5f, 0.6f);
        spinnerRect.anchorMax = new Vector2(0.5f, 0.6f);
        spinnerRect.sizeDelta = new Vector2(100, 100);
        
        // Assign references to manager
        SerializedObject serializedManager = new SerializedObject(loadingManager);
        serializedManager.FindProperty("loadingCanvasGroup").objectReferenceValue = panelGroup;
        serializedManager.FindProperty("progressBar").objectReferenceValue = slider;
        serializedManager.FindProperty("progressText").objectReferenceValue = progressText;
        serializedManager.FindProperty("statusText").objectReferenceValue = statusText;
        serializedManager.FindProperty("tipText").objectReferenceValue = tipText;
        serializedManager.FindProperty("spinnerTransform").objectReferenceValue = spinnerRect;
        serializedManager.FindProperty("progressBarFill").objectReferenceValue = fillImage;
        serializedManager.ApplyModifiedProperties();
        
        EditorUtility.SetDirty(loadingManager);
        
        Debug.Log("[MenuSystemSetup] Loading canvas created successfully!");
    }
    
    private void CreateManagers()
    {
        // Create MenuSystemManagers object
        GameObject managersObj = GameObject.Find("MenuSystemManagers");
        if (managersObj == null)
        {
            managersObj = new GameObject("MenuSystemManagers");
        }
        
        // Add MenuSceneManager
        if (managersObj.GetComponent<MenuSceneManager>() == null)
        {
            managersObj.AddComponent<MenuSceneManager>();
            Debug.Log("[MenuSystemSetup] Added MenuSceneManager");
        }
        
        // Add MenuTransitionManager
        if (managersObj.GetComponent<MenuTransitionManager>() == null)
        {
            managersObj.AddComponent<MenuTransitionManager>();
            Debug.Log("[MenuSystemSetup] Added MenuTransitionManager");
        }
        
        // Add LoadingProgressHandler
        if (managersObj.GetComponent<LoadingProgressHandler>() == null)
        {
            managersObj.AddComponent<LoadingProgressHandler>();
            Debug.Log("[MenuSystemSetup] Added LoadingProgressHandler");
        }
        
        Debug.Log("[MenuSystemSetup] Manager components added!");
    }
    
    private void SetupMainMenuReferences()
    {
        MainMenuManager mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu == null)
        {
            Debug.LogWarning("[MenuSystemSetup] MainMenuManager not found in scene!");
            return;
        }
        
        Debug.Log("[MenuSystemSetup] Found MainMenuManager, ready for manual configuration");
    }
}

/// <summary>
/// Custom editor for MenuSystemSetupHelper
/// </summary>
[CustomEditor(typeof(MenuSystemSetupHelper))]
public class MenuSystemSetupHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Setup Menu System", GUILayout.Height(40)))
        {
            MenuSystemSetupHelper helper = (MenuSystemSetupHelper)target;
            helper.SetupMenuSystem();
        }
        
        EditorGUILayout.HelpBox(
            "This helper will create:\n" +
            "- LoadingCanvas with all UI elements\n" +
            "- Manager components for scene loading\n" +
            "- Proper references and settings\n\n" +
            "Make sure to have DOTween and TextMeshPro installed!",
            MessageType.Info
        );
    }
}
#endif
