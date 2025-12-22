using UnityEngine;

/// <summary>
/// Diagnoses boost and animation issues
/// Attach to your bird character
/// </summary>
public class BoostDiagnostic : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("========== BOOST DIAGNOSTIC ==========");
        
        // Check Character
        Character character = GetComponent<Character>();
        if (character == null)
        {
            Debug.LogError("❌ Character component NOT FOUND!");
            return;
        }
        Debug.Log("✓ Character component found");
        
        // Check AnimationManager
        if (character.animationManager == null)
        {
            Debug.LogError("❌ AnimationManager is NULL on Character!");
        }
        else
        {
            Debug.Log("✓ AnimationManager assigned to Character");
            
            BirdAnimationManager animMgr = character.animationManager;
            
            // Check Animator
            Animator animator = animMgr.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("❌ Animator component NOT FOUND on AnimationManager!");
            }
            else
            {
                Debug.Log("✓ Animator component found");
                Debug.Log($"   Animator enabled: {animator.enabled}");
                Debug.Log($"   Animator active: {animator.isActiveAndEnabled}");
                
                // Check for Flap parameter
                bool hasFlap = false;
                bool hasFlying = false;
                bool hasGliding = false;
                
                Debug.Log("\n--- Animator Parameters ---");
                foreach (var param in animator.parameters)
                {
                    Debug.Log($"   {param.name} ({param.type})");
                    if (param.name == "Flap") hasFlap = true;
                    if (param.name == "Flying") hasFlying = true;
                    if (param.name == "Gliding") hasGliding = true;
                }
                
                if (!hasFlap)
                {
                    Debug.LogError("❌ 'Flap' TRIGGER parameter missing! Add it to your Animator.");
                }
                else
                {
                    Debug.Log("✓ 'Flap' trigger found");
                }
                
                if (!hasFlying) Debug.LogWarning("⚠️ 'Flying' bool parameter missing");
                if (!hasGliding) Debug.LogWarning("⚠️ 'Gliding' bool parameter missing");
            }
        }
        
        // Check Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("❌ Rigidbody NOT FOUND!");
        }
        else
        {
            Debug.Log("✓ Rigidbody found");
            Debug.Log($"   Mass: {rb.mass}");
            Debug.Log($"   Drag: {rb.linearDamping}");
            Debug.Log($"   Constraints: {rb.constraints}");
        }
        
        // Check BirdPathFollower
        BirdPathFollower pathFollower = GetComponent<BirdPathFollower>();
        if (pathFollower == null)
        {
            Debug.LogWarning("⚠️ BirdPathFollower NOT FOUND!");
        }
        else
        {
            Debug.Log("✓ BirdPathFollower found");
            Debug.Log($"   Follow speed: {pathFollower.followSpeed}");
        }
        
        // Check BoostSystem (new modular component)
        BoostSystem boostSystem = GetComponent<BoostSystem>();
        if (boostSystem == null)
        {
            Debug.LogWarning("⚠️ BoostSystem NOT FOUND! (new modular system)");
        }
        else
        {
            Debug.Log("✓ BoostSystem found (modular)");
        }
        
        // Check BoostInputDetector
        BoostInputDetector inputDetector = GetComponent<BoostInputDetector>();
        if (inputDetector == null)
        {
            Debug.LogWarning("⚠️ BoostInputDetector NOT FOUND!");
        }
        else
        {
            Debug.Log("✓ BoostInputDetector found");
        }
        
        // Check for Boost Button in UI
        ButtonBoostUI buttonUI = FindFirstObjectByType<ButtonBoostUI>();
        if (buttonUI == null)
        {
            Debug.Log("ℹ️ ButtonBoostUI not found (optional - for UI button visual feedback)");
        }
        else
        {
            Debug.Log("✓ ButtonBoostUI found");
            if (buttonUI.boostButton != null)
            {
                Debug.Log("   ✓ Boost button assigned");
            }
            else
            {
                Debug.LogWarning("   ⚠️ Boost button not assigned in ButtonBoostUI!");
            }
        }
        
        Debug.Log("\n========== DIAGNOSTIC COMPLETE ==========");
        Debug.Log("If Flap trigger is missing, add it in the Animator window:");
        Debug.Log("1. Select your Animator Controller");
        Debug.Log("2. Click Parameters tab");
        Debug.Log("3. Click + → Trigger");
        Debug.Log("4. Name it: 'Flap'");
        Debug.Log("5. Add transitions to Flapping state");
        Debug.Log("==========================================\n");
    }
}
