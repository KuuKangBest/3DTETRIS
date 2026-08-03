# Core 核心系统

## 棋盘

| 参数 | 值 |
|------|-----|
| 宽度 (X) | 8 |
| 高度 (Y) | 15 |
| 深度 (Z) | 8 |
| 单位 | 1×1×1 |
| 下落 | Y轴从上到下 |

## 方块

真正的 3D 立方体组合（polycube），由 1×1×1 小立方体拼接。

### 标准形状（11种）

| # | 名称 | 体积 | 形状描述 |
|---|------|------|----------|
| 1 | Straight-3 | 3 | 3格直线 |
| 2 | Corner-3 | 3 | L形3格 |
| 3 | Straight-4 | 4 | 4格直线 |
| 4 | Square-2x2 | 4 | 2×2平板 |
| 5 | L-4 | 4 | 长L形 |
| 6 | T-4 | 4 | T字形 |
| 7 | Z-4 | 4 | Z/S形 |
| 8 | 3D-Corner | 4 | 三轴拐角 |
| 9 | 3D-Plus | 5 | 十字立体 |
| 10 | Fat-L | 5 | 胖L形 |
| 11 | Stair | 5 | 台阶形 |

### 特殊方块
- 整行方块 (1×1×8)：5%概率，填满X轴一行
- 2×2×2 大立方体 (8格)：低概率

### 方块操作
- 三轴旋转：绕X/Y/Z轴 90°
- 硬降：瞬间落到底部
- 被推动：玩家技能交互

## 消除规则

三种消除**同时存在**，每个格子有 `EliminationFlags`：

| 类型 | 方向 | 条件 | 奖励 |
|------|------|------|------|
| Row 行消 | X轴 | 同一(Y,Z)全部X填满 | 10分/格 |
| Column 列消 | Z轴 | 同一(X,Y)全部Z填满 | 10分/格 |
| Face 面消 | XZ平面 | 同一Y全部XZ填满 | 10分/格 + 500分 |

### 消除后处理
- 被消除列的上方格子因重力下落填补空隙
- 方块标志：`All`(默认) / `RowEliminable` / `ColumnEliminable` / `FaceEliminable` / `None`

## 数据流

```
GameManager (状态机: Waiting→Playing→Paused→GameOver)
  ├── BlockFactory.SpawnBlock() → Block
  ├── Block 自动下落 (fallTimer)
  ├── Block.LockInPlace() → Board.PlaceBlock()
  ├── EliminationSystem.CheckAllEliminations()
  ├── EliminationSystem.ExecuteEliminations()
  │     └── ApplyGravity() 消除后下落
  └── SpawnNewBlock() 循环
```
