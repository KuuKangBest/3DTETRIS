using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 测试场景构建器 — 在 Start 时自动生成散落方块
    /// 仅在编辑器/测试时使用
    /// </summary>
    public class TestSceneBuilder : MonoBehaviour
    {
        [Header("方块配置")]
        [SerializeField] private int blockCount = 30;
        [SerializeField] private float blockSize = 0.95f;
        [SerializeField] private Material[] materials;

        [Header("引用")]
        [SerializeField] private Board board;

        // 预定义的散落方块 (gridX, gridY, gridZ)
        // 模拟游戏中的真实堆叠状态
        private static readonly Vector3Int[] ScatterPositions = new Vector3Int[]
        {
            // z=0 列
            new(2,0,0), new(5,0,0), new(5,1,0),
            // z=1 列
            new(1,0,1), new(3,0,1), new(6,0,1), new(1,1,1),
            // z=2 列 — 中间有座小塔
            new(0,0,2), new(4,0,2), new(7,0,2), new(4,1,2), new(4,2,2),
            // z=3 列 — 两柱
            new(2,0,3), new(5,0,3), new(2,1,3), new(5,1,3),
            // z=4 列
            new(3,0,4), new(6,0,4), new(3,1,4), new(6,1,4),
            // z=5 列
            new(1,0,5), new(4,0,5), new(7,0,5),
            // z=6 列
            new(0,0,6), new(2,0,6), new(5,0,6), new(2,1,6),
            // z=7 列
            new(3,0,7), new(6,0,7), new(3,1,7),
        };

        private void Start()
        {
            if (board == null)
                board = FindObjectOfType<Board>();

            var blocksRoot = transform; // 本组件所在的 GameObject 作为根

            // 确保有材质
            if (materials == null || materials.Length == 0)
                materials = CreateFallbackMaterials();

            // 生成方块
            for (int i = 0; i < ScatterPositions.Length; i++)
            {
                var gp = ScatterPositions[i];
                Vector3 worldPos = board.GridToWorld(gp.x, gp.y, gp.z);

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Block_{gp.x}_{gp.y}_{gp.z}";
                cube.transform.SetParent(blocksRoot, false);
                cube.transform.position = worldPos;
                cube.transform.localScale = Vector3.one * blockSize;

                // 随机上色
                var mat = materials[i % materials.Length];
                cube.GetComponent<Renderer>().material = mat;
            }

            Debug.Log($"[TestSceneBuilder] 生成了 {ScatterPositions.Length} 个散落方块");
        }

        private Material[] CreateFallbackMaterials()
        {
            Color[] colors = {
                new Color(1f, 0.42f, 0.42f),   // 红
                new Color(1f, 0.75f, 0.35f),   // 橙
                new Color(1f, 0.92f, 0.42f),   // 黄
                new Color(0.42f, 0.85f, 0.55f), // 绿
                new Color(0.42f, 0.65f, 1f),    // 蓝
                new Color(0.65f, 0.45f, 1f),    // 紫
                new Color(1f, 0.55f, 0.75f),    // 粉
                new Color(0.45f, 0.85f, 0.85f), // 青
            };

            var mats = new Material[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                mats[i] = new Material(Shader.Find("Standard"));
                mats[i].color = colors[i];
            }
            return mats;
        }
    }
}
