# 🎨 FPS COUNTER UI SETUP GUIDE

## Quick Setup (5 Minutes)

### **Step 1: Create the Canvas**

1. **In Unity Hierarchy:**
   - Right-click in Hierarchy
   - Select: **UI → Canvas**
   - A Canvas will be created (if you don't have one already)

2. **Canvas Settings (Optional):**
   - Select Canvas in Hierarchy
   - In Inspector, check:
     - Render Mode: **Screen Space - Overlay** ✓
     - This makes UI appear on top of game

---

### **Step 2: Create the FPS Text**

1. **Create Text Element:**
   - Right-click on **Canvas** in Hierarchy
   - Select: **UI → Text - TextMeshPro**
   - If prompted "Import TMP Essentials", click **Import TMP Essentials**

2. **Rename the Text:**
   - Select the new text object
   - Press F2 or right-click → Rename
   - Name it: **"FPS_Display"**

---

### **Step 3: Position the FPS Text**

#### **Method A: Using Anchor Presets (Recommended)**

1. **Select FPS_Display** in Hierarchy

2. **Open Rect Transform:**
   - In Inspector, find **Rect Transform** component
   - Click the **Anchor Preset** square (top-left of Rect Transform)

3. **Choose Position:**
   
   **For Top-Right Corner (Recommended):**
   - Hold **Alt + Shift**
   - Click **Top-Right** preset (top-right square)
   - This anchors AND positions the text

   **For Top-Left Corner:**
   - Hold **Alt + Shift**
   - Click **Top-Left** preset

   **For Bottom-Right Corner:**
   - Hold **Alt + Shift**
   - Click **Bottom-Right** preset

4. **Adjust Position:**
   - With FPS_Display still selected
   - In Rect Transform, adjust **Pos X** and **Pos Y**:
   
   ```
   Top-Right:    Pos X: -10,  Pos Y: -10
   Top-Left:     Pos X: 10,   Pos Y: -10
   Bottom-Right: Pos X: -10,  Pos Y: 10
   Bottom-Left:  Pos X: 10,   Pos Y: 10
   ```

#### **Method B: Manual Positioning**

1. Select **FPS_Display**
2. Use the **Rect Tool** (T key) in Scene view
3. Drag it to where you want it
4. Adjust size by dragging corners

---

### **Step 4: Style the Text (Optional)**

1. **Select FPS_Display** in Hierarchy

2. **In Inspector, find TextMeshProUGUI component:**

   **Font Settings:**
   - Font Size: **18** (or larger for easier reading)
   - Color: **White** (will change dynamically based on FPS)
   - Alignment: 
     - If top-right: **Right & Top**
     - If top-left: **Left & Top**

   **Visual Settings (Optional but Recommended):**
   - Enable **Outline** (adds border):
     - Material Preset → **Outline**
     - Outline Color: Black
     - Thickness: 0.2
   
   - Or Enable **Shadow/Glow**:
     - Scroll down to Material
     - Add **Underlay** effect for shadow

3. **Adjust Text Box Size:**
   - Width: **250**
   - Height: **150**

---

### **Step 5: Connect to FPSCounter Script**

#### **Option A: Using GameInitializer (Automatic)**

If you're using GameInitializer (recommended):

1. **Find GameManager** in Hierarchy (or create it)
2. Select it and add **GameInitializer** component
3. The FPSCounter will be auto-created
4. **After Play mode starts:**
   - Select the auto-created "FPSCounter" GameObject
   - **Stop Play Mode**
   - Drag your **FPS_Display** text into the **"FPS Text"** field
   - **Save the scene**

#### **Option B: Manual Setup**

1. **Create GameObject:**
   - Right-click in Hierarchy
   - Create Empty
   - Name it: **"PerformanceManager"**

2. **Add FPSCounter Script:**
   - Select PerformanceManager
   - Click **Add Component**
   - Search: **FPSCounter**
   - Click to add

3. **Assign Your Text:**
   - With PerformanceManager selected
   - In Inspector, find **FPSCounter** component
   - Drag **FPS_Display** from Hierarchy into the **"FPS Text"** field

4. **Configure Settings:**
   ```
   Display Settings:
   ├─ FPS Text: [Your FPS_Display] ✓
   ├─ Show FPS: ✓
   └─ Always Show In Builds: ✓
   
   Auto-Create Settings:
   └─ Auto Create If Missing: ✗ (uncheck since you assigned text)
   ```

---

### **Step 6: Test It!**

1. **Press Play** ▶️
2. You should see FPS counter updating in your chosen position
3. Press **F1** to toggle on/off
4. Text color changes:
   - 🟢 Green = 60+ FPS
   - 🟡 Yellow = 30-60 FPS
   - 🔴 Red = <30 FPS

---

## 🎨 STYLING EXAMPLES

### **Example 1: Minimal (Game HUD Style)**
```
Font Size: 16
Color: White
Alignment: Top-Right
Outline: None
Background: Transparent
```

### **Example 2: High Visibility (Debug Style)**
```
Font Size: 20
Color: Cyan
Alignment: Top-Left
Outline: Black (0.3 thickness)
Background: Semi-transparent black panel
```

### **Example 3: Match Existing UI**
```
Font: Same as your game's UI font
Font Size: Same as health/score display
Color: Match your UI color scheme
Position: Next to other UI elements
```

---

## 📐 COMMON POSITIONS

### **Position Reference:**

```
Top-Left          Top-Center          Top-Right
(10, -10)         (0, -10)            (-10, -10)
    ↓                 ↓                     ↓
┌─────────────────────────────────────────────┐
│ FPS: 60                                     │
│                                       FPS: 60│
│                                             │
│                                             │
│               FPS: 60                       │
│                                             │
│                                             │
│ FPS: 60                               FPS: 60│
└─────────────────────────────────────────────┘
    ↑                 ↑                     ↑
Bottom-Left     Bottom-Center       Bottom-Right
(10, 10)          (0, 10)             (-10, 10)
```

---

## 🔧 TROUBLESHOOTING

### **Problem: Text not visible**
✅ Check Canvas render mode is "Screen Space - Overlay"
✅ Check FPS_Display is child of Canvas
✅ Check text color is not same as background
✅ Check "Show FPS" is enabled in FPSCounter

### **Problem: Text is tiny**
✅ Increase Font Size to 18-24
✅ Check Canvas Scaler settings
✅ Adjust text Width/Height (250x150 recommended)

### **Problem: Text is cut off**
✅ Increase Width/Height in Rect Transform
✅ Enable "Auto Size" in TextMeshPro
✅ Check anchors are set correctly

### **Problem: FPS not updating**
✅ Make sure you assigned the text to FPSCounter script
✅ Check "Update Interval" is not too high
✅ Press Play and check Console for errors

---

## ⚡ PRO TIPS

1. **Add Background Panel (Optional):**
   - Right-click Canvas → UI → Panel
   - Resize to fit behind text
   - Set color to semi-transparent black
   - Makes text easier to read

2. **Make it Draggable (Optional):**
   - Add "Drag" script to make it moveable at runtime
   - Useful for testing different positions

3. **Persistent Across Scenes:**
   - FPSCounter has `DontDestroyOnLoad()`
   - Your text should also be on a DontDestroyOnLoad Canvas
   - Or put text in every scene

4. **Mobile Optimization:**
   - Keep font size 16-20 on mobile
   - Use high contrast colors
   - Don't make it too large (screen real estate)

---

## 📱 EXAMPLE COMPLETE SETUP

**Hierarchy:**
```
Canvas
├── FPS_Display (TextMeshProUGUI)
└── [Your other UI elements]

GameManager (GameObject)
├── GameInitializer (Script)
└── PerformanceManager (GameObject)
    └── FPSCounter (Script) ← FPS_Display assigned here
```

**Result:**
- FPS counter in top-right corner
- Updates every 0.5 seconds
- Color-coded performance
- Shows FPS, frame time, memory
- Toggle with F1 key

---

## ✅ CHECKLIST

Before building:
- [ ] Canvas exists in scene
- [ ] FPS_Display TextMeshProUGUI created
- [ ] FPS_Display positioned where you want
- [ ] FPS_Display assigned to FPSCounter script
- [ ] "Show FPS" enabled in FPSCounter
- [ ] Tested in Play mode
- [ ] FPS counter visible and updating

**You're done!** 🎉

---

**Time Required:** 5 minutes
**Difficulty:** Easy
**Result:** Professional FPS counter exactly where you want it!
