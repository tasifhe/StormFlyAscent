# Main Menu System - Setup Guide

## Overview
A complete mobile game main menu system with:
- **Splash Screen** - "Tap to Start" intro screen
- **Main Menu** - Start, Settings, Customization, Exit buttons
- **Settings Panel** - Audio, Graphics, and Controls options
- **Customization Panel** - Bird skin selection and unlocking
- **Scene Management** - Smooth transitions and loading screens

---

## Scripts Created

### Core Menu Scripts (Assets/_Script/UI/)
1. **SplashScreenManager.cs** - Handles the "Tap to Start" splash screen
2. **MainMenuManager.cs** - Controls main menu navigation
3. **SettingsManager.cs** - Manages all game settings
4. **CustomizationManager.cs** - Handles bird skin customization
5. **MenuSceneManager.cs** - Manages scene loading and transitions

### UI Helper Scripts
6. **ButtonEffects.cs** - Adds hover/press animations to buttons
7. **UIPanelAnimator.cs** - Animates panel show/hide transitions

---

## Scene Setup Instructions

### 1. Main Menu Scene Setup

#### A. Canvas Setup
1. Create a Canvas (if not exists)
   - Canvas Scaler: Scale with Screen Size
   - Reference Resolution: 1920x1080 (or your target)
   - Match: 0.5 (Width/Height balance)

#### B. Create Splash Screen Panel
```
Canvas
└── SplashPanel (GameObject)
    ├── Background (Image) - Dark background
    ├── Logo (Image) - Your game logo
    ├── TapToStartText (TextMeshPro) - "TAP TO START"
    └── FadeImage (Image) - Full-screen black image for fade
```

**Setup:**
- SplashPanel: Add `CanvasGroup` component
- TapToStartText: Center on screen, nice font
- FadeImage: Set color to black, alpha to 0, Raycast Target OFF

#### C. Create Main Menu Panel
```
Canvas
└── MainMenuPanel (GameObject)
    ├── Background (Image) - Menu background
    ├── Title (TextMeshPro) - Game title
    ├── ButtonContainer (VerticalLayoutGroup)
    │   ├── StartButton (Button + TextMeshPro)
    │   ├── SettingsButton (Button + TextMeshPro)
    │   ├── CustomizationButton (Button + TextMeshPro)
    │   └── ExitButton (Button + TextMeshPro)
    └── Logo (Image) - Optional logo
```

**Setup:**
- MainMenuPanel: Add `CanvasGroup` component
- ButtonContainer: Use VerticalLayoutGroup for auto-spacing
- Each button: Add `ButtonEffects.cs` for animations

#### D. Create Settings Panel
```
Canvas
└── SettingsPanel (GameObject)
    ├── Background (Image)
    ├── Title (TextMeshPro) - "SETTINGS"
    ├── BackButton (Button)
    ├── ScrollView
    │   └── Content
    │       ├── AudioSection
    │       │   ├── MasterVolumeSlider (Slider + Text)
    │       │   ├── MusicVolumeSlider (Slider + Text)
    │       │   ├── SFXVolumeSlider (Slider + Text)
    │       │   └── MuteToggle (Toggle)
    │       ├── GraphicsSection
    │       │   ├── QualityDropdown (TMP_Dropdown)
    │       │   ├── VSyncToggle (Toggle)
    │       │   └── ResolutionDropdown (TMP_Dropdown)
    │       └── ControlsSection
    │           ├── SensitivitySlider (Slider + Text)
    │           ├── InvertYToggle (Toggle)
    │           └── VibrationToggle (Toggle)
    └── ResetButton (Optional)
```

**Setup:**
- SettingsPanel: Add `CanvasGroup`, set Active = false initially
- Use Content Size Fitter on sections for proper scrolling

#### E. Create Customization Panel
```
Canvas
└── CustomizationPanel (GameObject)
    ├── Background (Image)
    ├── Title (TextMeshPro) - "CUSTOMIZE"
    ├── BackButton (Button)
    ├── PreviewSection
    │   ├── BirdPreviewImage (Image) - Large preview
    │   ├── BirdName (TextMeshPro)
    │   └── BirdDescription (TextMeshPro)
    ├── SkinScrollView
    │   └── Content (Transform)
    │       └── (Skin buttons spawn here)
    ├── SelectButton (Button)
    └── CoinsDisplay (TextMeshPro + Image)
```

**Setup:**
- CustomizationPanel: Add `CanvasGroup`, set Active = false initially
- Create a SkinButtonPrefab (see below)

#### F. Create Loading Panel
```
Canvas
└── LoadingPanel (GameObject)
    ├── Background (Image) - Full screen
    ├── LoadingBar (Slider)
    ├── PercentageText (TextMeshPro)
    ├── LoadingTip (TextMeshPro)
    └── FadeImage (Image)
```

**Setup:**
- LoadingPanel: Set Active = false initially
- LoadingBar: Min = 0, Max = 1

---

### 2. Create Prefabs

#### A. Skin Button Prefab
```
SkinButtonPrefab (Button)
├── PreviewImage (Image) - Small skin preview
├── NameText (TextMeshPro) - Skin name
├── LockIcon (Image) - Lock icon (hidden when unlocked)
├── CostText (TextMeshPro) - Cost in coins
└── EquippedIndicator (Image) - Checkmark or border
```

---

### 3. Component Setup

#### A. SplashScreenManager Component
1. Create an empty GameObject named "SplashScreenManager"
2. Add `SplashScreenManager.cs` component
3. Assign references:
   - Splash Panel → SplashPanel GameObject
   - Tap To Start Text → TapToStartText
   - Fade Image → FadeImage
4. Adjust animation settings as desired

#### B. MainMenuManager Component
1. Create an empty GameObject named "MainMenuManager"
2. Add `MainMenuManager.cs` component
3. Assign references:
   - Main Menu Panel → MainMenuPanel GameObject
   - Settings Panel → SettingsPanel GameObject
   - Customization Panel → CustomizationPanel GameObject
   - All buttons (Start, Settings, Customization, Exit)
4. Set "Hide On Start" = true

#### C. SettingsManager Component
1. Add `SettingsManager.cs` to SettingsPanel GameObject
2. Assign ALL UI references:
   - Settings Panel → Self
   - Back Button
   - All sliders and toggles
   - All text labels
3. Optional: Assign AudioMixer if using one

#### D. CustomizationManager Component
1. Add `CustomizationManager.cs` to CustomizationPanel GameObject
2. Assign references:
   - Customization Panel → Self
   - Back Button
   - Bird Preview Image
   - Selected Bird Name/Description
   - Skin Scroll Content (the Content transform)
   - Skin Button Prefab
   - Select Button
   - Coins Text
3. Add bird skins to the "Available Skins" list:
   - Expand "Available Skins"
   - Set Size (e.g., 5 for 5 skins)
   - For each skin:
     - Name: "Eagle", "Hawk", "Owl", etc.
     - Description: Brief description
     - Preview Image: Assign sprite
     - Cost: 0 for default, 100+ for unlockable
     - Is Unlocked: Check for default skins

#### E. MenuSceneManager Component
1. Create an empty GameObject named "MenuSceneManager"
2. Add `MenuSceneManager.cs` component
3. Assign references:
   - Main Menu Scene Name: "Main Menu"
   - Game Scene Name: "SampleScene" (or your gameplay scene)
   - Loading Panel → LoadingPanel GameObject
   - Loading Progress Bar → Slider
   - Loading Percentage Text → TextMeshPro
   - Loading Tip Text → TextMeshPro
   - Fade Image → FadeImage

---

## 4. Build Settings Setup

1. Open **File > Build Settings**
2. Add scenes in order:
   - Scene 0: Main Menu
   - Scene 1: SampleScene (gameplay)
3. Click "Add Open Scenes" or drag scenes from Project window

---

## 5. Testing the System

### Test Flow:
1. **Play Main Menu scene**
2. See "Tap to Start" splash screen
3. Click anywhere to proceed
4. See Main Menu with 4 buttons
5. Click "Start" → Should load gameplay scene (with loading screen)
6. Click "Settings" → Opens settings panel with all options
7. Click "Customization" → Opens customization with bird skins
8. Click "Exit" → Quits game (or stops play mode)

### Test Settings:
- Adjust audio sliders → Volume should change
- Toggle mute → Audio should mute
- Change quality → Graphics quality should update
- Adjust sensitivity → Value should save

### Test Customization:
- Click skin buttons → Preview updates
- Click "SELECT" → Skin equips (shows "EQUIPPED")
- Test locked skins → Shows cost and lock icon
- Add test coins (inspector button) → Can unlock skins

---

## 6. Customization

### Add More Loading Tips:
Edit `MenuSceneManager.cs` → Loading Tips array

### Change Button Animations:
Select buttons → Adjust `ButtonEffects` component settings

### Add Panel Animations:
Add `UIPanelAnimator.cs` to any panel for show/hide animations

### Custom Bird Skins:
1. Create bird prefab variants
2. Create preview sprites
3. Add to CustomizationManager's Available Skins list

---

## 7. Integration with Gameplay

### Loading the Game:
The "Start" button automatically loads the game scene defined in MenuSceneManager.

### Returning to Menu:
In your gameplay scene, create a pause menu or game over screen:
```csharp
MenuSceneManager.Instance.LoadMainMenuScene();
```

### Award Coins:
When player collects coins in gameplay:
```csharp
CustomizationManager.AddCoins(10);
```

### Get Settings:
In gameplay scripts:
```csharp
float sensitivity = SettingsManager.GetSensitivity();
bool invertY = SettingsManager.IsYAxisInverted();
bool vibration = SettingsManager.IsVibrationEnabled();
```

### Apply Equipped Skin:
In your player spawn script:
```csharp
string equippedSkin = CustomizationManager.GetEquippedSkinName();
// Load the appropriate bird model based on skin name
```

---

## 8. Optional Enhancements

### Add Sound Effects:
1. Import button click sounds
2. Assign to MainMenuManager's audio clips
3. Assign to ButtonEffects components

### Add Background Music:
1. Create an AudioSource with music
2. Set it to "Don't Destroy On Load"
3. Control via SettingsManager's music volume

### Add More Panels:
- Credits panel
- Achievements panel
- Leaderboard panel
- Shop panel

### Add Confirmation Dialogs:
Create a popup dialog prefab for:
- Exit confirmation
- Purchase confirmation
- Reset settings warning

---

## Troubleshooting

### "Splash screen doesn't transition"
- Ensure MainMenuManager exists in scene
- Check SplashScreenManager's OnTapDetected event is connected

### "Buttons don't work"
- Ensure Canvas has GraphicRaycaster component
- Ensure EventSystem exists in scene
- Check button's Interactable is enabled

### "Scene doesn't load"
- Verify scene names in MenuSceneManager match exactly
- Ensure scenes are added to Build Settings
- Check for errors in Console

### "Settings don't save"
- PlayerPrefs works automatically
- Check for errors in SettingsManager
- Test in build (not just editor)

### "Customization skins don't show"
- Verify Skin Button Prefab is assigned
- Check Available Skins list has entries
- Ensure prefab has required child objects

---

## Summary

You now have a complete, professional main menu system with:
✅ Splash screen with tap-to-start
✅ Main menu with navigation
✅ Full settings panel (audio, graphics, controls)
✅ Customization system with unlockable skins
✅ Smooth scene transitions with loading screen
✅ Button animations and effects
✅ Mobile-friendly touch input
✅ Persistent settings and unlocks

The system is modular, extensible, and production-ready!
