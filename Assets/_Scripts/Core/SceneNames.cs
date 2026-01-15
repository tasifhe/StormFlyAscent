using UnityEngine;

/// <summary>
/// Type-safe scene name constants
/// Centralizes all scene names to prevent typos and runtime errors
/// </summary>
public static class SceneNames
{
    // Scene name constants
    public const string MAIN_MENU = "Main Menu";
    public const string LEVEL_1 = "Level 1";

    /// <summary>
    /// Validate if a scene name is recognized
    /// </summary>
    public static bool IsValid(string sceneName)
    {
        return sceneName == MAIN_MENU || sceneName == LEVEL_1;
    }

    /// <summary>
    /// Get all valid scene names
    /// </summary>
    public static string[] GetAllScenes()
    {
        return new string[] { MAIN_MENU, LEVEL_1 };
    }
}
