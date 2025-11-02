# 🎮 Main Menu System - Visual Flow Diagram

## System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    MAIN MENU SYSTEM                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  1. GAME STARTS                                             │
│                                                              │
│  ┌──────────────────────────────────────┐                  │
│  │   SplashScreenManager.cs              │                  │
│  │   • "Tap to Start" text (pulsing)     │                  │
│  │   • Detects touch/mouse/keyboard      │                  │
│  │   • Fade in/out transitions           │                  │
│  └──────────────────┬───────────────────┘                  │
│                     │ [TAP DETECTED]                        │
│                     ▼                                        │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│  2. MAIN MENU SHOWS                                         │
│                                                              │
│  ┌──────────────────────────────────────┐                  │
│  │   MainMenuManager.cs                  │                  │
│  │   ┌──────────────────────────────┐   │                  │
│  │   │  [START BUTTON] ───────┐     │   │                  │
│  │   │  [SETTINGS BUTTON] ────┼──┐  │   │                  │
│  │   │  [CUSTOMIZATION]───────┼──┼─┐│   │                  │
│  │   │  [EXIT BUTTON]          │  │ ││   │                  │
│  │   └──────────────────────────────┘   │                  │
│  └──────┬───────────┬──────────┬────────┘                  │
│         │           │          │                             │
│         ▼           ▼          ▼                             │
└─────────────────────────────────────────────────────────────┘

┌──────────────┐    ┌──────────────┐    ┌──────────────────┐
│  3a. START   │    │ 3b. SETTINGS │    │ 3c. CUSTOMIZATION│
│              │    │              │    │                  │
│ MenuScene    │    │ Settings     │    │ Customization    │
│ Manager.cs   │    │ Manager.cs   │    │ Manager.cs       │
│              │    │              │    │                  │
│ • Loading    │    │ AUDIO:       │    │ • Skin Grid      │
│   Screen     │    │ - Master Vol │    │ • Preview        │
│ • Progress   │    │ - Music Vol  │    │ • Lock/Unlock    │
│   Bar        │    │ - SFX Vol    │    │ • Coin System    │
│ • Random     │    │ - Mute       │    │ • Equip System   │
│   Tips       │    │              │    │ • Persistence    │
│ • Fade       │    │ GRAPHICS:    │    │                  │
│              │    │ - Quality    │    │ [BACK] → Menu    │
│      ▼       │    │ - VSync      │    │                  │
│  GAMEPLAY    │    │ - Resolution │    └──────────────────┘
│   SCENE      │    │              │    
│              │    │ CONTROLS:    │    
└──────────────┘    │ - Sensitivity│    
                    │ - Invert Y   │    
                    │ - Vibration  │    
                    │              │    
                    │ [BACK] → Menu│    
                    └──────────────┘    

┌─────────────────────────────────────────────────────────────┐
│  HELPER SYSTEMS (Available Everywhere)                      │
│                                                              │
│  ┌──────────────────────────┐  ┌────────────────────────┐ │
│  │  ButtonEffects.cs        │  │  UIPanelAnimator.cs    │ │
│  │  • Hover scale           │  │  • Scale animation     │ │
│  │  • Press scale           │  │  • Fade animation      │ │
│  │  • Color tint            │  │  • Slide animation     │ │
│  │  • Audio feedback        │  │  • Custom curves       │ │
│  └──────────────────────────┘  └────────────────────────┘ │
│                                                              │
│  ┌──────────────────────────┐                               │
│  │  UIAudioManager.cs       │                               │
│  │  • Centralized sounds    │                               │
│  │  • Easy access           │                               │
│  │  • Volume control        │                               │
│  └──────────────────────────┘                               │
└─────────────────────────────────────────────────────────────┘
```

---

## Scene Hierarchy Structure

```
Main Menu Scene
│
├── Canvas (Canvas Scaler: Scale with Screen Size)
│   ├── EventSystem (Auto-created)
│   │
│   ├── 🎬 SplashPanel (CanvasGroup, Active on start)
│   │   ├── Background (Image - Dark)
│   │   ├── Logo (Image - Optional)
│   │   ├── TapToStartText (TextMeshPro - Animated)
│   │   └── FadeImage (Image - Black, Alpha 0)
│   │
│   ├── 📋 MainMenuPanel (CanvasGroup, Hidden on start)
│   │   ├── Background (Image)
│   │   ├── Title (TextMeshPro)
│   │   ├── ButtonContainer (VerticalLayoutGroup)
│   │   │   ├── StartButton (Button + ButtonEffects)
│   │   │   ├── SettingsButton (Button + ButtonEffects)
│   │   │   ├── CustomizationButton (Button + ButtonEffects)
│   │   │   └── ExitButton (Button + ButtonEffects)
│   │   └── Logo (Image - Optional)
│   │
│   ├── ⚙️ SettingsPanel (CanvasGroup, Hidden on start)
│   │   ├── Background (Image)
│   │   ├── Title (TextMeshPro - "SETTINGS")
│   │   ├── BackButton (Button + ButtonEffects)
│   │   └── ScrollView
│   │       └── Content
│   │           ├── 🔊 AudioSection
│   │           │   ├── Header
│   │           │   ├── MasterVolume (Slider + Text)
│   │           │   ├── MusicVolume (Slider + Text)
│   │           │   ├── SFXVolume (Slider + Text)
│   │           │   └── MuteToggle (Toggle)
│   │           │
│   │           ├── 🎨 GraphicsSection
│   │           │   ├── Header
│   │           │   ├── QualityDropdown (TMP_Dropdown)
│   │           │   ├── VSyncToggle (Toggle)
│   │           │   └── ResolutionDropdown (TMP_Dropdown)
│   │           │
│   │           └── 🎮 ControlsSection
│   │               ├── Header
│   │               ├── SensitivitySlider (Slider + Text)
│   │               ├── InvertYToggle (Toggle)
│   │               └── VibrationToggle (Toggle)
│   │
│   ├── 🎨 CustomizationPanel (CanvasGroup, Hidden on start)
│   │   ├── Background (Image)
│   │   ├── Title (TextMeshPro - "CUSTOMIZE")
│   │   ├── BackButton (Button + ButtonEffects)
│   │   ├── PreviewSection
│   │   │   ├── PreviewImage (Image - Large)
│   │   │   ├── SkinName (TextMeshPro)
│   │   │   └── SkinDescription (TextMeshPro)
│   │   │
│   │   ├── SkinScrollView
│   │   │   └── Content (Grid Layout Group)
│   │   │       └── [Skin buttons spawn here]
│   │   │
│   │   ├── SelectButton (Button + ButtonEffects)
│   │   └── CoinsDisplay
│   │       ├── CoinIcon (Image)
│   │       └── CoinText (TextMeshPro)
│   │
│   └── ⏳ LoadingPanel (Hidden on start)
│       ├── Background (Image - Full screen)
│       ├── LoadingBar (Slider)
│       │   ├── Background
│       │   ├── Fill Area
│       │   └── Handle (Optional)
│       ├── PercentageText (TextMeshPro - "0%")
│       ├── LoadingTip (TextMeshPro - Tips)
│       └── FadeImage (Image - Black)
│
├── 🎬 SplashScreenManager (Empty GameObject)
│   └── SplashScreenManager.cs
│
├── 📋 MainMenuManager (Empty GameObject)
│   └── MainMenuManager.cs
│
├── 🔄 MenuSceneManager (Empty GameObject)
│   └── MenuSceneManager.cs (DontDestroyOnLoad)
│
└── 🔊 UIAudioManager (Empty GameObject)
    └── UIAudioManager.cs (DontDestroyOnLoad)
```

---

## Component Connection Map

```
┌────────────────────────────────────────────────────────────┐
│  COMPONENT REFERENCES (What connects to what)              │
└────────────────────────────────────────────────────────────┘

SplashScreenManager.cs
├── Splash Panel ──────────► SplashPanel GameObject
├── Tap To Start Text ─────► TextMeshPro component
├── Fade Image ────────────► Full-screen Image
└── On Tap Detected Event ─► MainMenuManager.ShowMainMenu()

MainMenuManager.cs
├── Main Menu Panel ───────► MainMenuPanel GameObject
├── Settings Panel ────────► SettingsPanel GameObject
├── Customization Panel ───► CustomizationPanel GameObject
├── Start Button ──────────► Button component
├── Settings Button ───────► Button component
├── Customization Button ──► Button component
└── Exit Button ───────────► Button component

SettingsManager.cs (On SettingsPanel)
├── Settings Panel ────────► Self (SettingsPanel)
├── Back Button ───────────► Button component
├── Master Volume Slider ──► Slider component
├── Music Volume Slider ───► Slider component
├── SFX Volume Slider ─────► Slider component
├── Mute Toggle ───────────► Toggle component
├── Quality Dropdown ──────► TMP_Dropdown component
├── VSync Toggle ──────────► Toggle component
├── Resolution Dropdown ───► TMP_Dropdown component
├── Sensitivity Slider ────► Slider component
├── Invert Y Toggle ───────► Toggle component
├── Vibration Toggle ──────► Toggle component
└── All Text Labels ───────► TextMeshPro components

CustomizationManager.cs (On CustomizationPanel)
├── Customization Panel ───► Self (CustomizationPanel)
├── Back Button ───────────► Button component
├── Bird Preview Image ────► Image component
├── Selected Bird Name ────► TextMeshPro component
├── Selected Bird Desc ────► TextMeshPro component
├── Skin Scroll Content ───► Transform (Content)
├── Skin Button Prefab ────► Prefab with structure
├── Select Button ─────────► Button component
├── Coins Text ────────────► TextMeshPro component
└── Available Skins List ──► Filled in Inspector

MenuSceneManager.cs
├── Main Menu Scene Name ──► "Main Menu" (string)
├── Game Scene Name ───────► "SampleScene" (string)
├── Loading Panel ─────────► LoadingPanel GameObject
├── Loading Progress Bar ──► Slider component
├── Loading Percentage ────► TextMeshPro component
├── Loading Tip Text ──────► TextMeshPro component
└── Fade Image ────────────► Image component
```

---

## Data Flow Diagram

```
┌────────────────────────────────────────────────────────────┐
│  USER ACTIONS → SYSTEM RESPONSES                           │
└────────────────────────────────────────────────────────────┘

1. GAME START
   User Launches Game
   ↓
   Splash Screen Shows
   ↓
   Text Pulses (SplashScreenManager)
   
2. USER TAPS SCREEN
   Input Detected (SplashScreenManager)
   ↓
   Fade Out Animation
   ↓
   Call MainMenuManager.ShowMainMenu()
   ↓
   Main Menu Panel Fades In
   
3. USER CLICKS START
   Button Click (MainMenuManager)
   ↓
   Call MenuSceneManager.LoadGameScene()
   ↓
   Show Loading Panel
   ↓
   Async Load Scene
   ↓
   Update Progress Bar
   ↓
   Scene Activates
   ↓
   Fade In Game
   
4. USER CLICKS SETTINGS
   Button Click (MainMenuManager)
   ↓
   Hide Main Menu Panel
   ↓
   Show Settings Panel
   ↓
   Load Saved Settings (PlayerPrefs)
   ↓
   Display Current Values
   
5. USER CHANGES VOLUME
   Slider Changed (SettingsManager)
   ↓
   Update PlayerPrefs
   ↓
   Apply Audio Settings
   ↓
   Update Percentage Text
   
6. USER CLICKS BACK
   Button Click (SettingsManager)
   ↓
   Save Settings (PlayerPrefs.Save())
   ↓
   Call MainMenuManager.ReturnToMainMenu()
   ↓
   Hide Settings Panel
   ↓
   Show Main Menu Panel
   
7. USER OPENS CUSTOMIZATION
   Button Click (MainMenuManager)
   ↓
   Hide Main Menu Panel
   ↓
   Show Customization Panel
   ↓
   Load Unlocked Skins (PlayerPrefs)
   ↓
   Generate Skin Buttons
   ↓
   Display Equipped Skin
   
8. USER SELECTS SKIN
   Skin Button Click (CustomizationManager)
   ↓
   Update Preview Image
   ↓
   Update Name & Description
   ↓
   Check if Unlocked
   ↓
   Update Select Button State
   
9. USER UNLOCKS SKIN
   Select Button Click (CustomizationManager)
   ↓
   Check Coin Balance
   ↓
   Deduct Coins (PlayerPrefs)
   ↓
   Set Skin Unlocked (PlayerPrefs)
   ↓
   Refresh UI
   
10. USER EQUIPS SKIN
    Select Button Click (CustomizationManager)
    ↓
    Set Equipped Skin (PlayerPrefs)
    ↓
    Save (PlayerPrefs.Save())
    ↓
    Update UI (show "EQUIPPED")
```

---

## Key Integration Points

```
┌────────────────────────────────────────────────────────────┐
│  GAMEPLAY ←→ MENU SYSTEM                                   │
└────────────────────────────────────────────────────────────┘

FROM GAMEPLAY TO MENU:
━━━━━━━━━━━━━━━━━━━━━━
// In pause menu or game over:
MenuSceneManager.Instance.LoadMainMenuScene();

FROM GAMEPLAY TO CUSTOMIZATION:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// When player collects coins:
CustomizationManager.AddCoins(10);

FROM MENU TO GAMEPLAY:
━━━━━━━━━━━━━━━━━━━━━━
// User clicks Start button
// MainMenuManager → MenuSceneManager → Loads game scene

GET SETTINGS IN GAMEPLAY:
━━━━━━━━━━━━━━━━━━━━━━━━━
float sens = SettingsManager.GetSensitivity();
bool invertY = SettingsManager.IsYAxisInverted();
bool vibrate = SettingsManager.IsVibrationEnabled();

GET EQUIPPED SKIN:
━━━━━━━━━━━━━━━━━━
string skinName = CustomizationManager.GetEquippedSkinName();
// Load appropriate bird model based on skinName
```

---

## Quick Setup Checklist

- [ ] Create Canvas with EventSystem
- [ ] Build splash panel with background, text, fade image
- [ ] Build main menu panel with 4 buttons
- [ ] Build settings panel with all sections
- [ ] Build customization panel with preview and scroll view
- [ ] Build loading panel with progress bar
- [ ] Create skin button prefab
- [ ] Attach SplashScreenManager script to empty GameObject
- [ ] Attach MainMenuManager script to empty GameObject
- [ ] Attach SettingsManager script to SettingsPanel
- [ ] Attach CustomizationManager script to CustomizationPanel
- [ ] Attach MenuSceneManager script to empty GameObject
- [ ] Assign ALL references in Inspector
- [ ] Add bird skins to CustomizationManager list
- [ ] Add scenes to Build Settings
- [ ] Test splash → menu flow
- [ ] Test all 4 menu buttons
- [ ] Test settings save/load
- [ ] Test customization unlock/equip
- [ ] Test scene loading

---

**Ready to build!** Follow MENU_SETUP_GUIDE.md for detailed instructions.
