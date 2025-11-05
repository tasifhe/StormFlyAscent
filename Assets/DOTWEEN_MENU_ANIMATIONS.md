# DOTween Menu Animations Guide

## Overview
The `MainMenuManager.cs` now uses **DOTween** for smooth, professional menu animations instead of manual coroutines.

## Features Implemented

### 🎨 Panel Animations
1. **Fade In/Out** - Smooth alpha transitions
2. **Scale Animations** - Panels pop in/out with elastic easing
3. **Combined Effects** - Fade + Scale for polished look

### 🎯 Button Animations
1. **Hover Effects** - Buttons scale up on mouse hover
2. **Click Animations** - Punch scale effect when clicked
3. **Automatic Setup** - Hover effects added in Awake()

## Customizable Settings

### In Inspector (MainMenuManager Component)

#### DOTween Animation Settings:
- **Panel Fade Duration** (0.4s) - How fast panels fade
- **Panel Scale Duration** (0.5s) - How fast panels scale
- **Button Scale Duration** (0.2s) - How fast buttons animate
- **Panel Ease In** (OutBack) - Easing curve for panel appearance
- **Panel Ease Out** (InBack) - Easing curve for panel disappearance
- **Panel Start Scale** (0.8, 0.8, 1) - Initial scale for pop-in effect
- **Button Hover Scale** (1.1) - Scale multiplier on hover

### Available Ease Types
Try different easing curves for different feels:
- `Ease.OutBack` - Bouncy overshoot (recommended for panels)
- `Ease.OutElastic` - Spring-like bounce
- `Ease.OutBounce` - Bouncing effect
- `Ease.OutQuad` - Smooth deceleration
- `Ease.Linear` - Constant speed
- `Ease.InOutSine` - Smooth acceleration/deceleration

## Animation Breakdown

### ShowPanel() Method
```csharp
// Automatically adds CanvasGroup if needed
// Animates from panelStartScale (0.8) to 1.0
// Fades from alpha 0 to 1
// Uses OutBack easing for bouncy entrance
```

### HidePanel() Method
```csharp
// Animates from scale 1.0 to panelStartScale (0.8)
// Fades from alpha 1 to 0
// Uses InBack easing for smooth exit
// Deactivates panel after animation completes
```

### Button Hover
```csharp
// Scale up to buttonHoverScale (1.1) on hover
// Scale back to 1.0 on exit
// Smooth OutQuad easing
```

### Button Click
```csharp
// DOPunchScale creates a "press" effect
// Scales down briefly then bounces back
```

## How to Adjust Animations

### Make Panels Bounce More
1. Increase `Panel Start Scale` to 0.6 or 0.5
2. Change `Panel Ease In` to `Ease.OutElastic`
3. Increase `Panel Scale Duration` to 0.7s

### Make Buttons More Responsive
1. Decrease `Button Scale Duration` to 0.1s
2. Increase `Button Hover Scale` to 1.2
3. Change hover ease in code to `Ease.OutBack`

### Make Everything Faster
1. Decrease all duration values (0.2s, 0.3s, 0.1s)
2. Or globally: `DOTween.SetTimeScale(1.5f)` for 1.5x speed

### Make Everything Smoother
1. Use `Ease.InOutSine` or `Ease.InOutQuad`
2. Increase duration values slightly
3. Set Panel Start Scale closer to 1.0 (like 0.95)

## Advanced Customization

### Add Rotation to Panels
In `ShowPanel()` method, add:
```csharp
rectTransform.rotation = Quaternion.Euler(0, 0, 10);
rectTransform.DORotate(Vector3.zero, panelScaleDuration).SetEase(panelEaseIn);
```

### Add Slide-In Effect
Before scaling in `ShowPanel()`, add:
```csharp
rectTransform.anchoredPosition = new Vector2(0, -500); // Start below screen
rectTransform.DOAnchorPos(Vector2.zero, panelScaleDuration).SetEase(panelEaseIn);
```

### Sequential Button Animations
To animate buttons one after another:
```csharp
startButton.transform.DOScale(1, 0.3f).From(0).SetDelay(0.0f);
settingsButton.transform.DOScale(1, 0.3f).From(0).SetDelay(0.1f);
customizationButton.transform.DOScale(1, 0.3f).From(0).SetDelay(0.2f);
exitButton.transform.DOScale(1, 0.3f).From(0).SetDelay(0.3f);
```

## Performance Notes

- ✅ DOTween is highly optimized
- ✅ All tweens are properly killed in `OnDestroy()`
- ✅ Prevents memory leaks and duplicate animations
- ✅ Uses `DOVirtual.DelayedCall` instead of coroutines

## Troubleshooting

### Animations Don't Work?
1. Check DOTween is imported (it is in Plugins/Demigiant)
2. Ensure panels have RectTransform components
3. Verify button references are assigned in Inspector

### Animations Stutter?
1. Check V-Sync is enabled
2. Ensure Time.timeScale = 1
3. Try reducing number of simultaneous animations

### Buttons Don't Hover?
1. Check buttons have EventTrigger component (auto-added)
2. Ensure Canvas has GraphicRaycaster
3. Verify EventSystem exists in scene

## Quick Tips

🎯 **Best Practices:**
- Keep durations between 0.2s - 0.6s for snappy feel
- Use OutBack/OutElastic for menu entries
- Use InBack/Linear for menu exits
- Button animations should be faster than panels

🚀 **Pro Effects:**
- Layer multiple animations (fade + scale + rotate)
- Use `.SetDelay()` for sequenced animations
- Chain animations with `.OnComplete()`
- Use `.SetLoops()` for idle animations

📊 **Recommended Settings:**
- **Fast/Responsive:** 0.2s durations, OutQuad easing
- **Bouncy/Playful:** 0.5s durations, OutBack easing
- **Smooth/Professional:** 0.4s durations, InOutSine easing
- **Dramatic/Game:** 0.6s durations, OutElastic easing

## Resources

- [DOTween Documentation](http://dotween.demigiant.com/documentation.php)
- [DOTween Examples](http://dotween.demigiant.com/examples.php)
- DOTween Pro included in: `Assets/Plugins/Demigiant/DOTweenPro/`

---

**Ready to animate!** 🎉 Adjust the settings in the Inspector and see instant results!
