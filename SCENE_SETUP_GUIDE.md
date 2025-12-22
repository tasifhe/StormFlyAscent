# Scene Setup Guide - StormFly Ascent

## Overview
This guide explains how to properly set up the Main Menu scene and Playground scene for the loading system to work correctly.

---

## 🎮 Main Menu Scene Setup

### Required GameObjects:

#### 1. **MenuSceneManager** (Root GameObject)
This is the core loading system controller that persists across scenes.

**Inspector Setup:**
- **Scene Names:**
  - Main Menu Scene Name: `Main Menu` (your menu scene name)
  - Game Scene Name: `PLAYGROUND` (your gameplay scene name)

- **Loading Screen:** (All children of a Canvas)
  - Loading Panel: Drag the GameObject that contains all loading UI
  - Loading Progress Bar: Reference to the Slider component
  - Loading Percentage Text: Reference to TextMeshPro text showing "%"
  - Loading Tip Text: Reference to TextMeshPro text showing tips
  - Fade Image: Reference to a full-screen black Image component

- **Loading Settings:**
  - Fade Duration: `0.5` seconds
  - Keep Loading Panel For Level Gen: ✓ (checked)

- **Menu References:**
  - Main Menu Manager: Drag the GameObject with MainMenuManager component

- **Loading Tips:**
  - Add 5-7 helpful gameplay tips (already configured)

**Hierarchy Structure:**
```
Main Menu Scene
├── MenuSceneManager (Script: MenuSceneManager)
├── Canvas (Main UI Canvas)
│   ├── MainMenuPanel (Your menu buttons - Play, Settings, etc.)
│   ├── SettingsPanel
│   ├── CustomizationPanel
│   └── LoadingPanel (Contains all loading UI)
│       ├── FadeImage (Full-screen black image - should be first child)
│       ├── ProgressBar (Slider component)
│       ├── PercentageText (TextMeshPro)
│       └── TipText (TextMeshPro)
└── MainMenuManager (Script: MainMenuManager)
    └── References to all menu panels
```

**Important Notes:**
- MenuSceneManager GameObject should be at root level (not child of Canvas)
- It uses `DontDestroyOnLoad`, so it persists to Playground scene
- The Canvas with LoadingPanel also persists via DontDestroyOnLoad

---

#### 2. **MainMenuManager** GameObject
Controls menu navigation and panels.

**Inspector Setup:**
- **Menu Panels:**
  - Main Menu Panel: The main panel with Play/Settings/Exit buttons
  - Settings Panel: Settings UI panel
  - Customization Panel: Bird customization panel

- **Menu Buttons:**
  - Start Button: The "Play" or "Start Game" button
  - Settings Button
  - Customization Button
  - Exit Button

- **Settings:**
  - Hide On Start: ✓ (checked) - Important for proper flow

---

#### 3. **Canvas** (Main UI Canvas)
Contains all menu panels and loading panel.

**Setup:**
- Render Mode: Screen Space - Overlay
- Should have CanvasScaler component
- Should have GraphicRaycaster for button clicks

**Children:**
- MainMenuPanel (visible at start)
- SettingsPanel (hidden at start)
- CustomizationPanel (hidden at start)
- LoadingPanel (hidden at start, shown during loading)

---

#### 4. **LoadingPanel** GameObject
Contains all loading UI elements that appear during scene transitions.

**Children Setup:**

**FadeImage:**
- Component: Image (Unity UI)
- Color: Black (0, 0, 0, 255)
- RectTransform: Stretch to fill entire screen
  - Anchors: Min (0, 0), Max (1, 1)
  - Offsets: All zeros
- **IMPORTANT:** Should be the FIRST child in LoadingPanel (renders behind everything)

**ProgressBar:**
- Component: Slider (Unity UI)
- Min Value: 0
- Max Value: 1
- Value: 0
- Fill Rect: Assigned (the colored fill bar)
- Position: Center-bottom of screen

**PercentageText:**
- Component: TextMeshProUGUI
- Text: "0%"
- Font Size: ~40
- Alignment: Center
- Position: Near the progress bar

**TipText:**
- Component: TextMeshProUGUI
- Text: "Loading Tip..."
- Font Size: ~24
- Alignment: Center
- Position: Below progress bar

---

## 🎯 Playground Scene Setup

### Required GameObjects:

#### 1. **LevelGenerator** GameObject
The Dreamteck Forever Runner level generator.

**Setup:**
- Should exist in the scene (not added at runtime)
- Component: LevelGenerator (from Dreamteck Forever)
- Make sure it has segments assigned
- Should be configured and ready to generate

---

#### 2. **GameStartManager** GameObject
Manages bird initialization and waits for level to be ready.

**Inspector Setup:**
- Component: GameStartManager (your script)

**Dependencies:** (Auto-found at runtime)
- Will find BirdPathFollower
- Will find Character component
- Will find LevelGenerator

**What it does:**
1. Freezes the bird (Rigidbody kinematic + constraints)
2. Waits for LevelGenerator.instance.ready to be true
3. Calls Character.InitializeCharacter()
4. Unfreezes bird and calls BirdPathFollower.StartFollow()

---

#### 3. **Eagle** (Bird GameObject)
Your player character.

**Required Components:**
- Rigidbody (will be controlled by GameStartManager)
- Character (your main controller script)
- BirdPathFollower (follows the path)
- State machine components
- Input systems

**Setup Notes:**
- BirdPathFollower should have `follow = false` initially
- Character.Start() checks for GameStartManager
- Will be frozen until level is ready

---

## 🔄 Loading Flow Sequence

### When User Clicks "Start/Play":

**1. Main Menu Scene (0-2 seconds)**
```
User clicks Start button
  ↓
MenuSceneManager.LoadSceneAsync() called
  ↓
HideMainMenuPanels() - Hide all menu UI
  ↓
Show LoadingPanel
  ↓
Fade image becomes solid black
  ↓
Loading UI elements render on top (Progress bar, %, tips)
  ↓
Start async scene load in background
```

**2. Scene Loading (2-3 seconds)**
```
Progress: 0-50% - Loading scene assets
  ↓
Scene loaded to 90%
  ↓
Progress: 50-55% - "Activating..."
  ↓
Activate scene (switch from Main Menu to Playground)
  ↓
Wait 0.5 seconds for scene initialization
```

**3. Playground Scene - Level Generation (3-8 seconds)**
```
Progress: 60% - "Initializing..."
  ↓
Wait for LevelGenerator.instance (timeout 5 sec)
  ↓
Progress: 65% - "Found Level Generator..."
  ↓
Wait for LevelGenerator.ready (max 30 sec)
  ↓
Progress: 65-95% - "Preparing Level..." (smooth progress)
  ↓
GameStartManager unfreezes bird
  ↓
Character.InitializeCharacter() called
  ↓
BirdPathFollower.StartFollow() called
```

**4. Ready to Play (8+ seconds)**
```
Progress: 95% - "Level Ready..."
  ↓
Progress: 100% - "Starting..."
  ↓
Fade from black (0.5 sec)
  ↓
Hide loading panel
  ↓
Hide main menu panels (ensure cleanup)
  ↓
GAMEPLAY STARTS - Bird is flying on path
```

---

## ✅ Checklist - Main Menu Scene

- [ ] MenuSceneManager GameObject exists at root level
- [ ] MenuSceneManager has all references assigned:
  - [ ] Loading Panel
  - [ ] Progress Bar (Slider)
  - [ ] Percentage Text (TextMeshPro)
  - [ ] Tip Text (TextMeshPro)
  - [ ] Fade Image (Image)
  - [ ] Main Menu Manager (optional but recommended)
- [ ] LoadingPanel hierarchy correct (FadeImage is first child)
- [ ] FadeImage fills entire screen (stretch anchors)
- [ ] MainMenuManager has all panel references assigned
- [ ] Start button has onClick listener calling LoadGameScene()
- [ ] Canvas has GraphicRaycaster and EventSystem exists

---

## ✅ Checklist - Playground Scene

- [ ] LevelGenerator exists and is configured
- [ ] GameStartManager exists with script attached
- [ ] Eagle/Bird has all required components:
  - [ ] Rigidbody
  - [ ] Character script
  - [ ] BirdPathFollower script (follow = false initially)
  - [ ] State machine components
- [ ] BirdPathFollower references the Runner path
- [ ] LevelGenerator has segments assigned

---

## 🐛 Common Issues & Fixes

### Issue: Loading screen shows white/nothing visible
**Fix:** 
- Make sure FadeImage is first child in LoadingPanel
- Check FadeImage color is black (0,0,0,255)
- Verify FadeImage RectTransform fills screen

### Issue: Loading too fast, skips to gameplay immediately
**Fix:**
- Minimum 1.5 second load time is enforced
- Check LevelGenerator is taking time to generate
- Debug logs will show timing

### Issue: Main menu panels still visible in Playground
**Fix:**
- Assign Main Menu Manager reference in MenuSceneManager
- Or ensure MainMenuManager GameObject name is correct
- Check console for "Main menu manager disabled" message

### Issue: Bird falls before path is ready
**Fix:**
- GameStartManager should freeze bird (rigidbody kinematic)
- Check GameStartManager exists in Playground scene
- Verify LevelGenerator is marked as ready

### Issue: Black screen forever / stuck loading
**Fix:**
- Check console for error messages
- LevelGenerator might not be ready (check 30 sec timeout)
- Verify LevelGenerator.instance exists
- Make sure segments are assigned to LevelGenerator

### Issue: Progress bar not moving
**Fix:**
- Check Loading Progress Bar reference is assigned
- Verify it's a Slider component with value 0-1
- Check that Fill Rect is assigned on Slider

---

## 🎨 Recommended Loading Panel Design

```
Screen Layout:
┌─────────────────────────────────┐
│                                 │ ← Top third: Empty/Logo space
│        [Your Game Logo]         │
│                                 │
├─────────────────────────────────┤
│                                 │
│     "Loading... 55%"            │ ← Center: Percentage text
│   ▓▓▓▓▓▓▓▓░░░░░░░░░░           │ ← Progress bar
│                                 │
│  "Tip: Use joystick to steer"  │ ← Loading tip
│                                 │
└─────────────────────────────────┘
    ↑ All rendered on solid black background
```

**Colors:**
- Background: Pure black (for fade image)
- Progress bar background: Dark gray
- Progress bar fill: Bright color (yellow/green)
- Text: White or bright color for contrast

---

## 📝 Inspector Assignment Quick Reference

**MenuSceneManager Component:**
```
Scene Names:
  Main Menu Scene Name: Main Menu
  Game Scene Name: PLAYGROUND

Loading Screen:
  Loading Panel: Canvas/LoadingPanel
  Loading Progress Bar: Canvas/LoadingPanel/ProgressBar
  Loading Percentage Text: Canvas/LoadingPanel/PercentageText
  Loading Tip Text: Canvas/LoadingPanel/TipText
  Fade Image: Canvas/LoadingPanel/FadeImage

Loading Settings:
  Fade Duration: 0.5
  Keep Loading Panel For Level Gen: ✓

Menu References:
  Main Menu Manager: MainMenuManager GameObject
```

**MainMenuManager Component:**
```
Menu Panels:
  Main Menu Panel: Canvas/MainMenuPanel
  Settings Panel: Canvas/SettingsPanel
  Customization Panel: Canvas/CustomizationPanel

Menu Buttons:
  Start Button: MainMenuPanel/PlayButton
  Settings Button: MainMenuPanel/SettingsButton
  Customization Button: MainMenuPanel/CustomizeButton
  Exit Button: MainMenuPanel/ExitButton

Settings:
  Hide On Start: ✓
```

---

## 🚀 Testing Checklist

1. **Test in Main Menu:**
   - [ ] Menu buttons work
   - [ ] Settings panel opens/closes
   - [ ] Customization panel opens/closes

2. **Test Loading:**
   - [ ] Click Start/Play button
   - [ ] Loading panel appears with black background
   - [ ] Progress bar visible and animating
   - [ ] Percentage text updating (0% → 100%)
   - [ ] Loading tips showing
   - [ ] Main menu hides immediately

3. **Test in Playground:**
   - [ ] Scene loads fully
   - [ ] Level generates
   - [ ] Bird is frozen initially (not falling)
   - [ ] Loading panel stays visible during generation
   - [ ] Progress continues (60% → 100%)
   - [ ] When done, fade out happens smoothly
   - [ ] Loading panel disappears
   - [ ] Main menu stays hidden
   - [ ] Bird starts flying on path

4. **Test Completion:**
   - [ ] Gameplay is smooth
   - [ ] No menu UI visible during gameplay
   - [ ] Bird follows path correctly
   - [ ] No errors in console

---

## 💡 Pro Tips

1. **Loading Panel Design:**
   - Keep UI elements large and visible
   - Use high contrast colors
   - Test on mobile screen size

2. **Performance:**
   - Component caching in Character.cs reduces lag
   - Async loading prevents freezing
   - DontDestroyOnLoad minimizes reloading

3. **User Experience:**
   - Loading tips keep players engaged
   - Smooth progress bar shows activity
   - Minimum loading time prevents jarring transitions

4. **Debugging:**
   - Check console during loading for detailed logs
   - Each stage prints progress messages
   - Timeout warnings help identify stuck states

---

## 📞 Need Help?

If setup isn't working:
1. Check console for error messages
2. Verify all references are assigned (no "None" in Inspector)
3. Ensure scene names match exactly
4. Test in both Editor and Build
5. Check this guide's Common Issues section

---

**Last Updated:** December 16, 2025
**Version:** 1.0
