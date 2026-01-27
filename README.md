# Cyber Taoist: The Zi-Wu Files (赛博道士：子午档案)

A 2D action game demo featuring real-time hand gesture recognition powered by MediaPipe.

## 🎮 Project Overview

**Core Feature:** Hand gesture-controlled spell casting
**Platform:** Windows PC (Keyboard/Mouse + Webcam)
**Technology:**
- ✅ MediaPipe hand tracking with <200ms latency
- ✅ Three gesture system: Rock, Thumbs Up, Fist
- ✅ Left hand index finger for player movement
- ✅ Right hand gestures for spell casting
- ✅ Python(CV) ←UDP→ Unity communication

## 📁 Project Structure

```
CyberTaoist/
├── C# Scripts (Unity)
│   ├── Core Systems/
│   │   ├── GameManager.cs          # Global game management
│   │   ├── GameStateManager.cs     # Game state machine
│   │   ├── SealSlotManager.cs      # Gesture processing system
│   │   └── SpellSystem.cs          # Spell system (3 spells)
│   │
│   ├── Player/
│   │   ├── PlayerController.cs     # Player movement (hand tracking)
│   │   ├── PlayerCombatController.cs # Combat system
│   │   └── HandSignReceiver.cs     # UDP gesture receiver
│   │
│   ├── Enemy/
│   │   ├── EnemyBase.cs            # Enemy base class
│   │   ├── GlitchEnemy.cs          # Glitch enemy
│   │   ├── CyberAxeman.cs          # Cyber Axeman
│   │   ├── ReconstructorEnemy.cs   # Reconstructor
│   │   ├── BossController.cs       # Boss controller
│   │   └── EnemySpawner.cs         # Enemy spawning
│   │
│   └── Effects/
│       ├── SpellEffects.cs         # Spell effects
│       ├── Projectile.cs           # Projectiles
│       └── SkillManager.cs         # Skill visual feedback
│
├── Python Scripts (CV)
│   ├── mediapipe_demo.py           # MediaPipe hand detection (NEW)
│   ├── simple_demo.py              # Legacy YOLO demo
│   └── upd_streamer.py             # UDP utility
│
└── Data/
    └── labels.csv                  # Gesture labels
```

## 🎯 Control System

### Player Movement
- **Primary:** Left hand index finger position controls horizontal movement
- **Fallback:** Keyboard arrow keys or WASD
- Movement is limited to 2D horizontal plane (no jumping)

### Spell Casting (Right Hand)
| Gesture | Spell | Effect | Cooldown |
|---------|-------|--------|----------|
| ✊ Rock | Defense | Brief invincibility | 10s |
| 👍 Thumbs Up | Fireball | Ranged projectile attack | 10s |
| 👊 Fist | Power Strike | Powerful melee attack | 10s |

## 🛠️ Setup Guide

### Python Environment

1. **Install dependencies:**
```bash
pip install mediapipe opencv-python numpy
```

2. **Run hand detection:**
```bash
python mediapipe_demo.py --device 0
```

### Unity Project

1. **Create new Unity 2D project**
2. **Import all C# scripts to Assets**
3. **Scene setup:**

```
Scene Hierarchy:
├── Main Camera
├── Managers (Empty GameObject)
│   ├── GameManager
│   ├── GameStateManager
│   ├── SealSlotManager
│   └── SpellSystem
├── Player
│   ├── PlayerController
│   ├── PlayerCombatController
│   └── HandSignReceiver
├── Canvas (UI)
│   ├── HealthBar
│   └── CooldownIndicators
└── Ground
```

4. **Configure references between components**

### Testing

1. Start Unity game
2. Start Python MediaPipe demo
3. Use left index finger to move player
4. Use right hand gestures to cast spells

## ⚙️ Game Parameters

### Player
- Health: 100
- Movement Speed: 8
- Attack Damage: 25
- Spell Cooldown: 10 seconds

### Spells
| Spell | Damage | Duration |
|-------|--------|----------|
| Defense | - | 3 seconds |
| Fireball | 50 | Instant |
| Power Strike | 75 | Instant |

## 📝 Communication Protocol

### UDP Format (JSON)
```json
{
    "left_index_x": 0.5,    // 0-1, left index finger X position
    "left_index_y": 0.5,    // 0-1, left index finger Y position
    "gesture_id": 1,        // 0=None, 1=Rock, 2=ThumbsUp, 3=Fist
    "gesture_name": "Rock",
    "timestamp": 1234567890.123
}
```

### Gesture IDs
- 0: No gesture
- 1: Rock (Defense)
- 2: Thumbs Up (Fireball)
- 3: Fist (Normal Attack)

## 🎥 Demo Features

- Real-time hand landmark visualization
- Left index finger tracking for movement
- Right hand gesture recognition
- Visual feedback for detected gestures
- FPS and latency display

## 📄 License

This project is for learning and portfolio purposes.

---

**Version:** v3.0 (MediaPipe Update)
**Last Updated:** 2024
