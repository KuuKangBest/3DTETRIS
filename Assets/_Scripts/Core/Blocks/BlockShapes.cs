using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 3D方块形状库 — 所有可用的方块形状定义
    /// 每个形状由一系列 Vector3Int 偏移量组成
    /// </summary>
    public static class BlockShapes
    {
        /// <summary>
        /// 所有标准形状（3-5个立方体组成的3D polycube）
        /// </summary>
        public static readonly Vector3Int[][] StandardShapes = new Vector3Int[][]
        {
            // ===== 3立方体 (Tricubes) =====
            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(2,0,0) },
            // Straight-3: ■■■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(0,0,1) },
            // Corner-3: ■■
            //           ■

            // ===== 4立方体 (Tetracubes) =====
            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(2,0,0), new Vector3Int(3,0,0) },
            // Straight-4: ■■■■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(0,0,1), new Vector3Int(1,0,1) },
            // Square-2x2: ■■
            //             ■■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(2,0,0), new Vector3Int(0,0,1) },
            // L-4: ■■■
            //      ■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(2,0,0), new Vector3Int(1,0,1) },
            // T-4: ■■■
            //       ■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(1,0,1), new Vector3Int(2,0,1) },
            // Z-4(Skew): ■■
            //              ■■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(0,1,0), new Vector3Int(0,0,1) },
            // 3D-Corner: 三个方向的臂

            // ===== 5立方体 (Pentacubes) =====
            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(-1,0,0), new Vector3Int(0,1,0), new Vector3Int(0,0,1) },
            // 3D-Plus: 十字形（3轴各伸出一格）

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(2,0,0), new Vector3Int(0,0,1), new Vector3Int(1,0,1) },
            // Fat-L: ■■■
            //        ■■

            new[] { new Vector3Int(0,0,0), new Vector3Int(1,0,0), new Vector3Int(0,0,1), new Vector3Int(1,0,1), new Vector3Int(0,1,0) },
            // Stair: 2×2平台 + 1个在上面
        };

        /// <summary>
        /// 特殊形状：填满一整行（X方向8个）
        /// </summary>
        public static Vector3Int[] FullRowShape()
        {
            var cells = new Vector3Int[8];
            for (int x = 0; x < 8; x++)
                cells[x] = new Vector3Int(x, 0, 0);
            return cells;
        }

        /// <summary>
        /// 特殊形状：2×2×2 立方体
        /// </summary>
        public static readonly Vector3Int[] Cube2x2x2 = new Vector3Int[]
        {
            new(0,0,0), new(1,0,0), new(0,1,0), new(1,1,0),
            new(0,0,1), new(1,0,1), new(0,1,1), new(1,1,1),
        };

        /// <summary>
        /// 应用3D旋转（绕指定轴旋转90度的整数倍）
        /// </summary>
        public static Vector3Int[] RotateShape(Vector3Int[] shape, int rotationX, int rotationY, int rotationZ)
        {
            var rotated = new Vector3Int[shape.Length];
            for (int i = 0; i < shape.Length; i++)
            {
                var v = shape[i];

                // 绕X轴旋转
                for (int r = 0; r < (rotationX % 4 + 4) % 4; r++)
                    v = new Vector3Int(v.x, -v.z, v.y);

                // 绕Y轴旋转
                for (int r = 0; r < (rotationY % 4 + 4) % 4; r++)
                    v = new Vector3Int(v.z, v.y, -v.x);

                // 绕Z轴旋转
                for (int r = 0; r < (rotationZ % 4 + 4) % 4; r++)
                    v = new Vector3Int(-v.y, v.x, v.z);

                rotated[i] = v;
            }
            return rotated;
        }
    }
}
