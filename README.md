# 赛博道士：子午档案 (Cyber Taoist: The Zi-Wu Files)

一款融合计算机视觉手势识别的2D横版动作游戏Demo，玩家扮演赛博道士，通过真实手势结印释放术式。

## 🎮 项目概述

**核心卖点：** 高频次手势识别驱动的策略战斗
**目标平台：** Windows PC (键鼠+摄像头)
**技术特点：**
- ✅ 实时手势识别延迟<200ms
- ✅ 印记槽组合系统：3个印记→9种术式
- ✅ 慢动作结印模式：保证识别准确率的同时维持战斗流畅度
- ✅ 跨语言通信：Python(CV) ←UDP→ Unity(游戏逻辑)

## 📁 项目结构

```
CyberTaoist/
├── C# Scripts (Unity)
│   ├── Core Systems/
│   │   ├── GameManager.cs          # 游戏全局管理
│   │   ├── GameStateManager.cs     # 游戏状态机
│   │   ├── SealSlotManager.cs      # 印记槽系统
│   │   └── SpellSystem.cs          # 术式组合系统
│   │
│   ├── Player/
│   │   ├── PlayerController.cs     # 玩家移动控制
│   │   ├── PlayerCombat.cs         # 玩家战斗（旧版）
│   │   └── PlayerCombatController.cs # 玩家战斗系统
│   │
│   ├── Enemy/
│   │   ├── EnemyBase.cs            # 敌人基类
│   │   ├── GlitchEnemy.cs          # 故障者（小型近战）
│   │   ├── CyberAxeman.cs          # 赛博斧手（中型近战）
│   │   ├── ReconstructorEnemy.cs   # 重构体（大型坦克）
│   │   ├── BossController.cs       # Boss控制器
│   │   └── EnemySpawner.cs         # 敌人生成系统
│   │
│   ├── UI/
│   │   ├── GameUI.cs               # 主游戏UI
│   │   ├── SealSlotUI.cs           # 印记槽UI
│   │   └── DomainUI.cs             # 领域展开UI
│   │
│   ├── Effects/
│   │   ├── SpellEffects.cs         # 术式特效
│   │   ├── CameraController.cs     # 相机控制（震动/缩放）
│   │   └── Projectile.cs           # 投射物
│   │
│   ├── Audio/
│   │   └── AudioManager.cs         # 音频管理
│   │
│   └── Communication/
│       ├── HandSignReceiver.cs     # UDP手势接收器
│       └── SkillManager.cs         # 技能触发管理（旧版）
│
├── Python Scripts (CV)
│   ├── simple_demo.py              # 手势识别主程序
│   └── upd_streamer.py             # UDP发送工具类
│
└── Data/
    └── labels.csv                  # 手势标签映射
```

## 🎯 核心玩法

### 战斗循环
1. **常规战斗** - WASD移动 + 鼠标左键攻击敌人
2. **能量积累** - 击中敌人获得咒力（能量条上涨）
3. **领域展开** - 咒力满时按F键进入慢动作结印模式
4. **结印操作** - 对摄像头做手势，填充3个印记槽
5. **术式释放** - 根据印记组合释放对应术式

### 印记类型
| 印记 | 汉字 | 属性 | 颜色 | 手势ID |
|-----|------|------|------|--------|
| 🐉 龙印 | 辰 | 攻击 | 红色 | 5 |
| 🐂 牛印 | 丑 | 防御 | 金色 | 2 |
| 🐰 兔印 | 卯 | 机动 | 蓝色 | 4 |
| 🙏 祈印 | 祈 | 结束 | - | 13 |

### 术式组合
| 组合 | 术式名 | 效果 | CD |
|------|--------|------|-----|
| 龙龙龙 | 离火聚龙 | 火龙特效，200伤害 | 5秒 |
| 牛牛牛 | 金刚不坏身 | 5秒无敌+反伤 | 5秒 |
| 兔兔兔 | 神行千里 | 移速x3，持续10秒 | 10秒 |
| 龙牛兔 | 三元归一 | 回血50% | 5秒 |

## 🛠️ 开发指南

### 环境要求
- Unity 2021 LTS 或更高版本
- Python 3.9+
- NARUTO-HandSign 模型（YOLOX）
- 摄像头

### Unity项目设置

1. **创建新Unity 2D项目**
2. **导入所有C#脚本到Assets目录**
3. **设置场景结构：**

```
Scene Hierarchy:
├── Main Camera
│   └── CameraController.cs
├── Managers (Empty GameObject)
│   ├── GameManager.cs
│   ├── GameStateManager.cs
│   ├── SealSlotManager.cs
│   ├── SpellSystem.cs
│   ├── EnemySpawner.cs
│   ├── AudioManager.cs
│   └── SpellEffects.cs
├── Player
│   ├── PlayerController.cs
│   ├── PlayerCombatController.cs
│   └── HandSignReceiver.cs
├── Canvas (UI)
│   ├── HealthBar
│   ├── EnergyBar
│   ├── SealSlots (3个槽位)
│   ├── DomainOverlay
│   ├── BossHealthBar
│   └── Panels (Pause/Victory/Defeat)
└── Ground
```

4. **设置Layer和Tag：**
   - Tag: Player, Enemy, Wall, Ground
   - Layer: Ground, Enemy

5. **配置组件引用：**
   - 将各Manager脚本的引用相互关联
   - 配置UI元素到对应的UI脚本

### Python端设置

1. **安装依赖：**
```bash
pip install opencv-python numpy onnxruntime
```

2. **下载YOLOX模型：**
   - 将训练好的模型放到 `model/yolox/yolox_nano.onnx`

3. **运行手势识别：**
```bash
python simple_demo.py --device 0
```

### 通信测试

1. 先启动Unity游戏
2. 再启动Python手势识别脚本
3. 确保UDP端口5005未被占用
4. 在Unity Console查看接收到的手势信息

## 🎨 美术资源

### 占位符规格
- 玩家：白色圆角矩形
- 故障者：红色方块 (小)
- 赛博斧手：蓝色方块 (中)
- 重构体：灰色方块 (大)
- Boss：紫色方块 (最大)

### 特效
- 离火聚龙：Unity ParticleSystem 火焰效果
- 金刚不坏身：LineRenderer 金色圆环
- 神行千里：TrailRenderer 蓝色残影

## 🔊 音效需求

1. 结印成功：咔嚓声
2. 术式释放：轰鸣声
3. 敌人受击：打击声
4. 玩家受伤：受伤声
5. 背景音乐：赛博朋克风格Loop

## ⚙️ 游戏参数

### 玩家
- 生命值：100
- 移动速度：8
- 跳跃力度：12
- 攻击伤害：25
- 咒力获取：20/次

### 敌人
| 类型 | HP | 伤害 | 移动速度 | 生成间隔 |
|-----|-----|------|---------|---------|
| 故障者 | 100 | 20 | 4 | 3秒 |
| 赛博斧手 | 250 | 30 | 3 | 5秒 |
| 重构体 | 350 | 50 | 1.5 | 20秒 |
| Boss | 1000 | 30x3 | 2 | 60秒后 |

### 领域展开
- 持续时间：10秒
- 时间缩放：0.5x
- 结印触发：咒力满 + F键

## 📝 开发清单

### Week 1: 核心技术验证
- [x] Python手势识别
- [x] UDP通信
- [x] Unity角色移动/攻击
- [x] 印记槽系统
- [x] 术式组合系统

### Week 2: 游戏完整性
- [x] 3种敌人AI
- [x] Boss战系统
- [x] 敌人生成系统
- [x] UI系统
- [x] 特效系统
- [x] 音效系统
- [ ] 美术贴图
- [ ] 测试优化

## 🎥 作品集展示

成功标准：
- [ ] 手势识别延迟<300ms
- [ ] 一场战斗至少10次结印
- [ ] Python-Unity通信稳定
- [ ] 战斗循环完整
- [ ] 至少3种术式可用
- [ ] Boss战需要策略

## 📄 许可证

本项目仅用于学习和作品集展示目的。

## 🤝 贡献

欢迎提交Issue和Pull Request！

---

**开发者：** CyberTaoist Team
**版本：** v2.0
**最后更新：** 2024
