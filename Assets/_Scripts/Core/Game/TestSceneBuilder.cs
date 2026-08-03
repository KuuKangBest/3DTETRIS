using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 测试场景构建器 — Play时自动随机生成，编辑器中右键 Generate/Clear
    /// </summary>
    public class TestSceneBuilder : MonoBehaviour
    {
        [Header("方块外观")]
        [SerializeField] private float blockSizeRatio = 0.95f;

        [Header("随机堆叠")]
        [SerializeField] private int minStackY = 1;
        [SerializeField] private int maxStackY = 2;

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

            // 完全重置
            board?.ClearAll();
            ClearChildren();

            blockSize = board.CellSize * blockSizeRatio;
            BuildFloor();
            BuildRandomStacks();
            PlacePlayer();
        }

        #region 编辑模式右键菜单

        [ContextMenu("Generate")]
        private void EditorGenerate()
        {
            if (Application.isPlaying) return;
            if (board == null) board = FindObjectOfType<Board>();

            board?.ClearAll();
            ClearChildren();

            blockSize = board.CellSize * blockSizeRatio;
            BuildFloor();
            BuildRandomStacks();
        }

        [ContextMenu("Clear")]
        private void EditorClear()
        {
            if (Application.isPlaying) return;
            board?.ClearAll();
            ClearChildren();
        }

        #endregion

        private void BuildFloor()
        {
            System.Random rng = new System.Random(Random.Range(1, 99999));
            int placed = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    var pos = new Vector3Int(x, 0, z);
                    if (board.IsOccupied(pos)) continue;

                    var color = Colors[rng.Next(Colors.Length)];
                    board.PlaceBlock(pos, new[] { Vector3Int.zero }, EliminationFlags.All, color);
                    SpawnVisual(pos, color, "Floor");
                    placed++;
                }
            }

            Debug.Log($"[TestScene] 底层 {placed} 块");
        }

        private void BuildRandomStacks()
        {
            System.Random rng = new System.Random(Random.Range(1, 99999));
            int placed = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    int stackHeight = rng.Next(minStackY, maxStackY + 1);
                    if (stackHeight == 0) continue;
                    if (rng.NextDouble() < 0.3f) continue;

                    var color = Colors[rng.Next(Colors.Length)];
                    for (int y = minStackY; y <= stackHeight; y++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        if (board.IsOccupied(pos)) break;

                        board.PlaceBlock(pos, new[] { Vector3Int.zero }, EliminationFlags.All, color);
                        SpawnVisual(pos, color, "Block");
                        placed++;
                    }
                }
            }

            Debug.Log($"[TestScene] 堆叠 {placed} 块");
        }

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
                float py = board.CellSize * 2f;
                playerTransform.position = new Vector3(cx, py, cz);
            }
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}
