using UnityEngine;

/// <summary>
/// Controls obstacle movement patterns (static, up/down, left/right)
/// Attach this to obstacles that should move
/// </summary>
public class ObstacleMovement : MonoBehaviour
{
    public enum MovementType
    {
        Static,
        VerticalOscillate,      // Up and down
        HorizontalOscillate,    // Left and right
        DiagonalOscillate,      // Diagonal movement
        Circular                // Circular motion
    }
    
    [Header("Movement Settings")]
    [Tooltip("Type of movement for this obstacle")]
    public MovementType movementType = MovementType.Static;
    
    [Header("Oscillation Settings")]
    [Tooltip("Distance to move from starting position")]
    public float moveDistance = 3f;
    
    [Tooltip("Speed of movement")]
    public float moveSpeed = 2f;
    
    [Tooltip("Start with random offset in movement cycle")]
    public bool randomStartOffset = true;
    
    [Header("Circular Settings")]
    [Tooltip("Radius of circular movement")]
    public float circularRadius = 2f;
    
    private Vector3 startPosition;
    private float timeOffset;
    
    private void Awake()
    {
        startPosition = transform.localPosition;
        
        if (randomStartOffset)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }
    
    private void Update()
    {
        if (movementType == MovementType.Static)
            return;
        
        float time = Time.time * moveSpeed + timeOffset;
        
        switch (movementType)
        {
            case MovementType.VerticalOscillate:
                MoveVertical(time);
                break;
                
            case MovementType.HorizontalOscillate:
                MoveHorizontal(time);
                break;
                
            case MovementType.DiagonalOscillate:
                MoveDiagonal(time);
                break;
                
            case MovementType.Circular:
                MoveCircular(time);
                break;
        }
    }
    
    private void MoveVertical(float time)
    {
        float offsetY = Mathf.Sin(time) * moveDistance;
        transform.localPosition = startPosition + new Vector3(0f, offsetY, 0f);
    }
    
    private void MoveHorizontal(float time)
    {
        float offsetX = Mathf.Sin(time) * moveDistance;
        transform.localPosition = startPosition + new Vector3(offsetX, 0f, 0f);
    }
    
    private void MoveDiagonal(float time)
    {
        float offsetX = Mathf.Sin(time) * moveDistance;
        float offsetY = Mathf.Cos(time) * moveDistance;
        transform.localPosition = startPosition + new Vector3(offsetX, offsetY, 0f);
    }
    
    private void MoveCircular(float time)
    {
        float x = Mathf.Cos(time) * circularRadius;
        float y = Mathf.Sin(time) * circularRadius;
        transform.localPosition = startPosition + new Vector3(x, y, 0f);
    }
    
    /// <summary>
    /// Reset to starting position (for object pooling)
    /// </summary>
    public void ResetPosition()
    {
        transform.localPosition = startPosition;
        
        if (randomStartOffset)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }
    
    /// <summary>
    /// Set a new starting position
    /// </summary>
    public void SetStartPosition(Vector3 newPosition)
    {
        startPosition = newPosition;
        transform.localPosition = startPosition;
    }
}
