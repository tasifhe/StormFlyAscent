using UnityEngine;

public class CoreCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private Transform playerTransform;

    [Header("Position Settings")]
    [SerializeField] private float positionSmoothSpeed = 5f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("Rotation Settings")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float rotationSmoothSpeed = 5f;
    [SerializeField] private float horizontalRotationAmount = 5f;
    [SerializeField] private float verticalRotationAmount = 5f;
    [SerializeField] private float maxHorizontalRotation = 45f;
    [SerializeField] private float maxVerticalRotation = 45f;

    private void LateUpdate()
    {
        if (cameraHolder == null || playerTransform == null) return;

        Vector3 targetPosition = cameraHolder.localPosition;
        targetPosition.x = Mathf.Lerp(cameraHolder.localPosition.x, playerTransform.localPosition.x + offset.x, positionSmoothSpeed * Time.deltaTime);
        cameraHolder.localPosition = targetPosition;

        if (enableRotation)
        {
            Vector3 directionToPlayer = playerTransform.localPosition - cameraHolder.localPosition;
            float targetRotationX = Mathf.Clamp(directionToPlayer.y * verticalRotationAmount, -maxVerticalRotation, maxVerticalRotation);
            float targetRotationY = Mathf.Clamp(directionToPlayer.x * horizontalRotationAmount, -maxHorizontalRotation, maxHorizontalRotation);

            Quaternion targetRotation = Quaternion.Euler(targetRotationX, targetRotationY, 0f);
            cameraHolder.localRotation = Quaternion.Lerp(cameraHolder.localRotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }
}
