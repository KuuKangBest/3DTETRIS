using UnityEngine;
using UnityEngine.Events;

namespace TDTTetris.Core
{
    /// <summary>
    /// 3D棋盘 — 管理所有格子的占用状态
    /// 核心数据层，与渲染解耦
    /// </summary>
    public class Board : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GameConfig config;

        // 核心3D网格 [X, Y, Z]
        private Cell[,,] grid;

        // 棋盘在世界空间的原点（左下后角）
        public Vector3 Origin => transform.position;

        public int Width  { get; private set; }
        public int Height { get; private set; }
        public int Depth  { get; private set; }
        public float CellSize { get; private set; }

        // 事件
        public UnityEvent<Vector3Int> OnCellFilled;
        public UnityEvent<Vector3Int[], int> OnCellsEliminated; // cells, count

        private void Awake()
        {
            if (config == null)
            {
                config = Resources.Load<GameConfig>("GameConfig");
                if (config != null) Debug.Log("Board: 从 Resources 自动加载 GameConfig");
            }

            if (config == null)
            {
                Debug.LogWarning("Board: GameConfig 未找到，使用默认值。");
                Width  = 8;
                Height = 15;
                Depth  = 8;
                CellSize = 1f;
            }
            else
            {
                Width  = config.BoardWidth;
                Height = config.BoardHeight;
                Depth  = config.BoardDepth;
                CellSize = config.CellSize;
            }

            grid = new Cell[Width, Height, Depth];
        }

        #region 边界检查

        /// <summary>
        /// 检查坐标是否在棋盘范围内
        /// </summary>
        public bool IsInBounds(int x, int y, int z)
        {
            return x >= 0 && x < Width
                && y >= 0 && y < Height
                && z >= 0 && z < Depth;
        }

        public bool IsInBounds(Vector3Int pos) => IsInBounds(pos.x, pos.y, pos.z);

        #endregion

        #region 格子操作

        /// <summary>
        /// 检查指定坐标是否已被占用
        /// </summary>
        public bool IsOccupied(int x, int y, int z)
        {
            if (!IsInBounds(x, y, z)) return true; // 边界外视为已占用
            return grid[x, y, z].IsOccupied;
        }

        public bool IsOccupied(Vector3Int pos) => IsOccupied(pos.x, pos.y, pos.z);

        /// <summary>
        /// 检查一组偏移是否都可以放置（全部在范围内且未占用）
        /// </summary>
        public bool CanPlace(Vector3Int basePos, Vector3Int[] offsets)
        {
            foreach (var offset in offsets)
            {
                var pos = basePos + offset;
                if (!IsInBounds(pos) || IsOccupied(pos))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 将方块放置到棋盘上
        /// </summary>
        public void PlaceBlock(Vector3Int basePos, Vector3Int[] offsets, EliminationFlags flags, Color color)
        {
            Debug.Log($"[Board] PlaceBlock base={basePos}, offsets={offsets.Length}, " +
                      $"grid[base] occupied before={IsOccupied(basePos)}, instance={GetHashCode()}");
            foreach (var offset in offsets)
            {
                var pos = basePos + offset;
                if (!IsInBounds(pos))
                {
                    Debug.LogError($"Board.PlaceBlock: 位置 {pos} 超出棋盘范围！");
                    continue;
                }

                grid[pos.x, pos.y, pos.z] = new Cell(flags, color);
                OnCellFilled?.Invoke(pos);
            }
        }

        /// <summary>
        /// 清除指定格子
        /// </summary>
        public void ClearCell(int x, int y, int z)
        {
            if (IsInBounds(x, y, z))
                grid[x, y, z] = Cell.Empty;
        }

        public void ClearCell(Vector3Int pos) => ClearCell(pos.x, pos.y, pos.z);

        /// <summary>
        /// 清空整个棋盘
        /// </summary>
        public void ClearAll()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    for (int z = 0; z < Depth; z++)
                        grid[x, y, z] = Cell.Empty;
        }

        /// <summary>
        /// 仅清空指定Y范围内的格子（不破坏游戏方块）
        /// </summary>
        public void ClearRegion(int yMin, int yMax)
        {
            for (int x = 0; x < Width; x++)
                for (int y = yMin; y <= yMax; y++)
                    for (int z = 0; z < Depth; z++)
                        if (y >= 0 && y < Height)
                            grid[x, y, z] = Cell.Empty;
        }

        /// <summary>
        /// 获取格子数据（只读）
        /// </summary>
        public Cell GetCell(int x, int y, int z)
        {
            if (!IsInBounds(x, y, z)) return Cell.Empty;
            return grid[x, y, z];
        }

        public Cell GetCell(Vector3Int pos) => GetCell(pos.x, pos.y, pos.z);

        #endregion

        #region 坐标转换

        /// <summary>
        /// 格子坐标 → 世界坐标（格子中心位置）
        /// </summary>
        public Vector3 GridToWorld(int x, int y, int z)
        {
            return Origin + new Vector3(
                x * CellSize + CellSize * 0.5f,
                y * CellSize + CellSize * 0.5f,
                z * CellSize + CellSize * 0.5f
            );
        }

        public Vector3 GridToWorld(Vector3Int pos) => GridToWorld(pos.x, pos.y, pos.z);

        /// <summary>
        /// 世界坐标 → 格子坐标
        /// </summary>
        public Vector3Int WorldToGrid(Vector3 worldPos)
        {
            var local = worldPos - Origin;
            return new Vector3Int(
                Mathf.FloorToInt(local.x / CellSize),
                Mathf.FloorToInt(local.y / CellSize),
                Mathf.FloorToInt(local.z / CellSize)
            );
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取指定Y高度的最高占用XZ面
        /// </summary>
        public int GetHighestOccupiedY(int x, int z)
        {
            for (int y = Height - 1; y >= 0; y--)
            {
                if (IsInBounds(x, y, z) && grid[x, y, z].IsOccupied)
                    return y;
            }
            return -1;
        }

        /// <summary>
        /// 检测是否有方块到达失败高度
        /// </summary>
        public bool HasReachedFailHeight(int failHeight)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    if (failHeight < Height && grid[x, failHeight, z].IsOccupied)
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region 调试

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying && config != null)
            {
                Gizmos.color = Color.green;
                var center = transform.position + new Vector3(
                    config.BoardWidth * config.CellSize * 0.5f,
                    config.BoardHeight * config.CellSize * 0.5f,
                    config.BoardDepth * config.CellSize * 0.5f
                );
                var size = new Vector3(
                    config.BoardWidth * config.CellSize,
                    config.BoardHeight * config.CellSize,
                    config.BoardDepth * config.CellSize
                );
                Gizmos.DrawWireCube(center, size);
            }
        }

        #endregion
    }
}
