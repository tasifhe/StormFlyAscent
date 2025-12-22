# StormFly Ascent - Complete Project Documentation

## Table of Contents
1. [Project Overview](#project-overview)
2. [Scene Setup](#scene-setup)
3. [Controls & Input](#controls--input)
4. [Mobile Optimization](#mobile-optimization)
5. [Performance Features](#performance-features)
6. [Known Issues & Solutions](#known-issues--solutions)

---

## Project Overview

**StormFly Ascent** is a Unity endless runner game featuring:
- Bird flight mechanics with state machine
- Dreamteck Forever Runner integration
- Mobile-optimized touch controls
- Dynamic loading system with progress feedback
- DOTween menu animations
- Modular boost system

**Tech Stack:**
- Unity 6.0
- New Input System
- Dreamteck Forever Runner
- DOTween Pro
- TextMeshPro

---

## Scene Setup

### Main Menu Scene

**Required GameObjects:**
1. **MenuSceneManager** - Handles loading and scene transitions
2. **MainMenuManager** - Controls menu panels and navigation
3. **Canvas** - Contains all UI elements
4. **LoadingPanel** - Shows during scene loading

**Quick Setup:**
- Assign all loading UI references in MenuSceneManager
- Ensure FadeImage is first child in LoadingPanel (renders behind)
- Main Menu Manager optional reference for cleanup

### Playground Scene

**Required:**
1. **LevelGenerator** - Dreamteck Forever level generator
2. **GameStartManager** - Freezes bird until level ready
3. **Eagle (Bird)** - Player character with components

**Auto-handles:**
- Bird freezing during load
- Level generation wait
- Character initialization
- Path following activation

---

## Controls & Input

### Desktop
- **Mouse Movement** - Steer bird left/right
- **Left Click / Spacebar** - Boost

### Mobile
- **Touch Joystick** - Steer bird
- **Double Tap** - Boost with haptic feedback
- **Touch Zones** - Left/right screen tap for boost

### Input Settings
- Configurable via `MobileOptimizationSettings` ScriptableObject
- Adjustable sensitivity, cooldowns, and touch zones
- Runtime settings through `BoostInputSettingsManager`

---

## Mobile Optimization

### Performance Modes
- **High Performance** - 60 FPS target, full effects
- **Balanced** - 30 FPS, optimized
- **Battery Saver** - 30 FPS, minimal effects

### Optimizations
- Component caching (reduces ~2ms to ~0.0001ms per state)
- Touch input pooling
- Haptic feedback integration
- Device detection (mobile/tablet/desktop)

### Settings Location
`Assets/Settings/MobileOptimizationSettings.asset`

---

## Performance Features

### Component Caching
**Character.cs** caches all components on Start:
- Rigidbody, Transform, Animator
- All state machine components
- Input detectors and boost systems

**Benefits:**
- Eliminates GetComponent calls during gameplay
- State transitions now sub-millisecond
- Significant frame time reduction

### Loading System
**Features:**
- Async scene loading (0-50% progress)
- Level generation wait (50-95%)
- Smooth progress with easing animations
- Timeout fallbacks (max 8 seconds)
- Auto-hide menu panels

**Flow:**
```
Menu → Loading (0-50%) → Scene Activate (55%) → 
Level Gen (60-95%) → Ready (100%) → Gameplay
```

---

## Known Issues & Solutions

### Issue: Black screen during loading
**Solution:** FadeImage must be first child in LoadingPanel, with stretch anchors

### Issue: Loading stuck at 55%
**Solution:** Now has 8-second timeout, automatically proceeds

### Issue: Bird falling during load
**Solution:** GameStartManager freezes bird with kinematic rigidbody

### Issue: Main menu visible in gameplay
**Solution:** MenuSceneManager auto-detects and hides all menu objects

### Issue: Loading too fast
**Solution:** Minimum 1.5-second load time enforced

---

## File Structure

```
StormFlyAscent/
├── Assets/
│   ├── _Script/
│   │   ├── Player/
│   │   │   └── Core Components/
│   │   │       ├── Character.cs (Main controller)
│   │   │       └── BirdPathFollower.cs
│   │   ├── UI/
│   │   │   ├── MenuSceneManager.cs (Loading system)
│   │   │   └── MainMenuManager.cs (Menu navigation)
│   │   ├── States/ (State machine)
│   │   ├── BoostSystem/ (Modular boost)
│   │   └── GameStartManager.cs (Level wait)
│   ├── Scenes/
│   │   ├── Main Menu.unity
│   │   └── PLAYGROUND.unity
│   └── Settings/
│       └── MobileOptimizationSettings.asset
├── SCENE_SETUP_GUIDE.md (Detailed setup)
└── PROJECT_DOCUMENTATION.md (This file)
```

---

## Quick Start Checklist

**Main Menu:**
- [ ] MenuSceneManager references assigned
- [ ] LoadingPanel structure correct
- [ ] FadeImage as first child
- [ ] Start button calls LoadGameScene()

**Playground:**
- [ ] LevelGenerator configured
- [ ] GameStartManager exists
- [ ] Bird has all components
- [ ] BirdPathFollower.follow = false initially

**Test:**
- [ ] Click Start
- [ ] Loading progress visible (0-100%)
- [ ] Level generates
- [ ] Bird starts flying
- [ ] Menu hidden in gameplay

---

## Performance Tips

1. **Component Caching** - Always cache GetComponent calls
2. **Async Loading** - Use AsyncOperation for scenes
3. **Touch Pooling** - Reuse touch input objects
4. **State Machine** - Cache references in Awake/Start
5. **LOD System** - Use for distant objects
6. **Object Pooling** - For frequently spawned objects

---

## Support

For issues or questions:
1. Check console for error messages
2. Verify all references assigned in Inspector
3. Review SCENE_SETUP_GUIDE.md for detailed setup
4. Check timeout logs (LevelGenerator wait)

---

**Last Updated:** December 16, 2025
**Version:** 1.0
**Unity Version:** 6.0+
