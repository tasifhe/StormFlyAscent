using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Mobile Touch Diagnostic & Auto-Fix Tool
/// Add this to your Main Menu scene, enter Play mode, it will diagnose and fix issues
/// Then remove it from the scene
/// </summary>
public class MobileTouchDiagnostic : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    [SerializeField] private bool autoFixIssues = true;
    [SerializeField] private bool verboseLogging = true;

    private int issuesFound = 0;
    private int issuesFixed = 0;

    private void Start()
    {
        Debug.Log("=== MOBILE TOUCH DIAGNOSTIC START ===");

        // Run all diagnostics
        CheckEventSystem();
        CheckCanvasComponents();
        CheckButtons();
        CheckPanels();

        // Summary
        Debug.Log($"\n=== DIAGNOSTIC COMPLETE ===");
        Debug.Log($"Issues Found: {issuesFound}");
        Debug.Log($"Issues Fixed: {issuesFixed}");

        if (issuesFound == 0)
        {
            Debug.Log("✅ No issues found! Mobile touch should work perfectly!");
        }
        else if (issuesFixed == issuesFound)
        {
            Debug.Log($"✅ All {issuesFixed} issues have been FIXED!");
        }
        else
        {
            Debug.LogWarning($"⚠️ {issuesFound - issuesFixed} issues still need manual fixing");
        }

        Debug.Log("=========================\n");
    }

    private void CheckEventSystem()
    {
        Log("\n--- Checking EventSystem ---");

        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            LogIssue("❌ NO EVENTSYSTEM FOUND!");

            if (autoFixIssues)
            {
                GameObject obj = new GameObject("EventSystem");
                obj.AddComponent<EventSystem>();
                obj.AddComponent<InputSystemUIInputModule>();
                LogFix("✅ Created EventSystem with InputSystemUIInputModule");
            }
            return;
        }

        Log($"✅ EventSystem found: {eventSystem.gameObject.name}");

        // Check input module
        var inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        var oldModule = eventSystem.GetComponent<StandaloneInputModule>();

        if (inputModule != null)
        {
            Log("✅ Has InputSystemUIInputModule (correct for New Input System)");
        }
        else if (oldModule != null)
        {
            LogIssue("❌ Using OLD StandaloneInputModule (won't work with New Input System)");

            if (autoFixIssues)
            {
                Destroy(oldModule);
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                LogFix("✅ Replaced with InputSystemUIInputModule");
            }
        }
        else
        {
            LogIssue("❌ NO INPUT MODULE on EventSystem!");

            if (autoFixIssues)
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                LogFix("✅ Added InputSystemUIInputModule");
            }
        }
    }

    private void CheckCanvasComponents()
    {
        Log("\n--- Checking Canvas Components ---");

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        if (canvases.Length == 0)
        {
            LogIssue("❌ NO CANVAS FOUND IN SCENE!");
            return;
        }

        foreach (Canvas canvas in canvases)
        {
            Log($"\nCanvas: {canvas.gameObject.name}");

            // Check GraphicRaycaster
            GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                LogIssue("  ❌ Missing GraphicRaycaster (CRITICAL for touch input!)");

                if (autoFixIssues)
                {
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                    LogFix("  ✅ Added GraphicRaycaster");
                }
            }
            else
            {
                Log("  ✅ Has GraphicRaycaster");
            }

            // Check CanvasScaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                Log("  ⚠️ No CanvasScaler (recommended for mobile)");
            }
            else
            {
                Log($"  ✅ Has CanvasScaler (mode: {scaler.uiScaleMode})");
            }

            // Check render mode
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                Log($"  ✅ Render Mode: Screen Space - Overlay (sort order: {canvas.sortingOrder})");
            }
            else
            {
                Log($"  ℹ️ Render Mode: {canvas.renderMode}");
            }
        }
    }

    private void CheckButtons()
    {
        Log("\n--- Checking Buttons ---");

        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (buttons.Length == 0)
        {
            LogIssue("❌ NO BUTTONS FOUND!");
            return;
        }

        Log($"Found {buttons.Length} buttons");

        int navigationIssues = 0;

        foreach (Button button in buttons)
        {
            // Check navigation mode
            if (button.navigation.mode != Navigation.Mode.None &&
                button.navigation.mode != Navigation.Mode.Explicit)
            {
                navigationIssues++;

                if (verboseLogging)
                {
                    Log($"  ⚠️ Button '{button.gameObject.name}' has Navigation: {button.navigation.mode}");
                }
            }
        }

        if (navigationIssues > 0)
        {
            LogIssue($"❌ {navigationIssues} buttons have Automatic/Horizontal/Vertical navigation");
            Log("   This can cause touch issues on mobile!");

            if (autoFixIssues)
            {
                foreach (Button button in buttons)
                {
                    if (button.navigation.mode != Navigation.Mode.None)
                    {
                        Navigation nav = button.navigation;
                        nav.mode = Navigation.Mode.None;
                        button.navigation = nav;
                    }
                }
                LogFix($"✅ Set all {navigationIssues} buttons to Navigation: None");
            }
        }
        else
        {
            Log("✅ All buttons have proper navigation settings");
        }
    }

    private void CheckPanels()
    {
        Log("\n--- Checking UI Panels ---");

        // Find main menu manager
        MainMenuManager menuManager = FindFirstObjectByType<MainMenuManager>();

        if (menuManager == null)
        {
            Log("⚠️ MainMenuManager not found, skipping panel checks");
            return;
        }

        // Check panel structure through reflection
        var mainPanel = GetFieldValue<GameObject>(menuManager, "mainMenuPanel");
        var settingsPanel = GetFieldValue<GameObject>(menuManager, "settingsPanel");
        var customizationPanel = GetFieldValue<GameObject>(menuManager, "customizationPanel");

        CheckIndividualPanel("Main Menu Panel", mainPanel);
        CheckIndividualPanel("Settings Panel", settingsPanel);
        CheckIndividualPanel("Customization Panel", customizationPanel);
    }

    private void CheckIndividualPanel(string name, GameObject panel)
    {
        if (panel == null)
        {
            Log($"  ⚠️ {name}: Not assigned in inspector");
            return;
        }

        Log($"\n  Panel: {name} ({panel.name})");
        Log($"    Active: {panel.activeSelf}");

        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            Log($"    CanvasGroup: alpha={canvasGroup.alpha:F2}, interactable={canvasGroup.interactable}, blocksRaycasts={canvasGroup.blocksRaycasts}");

            if (!canvasGroup.blocksRaycasts && panel.activeSelf)
            {
                LogIssue($"    ❌ Panel is active but NOT blocking raycasts!");
            }
        }
        else
        {
            Log($"    ℹ️ No CanvasGroup (added dynamically by MainMenuManager)");
        }
    }

    private T GetFieldValue<T>(object obj, string fieldName)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (T)field.GetValue(obj);
        }

        return default(T);
    }

    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[MobileTouchDiagnostic] {message}");
        }
    }

    private void LogIssue(string message)
    {
        issuesFound++;
        Debug.LogWarning($"[MobileTouchDiagnostic] {message}");
    }

    private void LogFix(string message)
    {
        issuesFixed++;
        Debug.Log($"[MobileTouchDiagnostic] {message}");
    }
}
