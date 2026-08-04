# Core 核心系统

## 棋盘

| 参数 | 值 | 说明 |
|------|-----|------|
| 宽度 (X) | 8 | 格点数 |
| 高度 (Y) | 15 | 格点数 |
| 深度 (Z) | 8 | 格点数 |
| CellSize | 1.6 | 每格世界单位，通过 GameConfig 调整 |
| 棋盘世界尺寸 | 12.8 × 24 × 12.8 | Width × Height × Depth × CellSize |
| 下落方向 | Y轴从上到下 | |

## 方块

真正的 3D 立方体组合，由 1×1×1 小立方体拼接。所有方块通过 Board 注册，保证不重叠。

### 方块放置规则
- 输入：整数格点坐标 (gx, gy, gz)
- 映射：`GridToWorld(gx,gy,gz)` → 世界坐标 `(gx*cs+cs/2, gy*cs+cs/2, gz*cs+cs/2)`
- 缩放：`CellSize * blockSizeRatio`（默认 ratio=0.95）
- 碰撞：每个方块自带 BoxCollider
- 出生位置：随机 XZ，避免重复路径
- 落定延迟：0.5s（触底后可微调，参考经典俄罗斯方块）
- 幽灵预览：半透明白色方块标示最终落地位置，无碰撞

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

## 消除规则

三种消除同时存在，每个格子有 `EliminationFlags`：

| 类型 | 方向 | 条件 | 奖励 |
|------|------|------|------|
| Row 行消 | X轴 | 同一(Y,Z)全部X填满 | 10分/格 |
| Column 列消 | Z轴 | 同一(X,Y)全部Z填满 | 10分/格 |
| Face 面消 | XZ平面 | 同一Y全部XZ填满 | 10分/格+500 |

## 边界系统

| 脚本 | 功能 |
|------|------|
| BoardBoundary | 5面隐墙（底面+4侧面），顶部开放。自动适配 Board 尺寸 |
| BoardFloor | 自动生成与棋盘等大的半透明地板 |

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

## GameConfig 可配置参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| BoardWidth | 8 | X轴格子数 |
| BoardHeight | 15 | Y轴格子数 |
| BoardDepth | 8 | Z轴格子数 |
| CellSize | 1.6 | 每格世界单位 |
| BaseFallInterval | 2 | 基础掉落间隔(秒) |
| FullRowChance | 0.05 | 整行方块概率 |
| FailHeight | 14 | 失败高度 |
| PlayerMoveSpeed | 4 | 玩家移动速度 |
| PlayerJumpForce | 8 | 跳跃力度 |
| PlayerGravity | 15 | 玩家重力 |
