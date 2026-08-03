using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 测试场景构建器 — 模拟真实游戏流程
    /// 每个方块必须通过 Board 注册，绝不重叠
    /// </summary>
    public class TestSceneBuilder : MonoBehaviour
    {
        [Header("方块外观")]
        [SerializeField] private float blockSizeRatio = 0.95f; // 方块相对格子的缩放比例

        [Header("随机堆叠")]
        [SerializeField] private int minStackY = 1;   // 堆叠起始高度
        [SerializeField] private int maxStackY = 4;   // 堆叠最高高度

        [Header("引用")]
        [SerializeField] private Board board;
        [SerializeField] private Transform playerTransform;

        private static readonly Color[] Colors =
        {
            new Color(1.00f, 0.42f, 0.42f),
            new Color(1.00f, 0.75f, 0.35f),
            new Color(1.00f, 0.92f, 0.42f),
            new Color(0.42f, 0.85f, 0.55f),
            new Color(0.42f, 0.65f, 1.00f),
            new Color(0.65f, 0.45f, 1.00f),
            new Color(1.00f, 0.55f, 0.75f),
            new Color(0.45f, 0.85f, 0.85f),
        };

        private float blockSize;

        private void Start()
        {
            if (board == null) board = FindObjectOfType<Board>();
            blockSize = board.CellSize * blockSizeRatio;

            BuildFloor();
            BuildRandomStacks();
            PlacePlayer();
        }

        /// <summary>
        /// 底层铺满 8×8 — 模拟游戏中掉落方块形成的平面
        /// </summary>
        private void BuildFloor()
        {
            System.Random rng = new System.Random(42);
            int placed = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    // 用 Board.PlaceBlock 注册 — 不会重叠
                    var pos = new Vector3Int(x, 0, z);
                    if (board.IsOccupied(pos)) continue;

                    var color = Colors[rng.Next(Colors.Length)];
                    board.PlaceBlock(pos, new[] { Vector3Int.zero }, EliminationFlags.All, color);
                    SpawnVisual(pos, color, "Floor");
                    placed++;
                }
            }

            Debug.Log($"[TestScene] 底层铺满 {placed} 个方块 (y=0)");
        }

        /// <summary>
        /// 随机堆叠 — 每个 (x,z) 随机叠 0~maxStackY 层
        /// </summary>
        private void BuildRandomStacks()
        {
            System.Random rng = new System.Random(137);
            int placed = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    int stackHeight = rng.Next(minStackY, maxStackY + 1);
                    if (stackHeight == 0) continue;

                    // 约 30% 概率留空，制造散落感
                    if (rng.NextDouble() < 0.3f) continue;

                    var color = Colors[rng.Next(Colors.Length)];
                    for (int y = minStackY; y <= stackHeight; y++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        if (board.IsOccupied(pos)) break; // 上方已被占，停止这列

                        board.PlaceBlock(pos, new[] { Vector3Int.zero }, EliminationFlags.All, color);
                        SpawnVisual(pos, color, "Block");
                        placed++;
                    }
                }
            }

            Debug.Log($"[TestScene] 随机堆叠 {placed} 个方块");
        }

        /// <summary>
        /// 创建方块的视觉实体（带碰撞）
        /// </summary>
        private void SpawnVisual(Vector3Int gridPos, Color color, string tag)
        {
            Vector3 worldPos = board.GridToWorld(gridPos);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"{tag}_{gridPos.x}_{gridPos.y}_{gridPos.z}";
            cube.transform.SetParent(transform, false);
            cube.transform.position = worldPos;
            cube.transform.localScale = Vector3.one * blockSize;

            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            cube.GetComponent<Renderer>().material = mat;
        }

        /// <summary>
        /// 玩家放到棋盘正中央
        /// </summary>
        private void PlacePlayer()
        {
            if (playerTransform == null)
            {
                var pc = FindObjectOfType<Player.PlayerController>();
                if (pc != null) playerTransform = pc.transform;
            }

            if (playerTransform != null)
            {
                float cx = board.Width * board.CellSize * 0.5f;
                float cz = board.Depth * board.CellSize * 0.5f;
                float py = board.CellSize * 2f; // 站在方块上方
                playerTransform.position = new Vector3(cx, py, cz);
            }
        }
    }
}
