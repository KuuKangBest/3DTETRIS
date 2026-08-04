using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 方块工厂 — 负责生成新的方块实例
    /// 包括标准方块、特殊方块（整行等）
    /// </summary>
    public class BlockFactory : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Board board;
        [SerializeField] private GameConfig config;
        [SerializeField] private GameObject blockPrefab;

        private void Awake()
        {
            if (config == null) config = Resources.Load<GameConfig>("GameConfig");
        }

        [Header("方块颜色")]
        [SerializeField] private Color[] blockColors = new Color[]
        {
            new Color(1f, 0.42f, 0.42f),    // 珊瑚红
            new Color(1f, 0.75f, 0.35f),    // 暖橙
            new Color(1f, 0.92f, 0.42f),    // 奶油黄
            new Color(0.42f, 0.85f, 0.55f), // 薄荷绿
            new Color(0.42f, 0.65f, 1f),    // 天空蓝
            new Color(0.65f, 0.45f, 1f),    // 淡紫
            new Color(1f, 0.55f, 0.75f),    // 粉红
            new Color(0.45f, 0.85f, 0.85f), // 青绿
        };

        /// <summary>
        /// 生成默认方块（标准形状 + 默认消除属性）
        /// </summary>
        public Block SpawnBlock()
        {
            // 有一定概率生成特殊方块
            if (Random.value < config.FullRowChance)
            {
                return SpawnFullRow();
            }

            return SpawnStandardBlock();
        }

        /// <summary>
        /// 生成标准3D方块
        /// </summary>
        public Block SpawnStandardBlock()
        {
            var shape = BlockShapes.StandardShapes[Random.Range(0, BlockShapes.StandardShapes.Length)];
            var color = blockColors[Random.Range(0, blockColors.Length)];
            var flags = EliminationFlags.All; // 默认所有消除类型都支持
            var startPos = GetSpawnPosition(shape);

            return CreateBlock(shape, flags, color, startPos);
        }

        /// <summary>
        /// 生成一整行方块（X方向8格，用于加速游戏进度）
        /// </summary>
        public Block SpawnFullRow()
        {
            var shape = BlockShapes.FullRowShape();
            var color = blockColors[Random.Range(0, blockColors.Length)];
            var flags = EliminationFlags.FaceEliminable | EliminationFlags.RowEliminable; // 整行天然可面消/行消
            // 从顶部掉落的整行，Z随机
            int z = Random.Range(config.SpawnZMin, config.SpawnZMax + 1);

            // 验证8格整行是否在Z范围内都能放置
            // 如果shape的Z偏移为0，basePos的Z就是z
            var startPos = new Vector3Int(0, config.SpawnHeight, z);

            return CreateBlock(shape, flags, color, startPos);
        }

        /// <summary>
        /// 生成2×2×2大方块（低概率特殊方块）
        /// </summary>
        public Block SpawnBigCube()
        {
            var shape = BlockShapes.Cube2x2x2;
            var color = blockColors[Random.Range(0, blockColors.Length)];
            var flags = EliminationFlags.All;
            var startPos = GetSpawnPosition(shape);

            return CreateBlock(shape, flags, color, startPos);
        }

        /// <summary>
        /// 计算生成位置（在棋盘顶部居中）
        /// </summary>
        private Vector3Int GetSpawnPosition(Vector3Int[] shape)
        {
            // 计算形状在XZ平面的尺寸
            int minX = int.MaxValue, maxX = int.MinValue;
            int minZ = int.MaxValue, maxZ = int.MinValue;
            foreach (var off in shape)
            {
                if (off.x < minX) minX = off.x;
                if (off.x > maxX) maxX = off.x;
                if (off.z < minZ) minZ = off.z;
                if (off.z > maxZ) maxZ = off.z;
            }

            int shapeWidth = maxX - minX + 1;
            int shapeDepth = maxZ - minZ + 1;

            // 随机XZ位置
            int maxX = config.SpawnXMax - shapeWidth + 1;
            int maxZ = config.SpawnZMax - shapeDepth + 1;
            int x = Random.Range(config.SpawnXMin, Mathf.Max(config.SpawnXMin + 1, maxX + 1));
            int z = Random.Range(config.SpawnZMin, Mathf.Max(config.SpawnZMin + 1, maxZ + 1));

            return new Vector3Int(x, config.SpawnHeight, z);
        }

        /// <summary>
        /// 创建Block实例
        /// </summary>
        private Block CreateBlock(Vector3Int[] shape, EliminationFlags flags, Color color, Vector3Int startPos)
        {
            GameObject obj;
            if (blockPrefab != null)
            {
                obj = Instantiate(blockPrefab, transform);
            }
            else
            {
                obj = new GameObject($"Block_{System.Guid.NewGuid().ToString()[..6]}");
                obj.transform.SetParent(transform);
            }

            var block = obj.GetComponent<Block>();
            if (block == null)
                block = obj.AddComponent<Block>();

            block.Initialize(board, config, shape, flags, color, startPos);
            return block;
        }
    }
}
