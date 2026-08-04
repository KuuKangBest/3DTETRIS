# Blocks 方块系统

## 两套检测机制

| 机制 | 用途 | 实现 |
|------|------|------|
| 整数映射 | 方块间碰撞、幽灵预览计算 | `Board.CanPlace()` → `grid[x,y,z]` O(1) 查表 |
| 物理碰撞 | 玩家站立、技能交互 | `BoxCollider`（CreatePrimitive 自带） |

方块**同时拥有**两种检测，互不依赖。

## 落定流程

```
方块下落（Update → fallTimer → TryFall）
  ├── CanPlace(↓) == true  → 下移 + 更新幽灵
  └── CanPlace(↓) == false → isTouchingGround = true，启动 lockTimer
                                ↓
                         lockTimer < 0.5s → 仍可移动/旋转
                           移动/旋转后若下方有空间 → 重置 isTouchingGround
                                ↓
                         lockTimer ≥ 0.5s → LockInPlace()
                            ├── hasPlaced = true
                            ├── 删除幽灵方块
                            └── board.PlaceBlock() → 注册到 grid 数组
```

## 数据流

```
BlockFactory.SpawnBlock()
  └── Block.Initialize(shape, color, gridPos)
        ├── CreateVisualCubes()     # 实心方块（有 BoxCollider）
        └── CreateGhostPreview()    # 半透明白色（无碰撞）
              └── CalculateLandingPosition()
                    └── board.CanPlace() 逐格检查直到不能降落
```

## 关键状态

| 字段 | 含义 |
|------|------|
| hasPlaced | 已落定并注册到 Board |
| IsActive | `!hasPlaced` — GameManager 以此判断是否还需处理 |
| isTouchingGround | 触底但未过落定延迟 |
| lockTimer | 触底计时 |

## 方块形状库

参见 `BlockShapes.cs` — 11种标准形状 + 整行 + 2×2×2
