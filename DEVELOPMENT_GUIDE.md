# 《赛博道士：子午档案》开发指南

## 目录
1. [Unity项目搭建](#unity项目搭建)
2. [场景层级结构](#场景层级结构)
3. [组件配置详解](#组件配置详解)
4. [Python端配置](#python端配置)
5. [测试调试](#测试调试)
6. [常见问题](#常见问题)

---

## Unity项目搭建

### 第一步：创建Unity项目

1. 打开Unity Hub，点击"新建项目"
2. 选择 **2D (URP)** 模板
3. 项目名称：`CyberTaoist`
4. 选择Unity版本：2021.3 LTS或更高

### 第二步：导入脚本

1. 在Project窗口创建以下文件夹结构：
```
Assets/
├── Scripts/
│   ├── Core/
│   ├── Player/
│   ├── Enemy/
│   ├── UI/
│   ├── Effects/
│   └── Communication/
├── Prefabs/
├── Sprites/
├── Audio/
└── Scenes/
```

2. 将对应的C#脚本拖入相应文件夹

### 第三步：安装依赖包

1. 打开 Window > Package Manager
2. 安装以下包：
   - DOTween (可选，用于动画)
   - TextMeshPro (UI文字)

---

## 场景层级结构

在Hierarchy中创建以下结构：

```
Main Scene
│
├── --- MANAGERS ---
│   ├── GameManager        [添加 GameManager.cs]
│   ├── GameStateManager   [添加 GameStateManager.cs]
│   ├── SealSlotManager    [添加 SealSlotManager.cs]
│   ├── SpellSystem        [添加 SpellSystem.cs]
│   ├── EnemySpawner       [添加 EnemySpawner.cs]
│   ├── AudioManager       [添加 AudioManager.cs]
│   └── SpellEffects       [添加 SpellEffects.cs]
│
├── --- CAMERA ---
│   └── Main Camera        [添加 CameraController.cs]
│
├── --- PLAYER ---
│   └── Player             [添加组件见下文]
│       └── GroundCheck    [空物体，用于地面检测]
│       └── AttackPoint    [空物体，用于攻击检测]
│
├── --- ENVIRONMENT ---
│   ├── Ground             [添加 BoxCollider2D, 设置Layer为Ground]
│   └── Walls              [可选]
│
├── --- UI ---
│   └── Canvas
│       ├── PlayerUI
│       │   ├── HealthBar       [Slider]
│       │   └── EnergyBar       [Slider]
│       ├── SealSlotPanel       [添加 SealSlotUI.cs]
│       │   ├── Slot1           [Image]
│       │   ├── Slot2           [Image]
│       │   └── Slot3           [Image]
│       ├── DomainOverlay       [Image, 添加 DomainUI.cs]
│       ├── BossHealthPanel     [初始隐藏]
│       ├── PausePanel          [初始隐藏]
│       ├── VictoryPanel        [初始隐藏]
│       └── DefeatPanel         [初始隐藏]
│
└── --- SPAWN POINTS ---
    ├── SpawnPoint_Left
    ├── SpawnPoint_Right
    ├── SpawnPoint_Top
    └── SpawnPoint_Bottom
```

---

## 组件配置详解

### Player 配置

创建Player GameObject：
1. 创建空物体，重命名为"Player"
2. 添加组件：
   - Sprite Renderer
   - Rigidbody2D (Freeze Rotation Z)
   - BoxCollider2D
   - PlayerController.cs
   - PlayerCombatController.cs
   - HandSignReceiver.cs
3. 设置Tag为"Player"

**PlayerController 设置：**
```
Move Speed: 8
Jump Force: 12
Max Jumps: 2
Ground Check: [拖入GroundCheck子物体]
Ground Layer: Ground
Check Radius: 0.2
```

**PlayerCombatController 设置：**
```
Max Health: 100
Max Energy: 100
Energy Gain Per Hit: 20
Attack Damage: 25
Attack Range: 2
Attack Cooldown: 0.3
Attack Point: [拖入AttackPoint子物体]
Enemy Layer: Enemy
Domain Duration: 10
```

**HandSignReceiver 设置：**
```
Port: 5005
Stability Frames: 5
```

### Enemy Prefabs 配置

**故障者(GlitchEnemy)：**
1. 创建空物体，命名"GlitchEnemy"
2. 添加：Sprite Renderer(红色方块)、Rigidbody2D、BoxCollider2D、GlitchEnemy.cs
3. 设置Tag为"Enemy"，Layer为Enemy
4. 保存为Prefab

**赛博斧手(CyberAxeman)：**
- 同上，使用蓝色方块，添加CyberAxeman.cs
- 尺寸稍大

**重构体(ReconstructorEnemy)：**
- 同上，使用灰色方块，添加ReconstructorEnemy.cs
- 尺寸更大

**Boss(BossController)：**
- 同上，使用紫色大方块，添加BossController.cs
- 最大尺寸

### EnemySpawner 配置

```
Glitch Prefab: [拖入GlitchEnemy Prefab]
Cyber Axeman Prefab: [拖入CyberAxeman Prefab]
Reconstructor Prefab: [拖入ReconstructorEnemy Prefab]
Boss Prefab: [拖入Boss Prefab]
Spawn Points: [拖入4个SpawnPoint物体]
Auto Spawn: true
Glitch Interval: 3
Axeman Interval: 5
Reconstructor Interval: 20
Boss Spawn Delay: 60
```

### SealSlotManager 配置

```
Max Slots: 3
Dragon Sign ID: 5
Ox Sign ID: 2
Rabbit Sign ID: 4
End Seal Sign ID: 13
```

### UI 配置

**SealSlotUI 设置：**
```
Slot Images: [拖入3个Slot Image组件]
Slot Texts: [拖入3个Text组件]
Empty Slot Color: (0.2, 0.2, 0.2, 0.5)
Dragon Color: 红色
Ox Color: 金色 (1, 0.84, 0)
Rabbit Color: 蓝色
```

**DomainUI 设置：**
```
Domain Overlay: [拖入遮罩Image]
Edge Glow: [拖入边缘光晕Image]
Timer Text: [拖入倒计时Text]
Hint Text: [拖入提示Text]
Domain Duration: 10
```

### Camera Controller 配置

```
Target: [拖入Player]
Smooth Speed: 5
Offset: (0, 0, -10)
Default Zoom: 5
Domain Zoom: 4
Spell Zoom: 2.5
```

---

## Python端配置

### 环境安装

```bash
# 创建虚拟环境
python -m venv venv
source venv/bin/activate  # Linux/Mac
venv\Scripts\activate     # Windows

# 安装依赖
pip install opencv-python numpy onnxruntime
```

### 模型配置

1. 下载YOLOX模型文件 `yolox_nano.onnx`
2. 放置到 `model/yolox/` 目录
3. 确保 `setting/labels.csv` 文件存在

### 运行手势识别

```bash
# 使用默认摄像头
python simple_demo.py

# 指定摄像头
python simple_demo.py --device 1

# 调整置信度阈值
python simple_demo.py --score_th 0.8
```

### 标签映射 (labels.csv)

```csv
None,無
Ne(Rat),子
Ushi(Ox),丑
Tora(Tiger),寅
U(Hare),卯
Tatsu(Dragon),辰
Mi(Snake),巳
Uma(Horse),午
Hitsuji(Ram),未
Saru(Monkey),申
Tori(Bird),酉
Inu(Dog),戌
I(Boar),亥
Gassho,祈
Unknown,謎
Mizunoe,壬
```

---

## 测试调试

### 通信测试

1. **启动Unity游戏**
   - 在Unity Editor中点击Play

2. **检查UDP接收**
   - 打开Console窗口
   - 应该看到 "UDP Receiver starting..."

3. **启动Python端**
   ```bash
   python simple_demo.py
   ```

4. **测试手势**
   - 对着摄像头做手势
   - Python窗口显示识别结果
   - Unity Console显示接收到的信号

### 功能测试清单

- [ ] 玩家移动（WASD）
- [ ] 玩家跳跃（空格）
- [ ] 普通攻击（鼠标左键）
- [ ] 咒力累积
- [ ] 领域展开（F键，咒力满时）
- [ ] 手势接收
- [ ] 印记槽填充
- [ ] 术式释放
- [ ] 敌人生成
- [ ] 敌人AI行为
- [ ] Boss战斗
- [ ] UI显示
- [ ] 音效播放

### 调试技巧

1. **没有收到手势信号？**
   - 检查防火墙是否阻止UDP 5005端口
   - 确保Python脚本正在运行
   - 检查HandSignReceiver的port设置

2. **领域展开无法触发？**
   - 确认咒力条已满（100/100）
   - 检查GameState是否为Normal

3. **印记不填充？**
   - 检查stabilityFrames设置
   - 确认手势ID映射正确

---

## 常见问题

### Q: 手势识别延迟太高？
A: 
- 降低摄像头分辨率：`--width 640 --height 360`
- 增加跳帧：`--skip_frame 1`
- 使用GPU加速（如果支持）

### Q: 敌人不生成？
A: 
- 检查EnemySpawner的autoSpawn是否为true
- 确认Prefab已正确关联
- 检查SpawnPoints是否设置

### Q: 游戏状态卡住？
A:
- 检查GameStateManager的状态转换逻辑
- 确保SpellSystem正确触发状态恢复

### Q: UI不显示？
A:
- 检查Canvas设置（Screen Space - Overlay）
- 确认UI元素的RectTransform

---

## 优化建议

### 性能优化
- 使用对象池管理敌人
- 限制同屏敌人数量
- 优化粒子特效

### 游戏体验优化
- 调整stabilityFrames减少误触发
- 增加视觉反馈确认手势
- 添加手势教程

### 扩展功能
- 更多术式组合
- 多关卡设计
- 技能升级系统
- 排行榜

---

*本指南持续更新中，如有问题请提交Issue*
