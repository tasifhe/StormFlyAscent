using UnityEngine;

/// <summary>
/// Game Initialization Manager - Sets up all performance and optimization systems
/// Attach this to a GameObject in your first scene (Main Menu)
/// This script should run before anything else
/// </summary>
[DefaultExecutionOrder(-1000)] // Run first
public class GameInitializer : MonoBehaviour
{
    [Header("Performance Systems")]
    [SerializeField] private bool enableFPSCounter = true;
    [SerializeField] private bool enableOptimizations = true;
    [SerializeField] private bool enableObjectPooling = true;
    
    [Header("FPS Settings")]
    [SerializeField] private int targetFPS = -1; // -1 = unlimited, 60 = locked 60, 30 = locked 30
    [SerializeField] private bool disableVSync = true;
    
    [Header("Mobile Settings")]
    [SerializeField] private bool autoDetectMobile = true;
    [SerializeField] private bool forceOptimizeForMobile = false;
    
    [Header("Debug")]
    [SerializeField] private bool showInitLogs = true;
    
    private static bool hasInitialized = false;
    
    private void Awake()
    {
        // Only initialize once
        if (hasInitialized)
        {
            Destroy(gameObject);
            return;
        }
        
        hasInitialized = true;
        DontDestroyOnLoad(gameObject);
        
        if (showInitLogs)
        {
            Debug.Log("[GameInitializer] Starting game initialization...");
        }
        
        InitializePerformanceSystems();
    }
    
    private void InitializePerformanceSystems()
    {
        // 1. Setup FPS and Frame Rate
        SetupFrameRate();
        
        // 2. Create FPS Counter
        if (enableFPSCounter)
        {
            CreateFPSCounter();
        }
        
        // 3. Create Mobile Optimization Manager
        if (enableOptimizations)
        {
            CreateOptimizationManager();
        }
        
        // 4. Create Object Pool
        if (enableObjectPooling)
        {
            CreateObjectPool();
        }
        
        // 5. Detect and optimize for mobile
        if (autoDetectMobile || forceOptimizeForMobile)
        {
            OptimizeForMobile();
        }
        
        if (showInitLogs)
        {
            Debug.Log("[GameInitializer] ✅ Initialization complete!");
            LogSystemInfo();
        }
    }
    
    private void SetupFrameRate()
    {
        // Disable VSync
        if (disableVSync)
        {
            QualitySettings.vSyncCount = 0;
        }
        
        // Set target frame rate
        Application.targetFrameRate = targetFPS;
        
        if (showInitLogs)
        {
            string fpsText = targetFPS == -1 ? "UNLOCKED" : $"{targetFPS} FPS";
            Debug.Log($"[GameInitializer] Frame Rate: {fpsText} | VSync: {(disableVSync ? "OFF" : "ON")}");
        }
    }
    
    private void CreateFPSCounter()
    {
        GameObject fpsObject = new GameObject("FPSCounter");
        fpsObject.transform.SetParent(transform);
        fpsObject.AddComponent<FPSCounter>();
        
        if (showInitLogs)
        {
            Debug.Log("[GameInitializer] ✅ FPS Counter created (Press F1 to toggle)");
        }
    }
    
    private void CreateOptimizationManager()
    {
        GameObject optObject = new GameObject("OptimizationManager");
        optObject.transform.SetParent(transform);
        MobileOptimizationManager optManager = optObject.AddComponent<MobileOptimizationManager>();
        
        // Configure settings
        optManager.GetType().GetField("unlockFrameRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(optManager, true);
        optManager.GetType().GetField("targetFrameRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(optManager, targetFPS);
        optManager.GetType().GetField("disableVSync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(optManager, disableVSync);
        
        if (showInitLogs)
        {
            Debug.Log("[GameInitializer] ✅ Optimization Manager created");
        }
    }
    
    private void CreateObjectPool()
    {
        GameObject poolObject = new GameObject("ObjectPool");
        poolObject.transform.SetParent(transform);
        poolObject.AddComponent<ObjectPool>();
        
        if (showInitLogs)
        {
            Debug.Log("[GameInitializer] ✅ Object Pool created");
        }
    }
    
    private void OptimizeForMobile()
    {
        bool isMobile = Application.isMobilePlatform || forceOptimizeForMobile;
        
        if (isMobile)
        {
            // Mobile-specific optimizations
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // Reduce shadow distance
            QualitySettings.shadowDistance = 30f;
            
            // Optimize physics
            Physics.defaultSolverIterations = 6;
            Physics.defaultSolverVelocityIterations = 1;
            
            // Reduce pixel lights
            QualitySettings.pixelLightCount = 1;
            
            if (showInitLogs)
            {
                Debug.Log("[GameInitializer] ✅ Mobile optimizations applied");
            }
        }
    }
    
    private void LogSystemInfo()
    {
        Debug.Log("=== GAME INITIALIZED ===");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Device: {SystemInfo.deviceModel}");
        Debug.Log($"GPU: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"Memory: {SystemInfo.systemMemorySize} MB");
        Debug.Log($"Target FPS: {(targetFPS == -1 ? "Unlimited" : targetFPS.ToString())}");
        Debug.Log($"Quality: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log("=======================");
    }
}
