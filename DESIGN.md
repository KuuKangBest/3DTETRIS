# 🧊 3D TETRIS — 设计文档

> Unity 2022.3 | 第三人称 | 可爱风 | 3D俄罗斯方块

## 模块文档

| 模块 | 文档 | 说明 |
|------|------|------|
| 核心系统 | [Core/](Assets/_Scripts/Core/README.md) | 棋盘、方块、消除、游戏循环 |
| 玩家系统 | [Player/](Assets/_Scripts/Player/README.md) | 移动、视角、交互 |
| 技能系统 | [Skills/](Assets/_Scripts/Skills/README.md) | 技能接口、已实现/规划技能 |
| 美术设计 | [Art/](Assets/_Art/README.md) | 配色、风格、角色设计 |

## 游戏模式

| 模式 | 状态 |
|------|------|
| 经典模式（到顶=失败）| 🏗️ 开发中 |
| 限时得分 | 📋 规划 |
| 实时PvP对战 | 📋 规划 |

## 代码架构

```
Assets/_Scripts/
├── Core/           # 核心逻辑
├── Player/         # 玩家控制
├── Skills/         # 技能系统
└── UI/             # 界面
```

详见各模块 README。
