using UnityEngine;

/// <summary>
/// CharacterCameraController - Simple camera root that follows and rotates with the character
/// The camera root will match the character's position and rotation
/// </summary>
public class CharacterCameraController : MonoBehaviour
{
    [Header("Character Reference")]
    [Tooltip("The character to follow")]
    public Transform character;

    [Header("Follow Settings")]
    [Tooltip("Time it takes for the camera to reach the character position")]
    public float positionSmoothTime = 0.1f;

    [Tooltip("Time it takes for the camera to match character rotation")]
    public float rotationSmoothTime = 0.1f;

    private Vector3 positionVelocity;
    private Vector3 rotationVelocity;

   

    private void LateUpdate()
    {
        if (character == null) return;

        // Smoothly follow character's position using SmoothDamp
        transform.position = Vector3.SmoothDamp(
            transform.position,
            character.transform.position,
            ref positionVelocity,
            positionSmoothTime
        );

        // Smoothly match character's rotation using euler angles with SmoothDamp
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 targetRotation = character.transform.rotation.eulerAngles;

        Vector3 smoothedRotation = new Vector3(
            Mathf.SmoothDampAngle(currentRotation.x, targetRotation.x, ref rotationVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentRotation.y, targetRotation.y, ref rotationVelocity.y, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentRotation.z, targetRotation.z, ref rotationVelocity.z, rotationSmoothTime)
        );

        transform.rotation = Quaternion.Euler(smoothedRotation);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || character == null) return;

        // Draw line to character
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, character.transform.position);

        // Draw camera position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
