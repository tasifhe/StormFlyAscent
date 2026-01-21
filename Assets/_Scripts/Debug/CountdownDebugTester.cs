using UnityEngine;

/// <summary>
/// DEBUG SCRIPT - Add this to test countdown manually
/// Attach to any GameObject and press Space in Play mode
/// </summary>
public class CountdownDebugTester : MonoBehaviour
{
    private void Update()
    {
        // Press Space to trigger countdown
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("[DEBUG] Space pressed - searching for CountdownController...");

            CountdownController countdown = FindFirstObjectByType<CountdownController>();

            if (countdown != null)
            {
                Debug.Log("[DEBUG] ✓ CountdownController found! Starting countdown...");
                countdown.StartCountdown();
            }
            else
            {
                Debug.LogError("[DEBUG] ✗ CountdownController NOT FOUND in scene!");
            }
        }
    }
}
