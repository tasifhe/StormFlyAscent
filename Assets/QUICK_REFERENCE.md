# Main Menu System - Quick Reference

## 📋 Scripts Overview

### Core Systems
| Script | Purpose | Location |
|--------|---------|----------|
| **SplashScreenManager** | "Tap to Start" intro screen | Attach to manager GameObject |
| **MainMenuManager** | Main menu navigation controller | Attach to manager GameObject |
| **SettingsManager** | Audio/Graphics/Controls settings | Attach to Settings Panel |
| **CustomizationManager** | Bird skin selection & unlocking | Attach to Customization Panel |
| **MenuSceneManager** | Scene loading & transitions | Attach to manager GameObject |

### UI Helpers
| Script | Purpose | Usage |
|--------|---------|-------|
| **ButtonEffects** | Button hover/press animations | Attach to any Button |
| **UIPanelAnimator** | Panel show/hide animations | Attach to any Panel |

---

## 🎯 Quick Setup Checklist

### Scene Hierarchy
```
Main Menu Scene
├── Canvas
│   ├── SplashPanel (CanvasGroup)
│   ├── MainMenuPanel (CanvasGroup, starts hidden)
│   ├── SettingsPanel (CanvasGroup, starts hidden)
│   ├── CustomizationPanel (CanvasGroup, starts hidden)
│   └── LoadingPanel (starts hidden)
├── SplashScreenManager (SplashScreenManager.cs)
├── MainMenuManager (MainMenuManager.cs)
└── MenuSceneManager (MenuSceneManager.cs)
```

### Build Settings
1. Add "Main Menu" scene
2. Add "SampleScene" (gameplay) scene

---

## 🔧 Essential Connections

### SplashScreenManager
- Splash Panel → SplashPanel GameObject
- Tap To Start Text → TextMeshPro component
- Fade Image → Full-screen Image

### MainMenuManager
- Main Menu Panel → MainMenuPanel
- Settings Panel → SettingsPanel
- Customization Panel → CustomizationPanel
- All 4 buttons (Start, Settings, Customization, Exit)

### SettingsManager
- Settings Panel → Self
- Back Button
- All sliders (Master, Music, SFX, Sensitivity)
- All toggles (Mute, VSync, InvertY, Vibration)
- All text labels for percentages
- Dropdowns (Quality, Resolution)

### CustomizationManager
- Customization Panel → Self
- Back Button
- Preview Image & Text fields
- Skin Scroll Content (the Content transform)
- Skin Button Prefab
- Select Button
- Coins Display Text
- **Add bird skins to Available Skins list!**

### MenuSceneManager
- Scene names: "Main Menu", "SampleScene"
- Loading Panel → LoadingPanel
- Progress Bar → Slider
- Percentage Text → TextMeshPro
- Tip Text → TextMeshPro
- Fade Image

---

## 📱 Key Features

### Splash Screen
- ✅ Touch/mouse/keyboard input detection
- ✅ Pulsing "Tap to Start" text
- ✅ Smooth fade transitions
- ✅ Auto-triggers main menu

### Main Menu
- ✅ 4 main buttons with animations
- ✅ Panel-based navigation
- ✅ Optional audio feedback
- ✅ Smooth show/hide transitions

### Settings
- ✅ Master/Music/SFX volume controls
- ✅ Mute toggle
- ✅ Quality & VSync settings
- ✅ Resolution selection
- ✅ Sensitivity slider
- ✅ Invert Y & Vibration toggles
- ✅ Auto-save with PlayerPrefs

### Customization
- ✅ Dynamic skin grid system
- ✅ Lock/unlock with coin currency
- ✅ Preview system
- ✅ Equipped indicator
- ✅ Cost display for locked skins
- ✅ Persistent unlocks

### Scene Loading
- ✅ Async loading with progress bar
- ✅ Minimum loading time
- ✅ Random loading tips
- ✅ Fade in/out transitions
- ✅ Don't Destroy On Load (optional)

---

## 🎨 Customization Points

### Colors
Edit in Inspector:
- Button colors (ButtonEffects)
- Panel backgrounds (Image components)
- Text colors (TextMeshPro)

### Animations
Edit in Inspector:
- Button scale/speed (ButtonEffects)
- Panel animation type (UIPanelAnimator)
- Fade durations (SplashScreenManager, MenuSceneManager)

### Content
Edit in Inspector/Code:
- Loading tips (MenuSceneManager)
- Bird skins list (CustomizationManager)
- Scene names (MenuSceneManager)

---

## 🔌 Gameplay Integration

### Start Game
```csharp
// Already handled by MainMenuManager → MenuSceneManager
```

### Return to Menu
```csharp
MenuSceneManager.Instance.LoadMainMenuScene();
```

### Award Coins
```csharp
CustomizationManager.AddCoins(10);
```

### Get Settings
```csharp
float sensitivity = SettingsManager.GetSensitivity();
bool invertY = SettingsManager.IsYAxisInverted();
bool vibration = SettingsManager.IsVibrationEnabled();
```

### Get Equipped Skin
```csharp
string skinName = CustomizationManager.GetEquippedSkinName();
```

---

## 🐛 Common Issues

| Problem | Solution |
|---------|----------|
| Splash doesn't work | Check MainMenuManager exists in scene |
| Buttons not clickable | Add EventSystem, check Canvas has GraphicRaycaster |
| Scene won't load | Verify scene names and Build Settings |
| Settings don't save | PlayerPrefs saves automatically, check for errors |
| Skins don't appear | Assign Skin Button Prefab, add skins to list |

---

## 📦 What's Included

✅ **7 Complete Scripts** ready to use
✅ **Full documentation** in MENU_SETUP_GUIDE.md
✅ **Mobile-optimized** touch input
✅ **Production-ready** code with comments
✅ **Modular design** - easy to extend
✅ **No external dependencies** (uses built-in Unity UI)

---

## 🚀 Next Steps

1. **Set up the UI hierarchy** (see MENU_SETUP_GUIDE.md)
2. **Create the required prefabs** (Skin Button)
3. **Assign all component references**
4. **Add bird skin data** to CustomizationManager
5. **Test the flow** (Splash → Menu → Settings/Customization)
6. **Add visual assets** (backgrounds, logos, icons)
7. **Optional: Add sound effects**

---

## 💡 Pro Tips

- Use CanvasGroup on panels for smooth fading
- Add ButtonEffects to all interactive buttons
- Test on actual mobile device for touch input
- Use the customization system for other unlockables too
- Extend SettingsManager for game-specific options
- Add more loading tips for variety

---

**Need help?** Check the detailed guide: `MENU_SETUP_GUIDE.md`
