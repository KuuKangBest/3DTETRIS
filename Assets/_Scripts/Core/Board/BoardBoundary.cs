using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 棋盘边界 — 5面隐墙（底面+4侧面），顶部开放
    /// </summary>
    public class BoardBoundary : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private float wallThickness = 0.5f;

        private void Start()
        {
            if (board == null) board = GetComponent<Board>();
            CreateWalls();
        }

        private void CreateWalls()
        {
            float w = board.Width * board.CellSize;
            float h = board.Height * board.CellSize;
            float d = board.Depth * board.CellSize;
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float cz = d * 0.5f;
            float t = wallThickness;

            var walls = new GameObject("BoundaryWalls");
            walls.transform.SetParent(transform, false);

            // -Z 面
            CreateWall(walls, new Vector3(cx, cy, -t * 0.5f), new Vector3(w + t * 2, h, t), "Wall-Z");
            // +Z 面
            CreateWall(walls, new Vector3(cx, cy, d + t * 0.5f), new Vector3(w + t * 2, h, t), "Wall+Z");
            // -X 面
            CreateWall(walls, new Vector3(-t * 0.5f, cy, cz), new Vector3(t, h, d + t * 2), "Wall-X");
            // +X 面
            CreateWall(walls, new Vector3(w + t * 0.5f, cy, cz), new Vector3(t, h, d + t * 2), "Wall+X");
            // 底面
            CreateWall(walls, new Vector3(cx, -t * 0.5f, cz), new Vector3(w + t * 2, t, d + t * 2), "Wall-Y");
        }

        private void CreateWall(GameObject parent, Vector3 pos, Vector3 scale, string name)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent.transform, false);
            wall.transform.localPosition = pos;
            wall.transform.localScale = scale;

            // 隐藏渲染，保留碰撞，放在 IgnoreRaycast 层（不挡相机）
            wall.layer = LayerMask.NameToLayer("Ignore Raycast");
            var renderer = wall.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = false;
        }
    }
}
