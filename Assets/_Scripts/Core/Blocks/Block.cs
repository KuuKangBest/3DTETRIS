using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 游戏中的活动方块
    /// 负责方块的物理下落、旋转和玩家交互
    /// </summary>
    public class Block : MonoBehaviour
    {
        [Header("方块属性")]
        [SerializeField] private Vector3Int[] shapeOffsets;  // 形状偏移
        [SerializeField] private EliminationFlags flags = EliminationFlags.All;
        [SerializeField] private Color blockColor = Color.white;

        [Header("状态")]
        [SerializeField] private Vector3Int gridPosition;    // 当前棋盘坐标
        [SerializeField] private float fallTimer;

        // 旋转状态（0-3表示90度的倍数）
        private int rotationX, rotationY, rotationZ;

        // 引用
        private Board board;
        private GameConfig config;

        // 属性
        public Vector3Int[] ShapeOffsets => shapeOffsets;
        public Vector3Int GridPosition => gridPosition;
        public EliminationFlags Flags => flags;
        public Color BlockColor => blockColor;
        public bool IsActive { get; private set; } = true;

        /// <summary>
        /// 初始化方块
        /// </summary>
        public void Initialize(Board boardRef, GameConfig gameConfig,
            Vector3Int[] offsets, EliminationFlags eliminationFlags, Color color,
            Vector3Int startPos)
        {
            board = boardRef;
            config = gameConfig;
            shapeOffsets = offsets;
            flags = eliminationFlags;
            blockColor = color;
            gridPosition = startPos;
            rotationX = rotationY = rotationZ = 0;
            fallTimer = 0f;
            IsActive = true;

            UpdateVisualPosition();
        }

        private void Update()
        {
            if (!IsActive || board == null || config == null) return;

            // 计时下落
            fallTimer += Time.deltaTime;
            if (fallTimer >= config.BaseFallInterval)
            {
                fallTimer = 0f;
                TryFall();
            }
        }

        /// <summary>
        /// 尝试向下移动一格
        /// </summary>
        public bool TryFall()
        {
            var newPos = gridPosition + Vector3Int.down;
            if (board.CanPlace(newPos, shapeOffsets))
            {
                gridPosition = newPos;
                UpdateVisualPosition();
                return true;
            }
            else
            {
                // 无法下落 → 固定到棋盘
                LockInPlace();
                return false;
            }
        }

        /// <summary>
        /// 尝试移动（玩家推动等）
        /// </summary>
        public bool TryMove(Vector3Int direction)
        {
            var newPos = gridPosition + direction;
            if (board.CanPlace(newPos, shapeOffsets))
            {
                gridPosition = newPos;
                UpdateVisualPosition();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 绕指定轴旋转90度
        /// </summary>
        public bool TryRotate(int dx, int dy, int dz)
        {
            var newRotX = rotationX + dx;
            var newRotY = rotationY + dy;
            var newRotZ = rotationZ + dz;

            var rotated = BlockShapes.RotateShape(shapeOffsets, newRotX, newRotY, newRotZ);

            if (board.CanPlace(gridPosition, rotated))
            {
                rotationX = newRotX;
                rotationY = newRotY;
                rotationZ = newRotZ;
                shapeOffsets = rotated;
                UpdateVisualPosition();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 立即硬降到最低位置
        /// </summary>
        public void HardDrop()
        {
            while (TryFall()) { }
            // TryFall 内部会调用 LockInPlace
        }

        /// <summary>
        /// 将方块固定到棋盘
        /// </summary>
        private void LockInPlace()
        {
            IsActive = false;
            board.PlaceBlock(gridPosition, shapeOffsets, flags, blockColor);
            // GameManager 会通过事件收到通知并处理消除+生成新方块
        }

        /// <summary>
        /// 强制立即下落（供技能系统调用）
        /// </summary>
        public void ForceFall()
        {
            fallTimer = config.BaseFallInterval; // 触发立即下落
        }

        /// <summary>
        /// 更新方块在世界空间的位置
        /// </summary>
        private void UpdateVisualPosition()
        {
            // 方块的位置是basePos的平均中心
            var center = Vector3.zero;
            foreach (var offset in shapeOffsets)
                center += (Vector3)offset;
            center /= shapeOffsets.Length;

            transform.position = board.GridToWorld(gridPosition) + center;
        }

        private void OnDrawGizmos()
        {
            if (shapeOffsets == null || board == null) return;

            Gizmos.color = IsActive ? blockColor : Color.gray;
            foreach (var offset in shapeOffsets)
            {
                var pos = board.GridToWorld(gridPosition + offset);
                Gizmos.DrawWireCube(pos, Vector3.one * board.CellSize * 0.95f);
            }
        }
    }
}
