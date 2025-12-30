using UnityEngine;

/// <summary>
/// Smooths input for AC-style responsive feel
/// Uses SmoothDamp for natural acceleration/deceleration
/// </summary>
public class InputSmoother
{
    private Vector3 smoothedInput = Vector3.zero;
    private Vector3 inputVelocity = Vector3.zero;
    private float responsiveness;
    
    public InputSmoother(float responsiveness = 10f)
    {
        this.responsiveness = responsiveness;
    }
    
    /// <summary>
    /// Get current smoothed input value
    /// </summary>
    public Vector3 SmoothedInput => smoothedInput;
    
    /// <summary>
    /// Smooth raw input using SmoothDamp
    /// Call this every frame with raw input
    /// </summary>
    /// <param name="rawInput">Raw input vector</param>
    /// <returns>Smoothed input vector</returns>
    public Vector3 SmoothInput(Vector3 rawInput)
    {
        float smoothTime = 1f / responsiveness;
        smoothedInput = Vector3.SmoothDamp(smoothedInput, rawInput, ref inputVelocity, smoothTime);
        return smoothedInput;
    }
    
    /// <summary>
    /// Reset smoother to zero (useful when entering state)
    /// </summary>
    public void Reset()
    {
        smoothedInput = Vector3.zero;
        inputVelocity = Vector3.zero;
    }
    
    /// <summary>
    /// Update responsiveness value
    /// </summary>
    public void SetResponsiveness(float newResponsiveness)
    {
        responsiveness = newResponsiveness;
    }
}
