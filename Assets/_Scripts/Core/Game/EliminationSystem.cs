using System.Collections.Generic;
using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 消除系统 — 检查并执行各种消除规则
    ///
    /// 三种消除方式：
    /// 1. Row消除:   同一Y和Z，X轴方向完整一行  → 共 Height × Depth 条可能行
    /// 2. Column消除: 同一X和Y，Z轴方向完整一列 → 共 Width × Height 条可能列
    /// 3. Face消除:   同一Y高度的完整XZ平面    → 共 Height 个可能面
    ///
    /// 每个Cell的EliminationFlags决定它参与哪些消除判定
    /// </summary>
    public class EliminationSystem : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Board board;

        /// <summary>
        /// 检查所有可能的消除，返回被消除的格子列表
        /// </summary>
        public List<Vector3Int> CheckAllEliminations()
        {
            var eliminated = new List<Vector3Int>();

            eliminated.AddRange(CheckRowEliminations());
            eliminated.AddRange(CheckColumnEliminations());
            eliminated.AddRange(CheckFaceEliminations());

            return eliminated;
        }

        /// <summary>
        /// 检查X轴方向行消除（同一Y,Z，所有X必须被可RowEliminable的格子填满）
        /// </summary>
        private List<Vector3Int> CheckRowEliminations()
        {
            var eliminated = new List<Vector3Int>();

            for (int y = 0; y < board.Height; y++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    if (IsCompleteRow(y, z))
                    {
                        for (int x = 0; x < board.Width; x++)
                        {
                            eliminated.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }

            return eliminated;
        }

        private bool IsCompleteRow(int y, int z)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var cell = board.GetCell(x, y, z);
                if (!cell.IsOccupied) return false;
                if (!cell.Flags.HasFlag(EliminationFlags.RowEliminable)) return false;
            }
            return true;
        }

        /// <summary>
        /// 检查Z轴方向列消除（同一X,Y，所有Z必须被可ColumnEliminable的格子填满）
        /// </summary>
        private List<Vector3Int> CheckColumnEliminations()
        {
            var eliminated = new List<Vector3Int>();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (IsCompleteColumn(x, y))
                    {
                        for (int z = 0; z < board.Depth; z++)
                        {
                            eliminated.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }

            return eliminated;
        }

        private bool IsCompleteColumn(int x, int y)
        {
            for (int z = 0; z < board.Depth; z++)
            {
                var cell = board.GetCell(x, y, z);
                if (!cell.IsOccupied) return false;
                if (!cell.Flags.HasFlag(EliminationFlags.ColumnEliminable)) return false;
            }
            return true;
        }

        /// <summary>
        /// 检查XZ面消除（同一Y高度，所有X和Z必须被FaceEliminable的格子填满）
        /// </summary>
        private List<Vector3Int> CheckFaceEliminations()
        {
            var eliminated = new List<Vector3Int>();

            for (int y = 0; y < board.Height; y++)
            {
                if (IsCompleteFace(y))
                {
                    for (int x = 0; x < board.Width; x++)
                    {
                        for (int z = 0; z < board.Depth; z++)
                        {
                            eliminated.Add(new Vector3Int(x, y, z));
                        }
                    }
                }
            }

            return eliminated;
        }

        private bool IsCompleteFace(int y)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    var cell = board.GetCell(x, y, z);
                    if (!cell.IsOccupied) return false;
                    if (!cell.Flags.HasFlag(EliminationFlags.FaceEliminable)) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 执行消除并返回消除的格子
        /// </summary>
        public int ExecuteEliminations(List<Vector3Int> cellsToEliminate)
        {
            foreach (var pos in cellsToEliminate)
            {
                board.ClearCell(pos);
            }

            // 消除后需要处理重力 — 上方格子下落
            ApplyGravity(cellsToEliminate);

            return cellsToEliminate.Count;
        }

        /// <summary>
        /// 消除后让上方的格子下落填补空隙
        /// 对每个被消除位置所在的XZ列，上方格子下移
        /// </summary>
        private void ApplyGravity(List<Vector3Int> eliminatedCells)
        {
            // 收集所有受影响的XZ列（去重）
            var affectedColumns = new HashSet<(int x, int z)>();
            foreach (var pos in eliminatedCells)
                affectedColumns.Add((pos.x, pos.z));

            foreach (var (x, z) in affectedColumns)
            {
                // 从底部向上扫描
                int writeY = 0;
                for (int readY = 0; readY < board.Height; readY++)
                {
                    var cell = board.GetCell(x, readY, z);
                    if (cell.IsOccupied)
                    {
                        if (writeY != readY)
                        {
                            // 将格子从readY移动到writeY
                            board.ClearCell(x, readY, z);
                            // 需要重新设置writeY位置的格子
                            // 由于Cell是struct，我们需要Place的方式
                            // 暂时用反射绕过 — 更好的做法是Board提供MoveCell方法
                            MoveCellDown(x, readY, z, writeY);
                        }
                        writeY++;
                    }
                }
            }
        }

        /// <summary>
        /// 将格子从(fromY)移动到(toY)（简易实现）
        /// </summary>
        private void MoveCellDown(int x, int fromY, int z, int toY)
        {
            var cell = board.GetCell(x, fromY, z);
            if (cell.IsOccupied)
            {
                board.ClearCell(x, fromY, z);
                board.PlaceBlock(
                    new Vector3Int(x, toY, z),
                    new Vector3Int[] { Vector3Int.zero },
                    cell.Flags,
                    cell.CellColor
                );
            }
        }
    }
}
