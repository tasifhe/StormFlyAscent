# Pause Menu Setup Guide

## Overview
This guide shows how to set up the in-game pause menu with settings integration.

---

## 📋 Setup Instructions

### 1. Create UI Hierarchy in PLAYGROUND Scene

```
Canvas (Game UI Canvas)
├── PauseButton (Top-right corner)
│   └── Icon (Pause icon image)
│
├── PauseMenuPanel (Initially hidden)
│   ├── BlurBackground (Optional - semi-transparent dark image)
│   ├── MenuContainer (Panel background)
│   │   ├── TitleText ("PAUSED")
│   │   ├── ResumeButton
│   │   │   └── Text ("Resume")
│   │   ├── SettingsButton
│   │   │   └── Text ("Settings")
│   │   ├── MainMenuButton
│   │   │   └── Text ("Main Menu")
│   │   └── QuitButton (Optional)
│   │       └── Text ("Quit")
│   │
│   └── SettingsSubPanel (Initially hidden)
│       ├── SettingsContainer
│       │   ├── TitleText ("Settings")
│       │   ├── MusicVolumeSlider
│       │   │   ├── Label ("Music Volume")
│       │   │   └── Slider
│       │   ├── SFXVolumeSlider
│       │   │   ├── Label ("SFX Volume")
│       │   │   └── Slider
│       │   ├── VibrationToggle
│       │   │   ├── Label ("Vibration")
│       │   │   └── Toggle
│       │   └── BackButton
│       │       └── Text ("Back")
```

---

## 🎯 Component Setup

### PauseButton GameObject
1. Add **Image** component (pause icon)
2. Add **Button** component
3. Add **PauseButton** script
4. Position in top-right corner of screen

### PauseMenuPanel GameObject
1. Set as child of Canvas
2. Initially set **Active = false** in Inspector
3. Add **CanvasGroup** component (for fade animations)
4. Set anchors to stretch full screen

### PauseMenuManager GameObject
Create empty GameObject in scene root:
1. Name: "PauseMenuManager"
2. Add **PauseMenuManager** script
3. Configure references in Inspector:

**Menu Panels:**
- Pause Menu Panel: Drag PauseMenuPanel
- Settings Sub Panel: Drag SettingsSubPanel

**Buttons:**
- Resume Button: Drag ResumeButton
- Settings Button: Drag SettingsButton
- Main Menu Button: Drag MainMenuButton
- Quit Button: Drag QuitButton (optional)

**Settings Buttons:**
- Back From Settings Button: Drag BackButton in settings
- Music Volume Slider: Drag music slider
- SFX Volume Slider: Drag SFX slider
- Vibration Toggle: Drag vibration toggle

**Animation Settings:**
- Panel Fade Duration: `0.3`
- Panel Scale Duration: `0.4`
- Panel Ease In: `OutBack`
- Panel Ease Out: `InBack`

---

## 🎨 Quick UI Layout Tips

### Pause Button
- **RectTransform:**
  - Anchor: Top-Right
  - Pivot: (1, 1)
  - Position: (-20, -20)
  - Size: (60, 60)

### Pause Menu Panel
- **RectTransform:**
  - Anchor: Stretch (all sides)
  - Left/Right/Top/Bottom: 0
  - Makes it fill entire screen

### Menu Container
- **RectTransform:**
  - Anchor: Center
  - Size: (400, 600)
- **Image:** Semi-transparent dark background
- **Vertical Layout Group:** 
  - Spacing: 20
  - Padding: 40

### Settings Sub Panel
- Same as Menu Container
- Size: (500, 600)
- Positioned on top of pause menu

---

## ⚙️ Features

### Pause System
- Press **ESC** or **Android Back** to pause/unpause
- Click pause button to pause
- Time.timeScale freezes to 0 when paused

### Menu Navigation
- **Resume:** Closes menu and resumes game
- **Settings:** Opens settings sub-panel
- **Main Menu:** Returns to main menu scene
- **Quit:** Exits game

### Settings Integration
- Music volume slider
- SFX volume slider
- Vibration toggle
- Saves to PlayerPrefs automatically
- Integrates with existing SettingsManager

### Animations
- DOTween fade in/out animations
- Scale bounce effect
- Blur background (optional)
- Time-independent animations (works while paused)

---

## 🔊 Audio Integration (Optional)

If you want button click sounds, add to PauseMenuManager:

```csharp
[Header("Audio")]
[SerializeField] private AudioClip buttonClickSound;
[SerializeField] private AudioClip pauseSound;
[SerializeField] private AudioClip resumeSound;

private void PlaySound(AudioClip clip)
{
    if (clip != null)
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
}
```

---

## 🐛 Troubleshooting

### Pause menu doesn't appear
- Check that PauseMenuPanel is child of Canvas
- Verify PauseMenuManager has all references assigned
- Make sure Canvas is not disabled

### Time doesn't freeze
- Ensure Time.timeScale is set to 0 in PauseGame()
- Check no other scripts are resetting timeScale

### Settings don't save
- Verify SettingsManager exists in scene
- Check PlayerPrefs keys are correct
- Test in build (not just editor)

### Escape key doesn't work on mobile
- Android back button works the same way
- Add a pause button for touch input

---

## 📱 Mobile Considerations

- Make pause button larger (80x80 minimum)
- Add touch-friendly button spacing (min 100px apart)
- Test with finger tap sizes
- Consider adding swipe to pause gesture

---

## ✅ Testing Checklist

- [ ] Pause button pauses game
- [ ] ESC/Back button pauses/unpauses
- [ ] Resume button works
- [ ] Settings panel opens/closes
- [ ] Volume sliders work
- [ ] Vibration toggle works
- [ ] Settings are saved
- [ ] Main Menu button returns to menu
- [ ] Game resumes with correct time scale
- [ ] UI scales properly on different resolutions

---

## 🎯 Next Steps

After basic setup:
1. Add confirmation dialog for "Return to Main Menu"
2. Add restart level button
3. Add control remapping options
4. Add graphics quality settings
5. Style buttons with your game's art style
6. Add sound effects to button clicks
7. Add achievements/stats panel
