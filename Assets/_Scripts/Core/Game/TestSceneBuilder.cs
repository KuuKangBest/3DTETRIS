using System.Collections.Generic;
using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 测试场景构建器 — 底层铺满 8×8，上方随机堆叠
    /// </summary>
    public class TestSceneBuilder : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private float blockSize = 0.95f;
        [SerializeField] private int minStackHeight = 0;   // 每个格点最低堆叠高度
        [SerializeField] private int maxStackHeight = 4;   // 每个格点最高堆叠高度

        [Header("引用")]
        [SerializeField] private Board board;
        [SerializeField] private Transform playerTransform;

        private static readonly Color[] BlockColors =
        {
            new Color(1.00f, 0.42f, 0.42f), // 红
            new Color(1.00f, 0.75f, 0.35f), // 橙
            new Color(1.00f, 0.92f, 0.42f), // 黄
            new Color(0.42f, 0.85f, 0.55f), // 绿
            new Color(0.42f, 0.65f, 1.00f), // 蓝
            new Color(0.65f, 0.45f, 1.00f), // 紫
            new Color(1.00f, 0.55f, 0.75f), // 粉
            new Color(0.45f, 0.85f, 0.85f), // 青
        };

        private void Start()
        {
            if (board == null)
                board = FindObjectOfType<Board>();

            BuildFloor();
            BuildRandomStacks();
            PlacePlayer();
        }

        /// <summary>
        /// 底层铺满 8×8（y=0）
        /// </summary>
        private void BuildFloor()
        {
            var mat = CreateMaterial(BlockColors[4]); // 蓝色地板

            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    CreateBlock(x, 0, z, mat, "Floor");
                }
            }

            Debug.Log("[TestScene] 底层 8×8 铺满完成");
        }

        /// <summary>
        /// 每个 (x,z) 格点随机叠加方块
        /// </summary>
        private void BuildRandomStacks()
        {
            // 确保同一列的方块颜色一致（看起来像整块叠上去的）
            // 但为了更像散落，每层随机
            int blockCount = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int z = 0; z < board.Depth; z++)
                {
                    int height = Random.Range(minStackHeight, maxStackHeight + 1);

                    // 留一些空洞 — 约30%的格点不叠
                    if (Random.value < 0.3f && height == maxStackHeight)
                        height = 0;

                    for (int y = 1; y <= height; y++)
                    {
                        var mat = CreateMaterial(BlockColors[Random.Range(0, BlockColors.Length)]);
                        CreateBlock(x, y, z, mat, "Block");
                        blockCount++;
                    }
                }
            }

            Debug.Log($"[TestScene] 随机堆叠 {blockCount} 个方块");
        }

        /// <summary>
        /// 玩家放在棋盘正中央
        /// </summary>
        private void PlacePlayer()
        {
            if (playerTransform == null)
            {
                var player = FindObjectOfType<Player.PlayerController>();
                if (player != null)
                    playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                float cx = board.Width * 0.5f;
                float cz = board.Depth * 0.5f;
                playerTransform.position = new Vector3(cx, 2f, cz);
            }
        }

        private GameObject CreateBlock(int x, int y, int z, Material mat, string tag)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"{tag}_{x}_{y}_{z}";
            cube.transform.SetParent(transform, false);
            // 直接放整数坐标 — 和网格线对齐
            cube.transform.position = new Vector3(x, y, z);
            cube.transform.localScale = Vector3.one * blockSize;

            var renderer = cube.GetComponent<Renderer>();
            renderer.material = mat;

            // BoxCollider 已经由 CreatePrimitive 自带

            return cube;
        }

        private Material CreateMaterial(Color color)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            return mat;
        }
    }
}
