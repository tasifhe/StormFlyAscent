using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Sliding toggle switch with handle movement and sprite changes
/// </summary>
public class ImageToggle : MonoBehaviour
{
    [Header("Required Components")]
    [Tooltip("The background image of the toggle (changes sprite)")]
    public Image backgroundImage;

    [Tooltip("The handle that slides left/right (changes sprite and position)")]
    public RectTransform handle;

    [Tooltip("The Image component on the handle")]
    public Image handleImage;

    [Header("Background Sprites")]
    [Tooltip("Background when toggle is OFF (gray)")]
    public Sprite backgroundOffSprite;

    [Tooltip("Background when toggle is ON (blue/green)")]
    public Sprite backgroundOnSprite;

    [Header("Handle Sprites")]
    [Tooltip("Handle sprite when OFF")]
    public Sprite handleOffSprite;

    [Tooltip("Handle sprite when ON")]
    public Sprite handleOnSprite;

    [Header("Handle Movement")]
    [Tooltip("X position when OFF (negative = left)")]
    public float offPositionX = -20f;

    [Tooltip("X position when ON (positive = right)")]
    public float onPositionX = 20f;

    [Tooltip("Speed of sliding animation")]
    public float slideSpeed = 15f;

    [Header("Settings")]
    public bool isOn = false;
    public string saveKey = "";
    public bool enableAnimation = true;

    [Header("Events")]
    public UnityEvent<bool> onValueChanged;

    private Button button;
    private Vector2 targetPosition;
    private bool isMoving = false;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    void Start()
    {
        // Load saved state
        if (!string.IsNullOrEmpty(saveKey))
        {
            isOn = PlayerPrefs.GetInt(saveKey, 0) == 1;
        }

        // Initialize visuals immediately
        UpdateVisualsImmediate();
    }

    void Update()
    {
        // Animate handle movement
        if (isMoving && handle != null && enableAnimation)
        {
            handle.anchoredPosition = Vector2.Lerp(
                handle.anchoredPosition,
                targetPosition,
                Time.deltaTime * slideSpeed
            );

            // Stop when close enough
            if (Vector2.Distance(handle.anchoredPosition, targetPosition) < 0.5f)
            {
                handle.anchoredPosition = targetPosition;
                isMoving = false;
            }
        }
    }

    public void OnClick()
    {
        Toggle();
    }

    public void Toggle()
    {
        SetValue(!isOn);
    }

    public void SetValue(bool value)
    {
        isOn = value;

        // Save to PlayerPrefs
        if (!string.IsNullOrEmpty(saveKey))
        {
            PlayerPrefs.SetInt(saveKey, isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        // Update visuals
        if (enableAnimation)
        {
            UpdateVisualsAnimated();
        }
        else
        {
            UpdateVisualsImmediate();
        }

        // Trigger event
        onValueChanged?.Invoke(isOn);

        Debug.Log($"Toggle [{saveKey}] is now: {(isOn ? "ON" : "OFF")}");
    }

    private void UpdateVisualsImmediate()
    {
        // Change background sprite
        if (backgroundImage != null)
        {
            if (isOn && backgroundOnSprite != null)
                backgroundImage.sprite = backgroundOnSprite;
            else if (!isOn && backgroundOffSprite != null)
                backgroundImage.sprite = backgroundOffSprite;
        }

        // Change handle sprite
        if (handleImage != null)
        {
            if (isOn && handleOnSprite != null)
                handleImage.sprite = handleOnSprite;
            else if (!isOn && handleOffSprite != null)
                handleImage.sprite = handleOffSprite;
        }

        // Move handle immediately
        if (handle != null)
        {
            float targetX = isOn ? onPositionX : offPositionX;
            Vector2 currentPos = handle.anchoredPosition;
            handle.anchoredPosition = new Vector2(targetX, currentPos.y);
            targetPosition = handle.anchoredPosition;
            isMoving = false;
        }
    }

    private void UpdateVisualsAnimated()
    {
        // Change sprites immediately
        if (backgroundImage != null)
        {
            if (isOn && backgroundOnSprite != null)
                backgroundImage.sprite = backgroundOnSprite;
            else if (!isOn && backgroundOffSprite != null)
                backgroundImage.sprite = backgroundOffSprite;
        }

        if (handleImage != null)
        {
            if (isOn && handleOnSprite != null)
                handleImage.sprite = handleOnSprite;
            else if (!isOn && handleOffSprite != null)
                handleImage.sprite = handleOffSprite;
        }

        // Animate handle position
        if (handle != null)
        {
            float targetX = isOn ? onPositionX : offPositionX;
            Vector2 currentPos = handle.anchoredPosition;
            targetPosition = new Vector2(targetX, currentPos.y);
            isMoving = true;
        }
    }

    // Public getter
    public bool IsOn => isOn;
}
