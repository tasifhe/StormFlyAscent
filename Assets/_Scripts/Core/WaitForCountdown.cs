using UnityEngine;
using System.Collections;

/// <summary>
/// Simple script to disable the entire player GameObject until countdown finishes
/// Attach this to your Player/Character GameObject
/// </summary>
public class WaitForCountdown : MonoBehaviour
{
    [Header("Components to Disable")]
    [Tooltip("If true, disables all MonoBehaviours on start")]
    [SerializeField] private bool disableAllComponents = true;

    [Tooltip("Or manually list specific components to disable")]
    [SerializeField] private MonoBehaviour[] specificComponents;

    private MonoBehaviour[] componentsToDisable;

    private void Awake()
    {
        // Find all components to disable
        if (disableAllComponents)
        {
            // Get all MonoBehaviours except this one
            var allComponents = GetComponents<MonoBehaviour>();
            var tempList = new System.Collections.Generic.List<MonoBehaviour>();

            foreach (var comp in allComponents)
            {
                if (comp != this && comp != null)
                {
                    tempList.Add(comp);
                }
            }

            componentsToDisable = tempList.ToArray();
        }
        else
        {
            componentsToDisable = specificComponents;
        }

        // Disable all components immediately
        DisableComponents();
        Debug.Log($"[WaitForCountdown] Disabled {componentsToDisable.Length} components on {gameObject.name}");
    }

    private void Start()
    {
        // Find countdown controller and subscribe
        CountdownController countdown = FindFirstObjectByType<CountdownController>();

        if (countdown != null)
        {
            countdown.OnCountdownComplete += EnableComponents;
            Debug.Log("[WaitForCountdown] Subscribed to countdown complete event");
        }
        else
        {
            Debug.LogWarning("[WaitForCountdown] No CountdownController found - enabling components immediately");
            EnableComponents();
        }
    }

    private void DisableComponents()
    {
        if (componentsToDisable == null) return;

        foreach (var comp in componentsToDisable)
        {
            if (comp != null)
            {
                comp.enabled = false;
            }
        }
    }

    private void EnableComponents()
    {
        if (componentsToDisable == null) return;

        foreach (var comp in componentsToDisable)
        {
            if (comp != null)
            {
                comp.enabled = true;
            }
        }

        Debug.Log($"[WaitForCountdown] ✅ Enabled {componentsToDisable.Length} components - ready to play!");
    }

    private void OnDestroy()
    {
        // Clean up subscription
        CountdownController countdown = FindFirstObjectByType<CountdownController>();
        if (countdown != null)
        {
            countdown.OnCountdownComplete -= EnableComponents;
        }
    }
}
