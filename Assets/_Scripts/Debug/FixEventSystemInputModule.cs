using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

/// <summary>
/// Quick fix: Manually replace StandaloneInputModule with InputSystemUIInputModule
/// Add this to your scene, run once, then remove it
/// </summary>
public class FixEventSystemInputModule : MonoBehaviour
{
    private void Start()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        
        if (eventSystem == null)
        {
            Debug.LogError("[FixEventSystemInputModule] No EventSystem found!");
            return;
        }
        
        // Check what module it has
        var oldModule = eventSystem.GetComponent<StandaloneInputModule>();
        var newModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        
        if (oldModule != null && newModule == null)
        {
            Debug.LogWarning("[FixEventSystemInputModule] Found OLD StandaloneInputModule! Replacing with InputSystemUIInputModule...");
            DestroyImmediate(oldModule);
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[FixEventSystemInputModule] ✅ FIXED! Touch input should now work!");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(eventSystem.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            #endif
        }
        else if (newModule != null)
        {
            Debug.Log("[FixEventSystemInputModule] ✅ Already has InputSystemUIInputModule! Touch should work!");
        }
        else if (oldModule == null && newModule == null)
        {
            Debug.LogWarning("[FixEventSystemInputModule] EventSystem has NO input module! Adding InputSystemUIInputModule...");
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            Debug.Log("[FixEventSystemInputModule] ✅ InputSystemUIInputModule added!");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(eventSystem.gameObject);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
            #endif
        }
        
        // Destroy this script after running once
        Destroy(this);
    }
}
