using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 棋盘渲染器 — 负责将Board数据可视化
    /// 第一版使用Gizmos + 基础Cube，后续可替换为美术资源
    /// </summary>
    public class BoardRenderer : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Board board;
        [SerializeField] private GameObject cellPrefab;       // 单个方块预制体
        [SerializeField] private Transform cellsContainer;    // 方块的父节点

        [Header("外观")]
        [SerializeField] private Color gridLineColor = new Color(0.2f, 0.2f, 0.3f, 0.4f);
        [SerializeField] private Color basePlaneColor = new Color(0.15f, 0.15f, 0.25f, 0.3f);

        private GameObject[,][] cellObjects; // [X, Z][Y] — 存储渲染对象引用

        private void Awake()
        {
            if (board == null)
                board = GetComponent<Board>();

            if (cellsContainer == null)
            {
                cellsContainer = new GameObject("CellsContainer").transform;
                cellsContainer.SetParent(transform);
            }
        }

        private void Start()
        {
            InitializeCellObjectArray();
            board.OnCellFilled.AddListener(OnCellFilled);
            board.OnCellsEliminated.AddListener(OnCellsEliminated);
        }

        private void InitializeCellObjectArray()
        {
            cellObjects = new GameObject[board.Width, board.Depth][];
            for (int x = 0; x < board.Width; x++)
                for (int z = 0; z < board.Depth; z++)
                    cellObjects[x, z] = new GameObject[board.Height];
        }

        private void OnCellFilled(Vector3Int pos)
        {
            CreateCellVisual(pos);
        }

        private void CreateCellVisual(Vector3Int pos)
        {
            var cell = board.GetCell(pos);
            if (!cell.IsOccupied) return;

            GameObject obj;
            if (cellPrefab != null)
            {
                obj = Instantiate(cellPrefab, cellsContainer);
            }
            else
            {
                // 后备：创建基础Cube
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.SetParent(cellsContainer);
            }

            obj.name = $"Cell_{pos.x}_{pos.y}_{pos.z}";
            obj.transform.position = board.GridToWorld(pos);
            obj.transform.localScale = Vector3.one * board.CellSize * 0.95f;

            // 设置颜色
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = cell.CellColor;
                renderer.material = mat;
            }

            cellObjects[pos.x, pos.z][pos.y] = obj;
        }

        private void OnCellsEliminated(Vector3Int[] positions, int count)
        {
            foreach (var pos in positions)
            {
                if (cellObjects[pos.x, pos.z][pos.y] != null)
                {
                    Destroy(cellObjects[pos.x, pos.z][pos.y]);
                    cellObjects[pos.x, pos.z][pos.y] = null;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (board == null) return;

            // 画网格线
            Gizmos.color = gridLineColor;
            var origin = board.Origin;
            float w = board.Width * board.CellSize;
            float h = board.Height * board.CellSize;
            float d = board.Depth * board.CellSize;

            // X轴方向线
            for (int y = 0; y <= board.Height; y++)
            {
                for (int z = 0; z <= board.Depth; z++)
                {
                    var start = origin + new Vector3(0, y * board.CellSize, z * board.CellSize);
                    var end = start + new Vector3(w, 0, 0);
                    Gizmos.DrawLine(start, end);
                }
            }

            // Z轴方向线
            for (int y = 0; y <= board.Height; y++)
            {
                for (int x = 0; x <= board.Width; x++)
                {
                    var start = origin + new Vector3(x * board.CellSize, y * board.CellSize, 0);
                    var end = start + new Vector3(0, 0, d);
                    Gizmos.DrawLine(start, end);
                }
            }

            // Y轴方向线
            for (int x = 0; x <= board.Width; x++)
            {
                for (int z = 0; z <= board.Depth; z++)
                {
                    var start = origin + new Vector3(x * board.CellSize, 0, z * board.CellSize);
                    var end = start + new Vector3(0, h, 0);
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}
