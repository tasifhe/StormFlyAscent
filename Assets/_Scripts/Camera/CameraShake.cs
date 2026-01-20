using UnityEngine;
using System.Collections;

/// <summary>
/// Camera shake effect for impact feedback - Works with standard Unity Camera
/// No Cinemachine required!
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Shake Settings")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [SerializeField] private float shakeFrequency = 25f;
    [SerializeField] private float shakeDuration = 0.3f;

    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;

    private Vector3 originalPosition;
    private bool isShaking = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
            else
            {
                Debug.LogWarning("[CameraShake] No camera found! Assign manually.");
            }
        }
    }

    private void LateUpdate()
    {
        // Store original position when not shaking
        if (!isShaking && cameraTransform != null)
        {
            originalPosition = cameraTransform.localPosition;
        }
    }

    /// <summary>
    /// Shake the camera with given intensity and duration
    /// </summary>
    public void Shake(float intensity = 1f, float duration = 0.3f)
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("[CameraShake] No camera transform assigned!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(intensity, duration));
    }

    private IEnumerator ShakeCoroutine(float intensity, float duration)
    {
        isShaking = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Random offset based on intensity
            float x = Random.Range(-1f, 1f) * shakeIntensity * intensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity * intensity;
            float z = Random.Range(-1f, 1f) * shakeIntensity * intensity * 0.5f; // Less Z movement

            cameraTransform.localPosition = originalPosition + new Vector3(x, y, z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset to original position
        cameraTransform.localPosition = originalPosition;
        isShaking = false;
    }

    /// <summary>
    /// Quick shake for light impacts
    /// </summary>
    public void ShakeLight()
    {
        Shake(0.5f, 0.2f);
    }

    /// <summary>
    /// Medium shake for obstacle hits
    /// </summary>
    public void ShakeMedium()
    {
        Shake(1f, 0.3f);
    }

    /// <summary>
    /// Strong shake for crashes
    /// </summary>
    public void ShakeHeavy()
    {
        Shake(2f, 0.5f);
    }

    /// <summary>
    /// Stop shake immediately
    /// </summary>
    public void StopShake()
    {
        StopAllCoroutines();
        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalPosition;
        }
        isShaking = false;
    }
}
