using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 消除类型标志 — 一个方块可以支持多种消除方式
    /// </summary>
    [System.Flags]
    public enum EliminationFlags
    {
        None            = 0,
        RowEliminable   = 1 << 0,   // X轴方向行消除 (同一Y,Z平面上的完整X行)
        ColumnEliminable = 1 << 1,  // Z轴方向列消除 (同一X,Y平面上的完整Z列)
        FaceEliminable  = 1 << 2,   // XZ面消除 (同一Y高度的完整平面)
        All             = RowEliminable | ColumnEliminable | FaceEliminable
    }

    /// <summary>
    /// 棋盘上的单个单元格
    /// </summary>
    public struct Cell
    {
        public bool IsOccupied;
        public EliminationFlags Flags;
        public Color CellColor;             // 方块颜色（用于渲染和消除特效）

        public static Cell Empty => new Cell
        {
            IsOccupied = false,
            Flags = EliminationFlags.None,
            CellColor = Color.clear
        };

        public Cell(EliminationFlags flags, Color color)
        {
            IsOccupied = true;
            Flags = flags;
            CellColor = color;
        }
    }
}
