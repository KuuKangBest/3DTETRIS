using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 游戏配置 — ScriptableObject，所有可调参数
    /// 可在 Unity Editor 中直接修改，无需改代码
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "3D Tetris/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("棋盘规格")]
        [Tooltip("X轴方向格子数")]
        public int BoardWidth = 8;

        [Tooltip("Y轴方向格子数（高度 / 掉落方向）")]
        public int BoardHeight = 15;

        [Tooltip("Z轴方向格子数")]
        public int BoardDepth = 8;

        [Tooltip("每个格子的世界单位大小")]
        public float CellSize = 1f;

        [Header("方块掉落")]
        [Tooltip("基础掉落间隔（秒）")]
        public float BaseFallInterval = 2f;

        [Tooltip("方块初始下落Y坐标")]
        public int SpawnHeight = 14;

        [Tooltip("方块初始XZ范围（棋盘上方预留空间）")]
        public int SpawnXMin = 0;
        public int SpawnXMax = 7;
        public int SpawnZMin = 0;
        public int SpawnZMax = 7;

        [Header("特殊方块概率")]
        [Tooltip("出现整行方块的概率 (0-1)")]
        [Range(0f, 1f)]
        public float FullRowChance = 0.05f;

        [Header("游戏模式")]
        [Tooltip("默认失败高度（方块堆到该Y值则失败）")]
        public int FailHeight = 14;

        [Header("玩家")]
        [Tooltip("玩家移动速度")]
        public float PlayerMoveSpeed = 5f;

        [Tooltip("玩家跳跃高度")]
        public float PlayerJumpForce = 8f;

        [Tooltip("玩家重力")]
        public float PlayerGravity = 15f;
    }
}
