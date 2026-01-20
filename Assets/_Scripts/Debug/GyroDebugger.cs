using UnityEngine;

/// <summary>
/// Simple debugger to visualize Gyro/Accelerometer values on screen
/// Add this to any GameObject in the scene to see the values in the build
/// </summary>
public class GyroDebugger : MonoBehaviour
{
    private GUIStyle style;

    private void Start()
    {
        style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        float accelX = Input.acceleration.x;
        float accelY = Input.acceleration.y;
        float accelZ = Input.acceleration.z;

        bool isEnabled = SettingsManager.IsGyroControlEnabled();

        GUILayout.BeginArea(new Rect(50, 50, Screen.width, Screen.height));

        GUILayout.Label($"Gyro Enabled: {isEnabled}", style);
        GUILayout.Label($"Accel X: {accelX:F3}", style);
        GUILayout.Label($"Accel Y: {accelY:F3}", style);
        GUILayout.Label($"Accel Z: {accelZ:F3}", style);

        GUILayout.EndArea();
    }
}
