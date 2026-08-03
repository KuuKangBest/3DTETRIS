using System.Collections.Generic;
using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 棋盘可视化网格 — 运行时可见的格子线
    /// 用一个 LineRenderer 画外框 + 关键线，不对性能造成负担
    /// </summary>
    public class BoardGrid : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Color gridColor = new Color(0.3f, 0.6f, 0.9f, 0.4f);
        [SerializeField] private float lineWidth = 0.04f;

        private void Start()
        {
            if (board == null) board = GetComponent<Board>();
            if (lineMaterial == null) lineMaterial = new Material(Shader.Find("Sprites/Default"));

            var go = new GameObject("GridLines");
            go.transform.SetParent(transform, false);

            float w = board.Width * board.CellSize;
            float h = board.Height * board.CellSize;
            float d = board.Depth * board.CellSize;
            Vector3 o = transform.position;

            // ═══ 底边格子线 (XZ平面，每格一条) ═══
            CreateGridPlane(go, o, w, d, board, true, gridColor);
            // ═══ 顶边格子线 (XZ平面，标记顶部) ═══
            CreateGridPlane(go, o + Vector3.up * h, w, d, board, false, new Color(1f, 0.3f, 0.3f, 0.3f));
            // ═══ 边框 ═══
            CreateBoxFrame(go, o, w, h, d, Color.white);
        }

        void CreateGridPlane(GameObject parent, Vector3 origin, float w, float d, Board b, bool drawFull, Color color)
        {
            var points = new List<Vector3>();
            float cs = b.CellSize;

            // X方向线
            for (int z = 0; z <= b.Depth; z++)
            {
                points.Add(origin + new Vector3(0, 0, z * cs));
                points.Add(origin + new Vector3(w, 0, z * cs));
            }
            // Z方向线
            for (int x = 0; x <= b.Width; x++)
            {
                points.Add(origin + new Vector3(x * cs, 0, 0));
                points.Add(origin + new Vector3(x * cs, 0, d));
            }

            var lr = new GameObject("GridPlane").AddComponent<LineRenderer>();
            lr.transform.SetParent(parent.transform, false);
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = lineMaterial;
            lr.startColor = color;
            lr.endColor = color;
            lr.useWorldSpace = true;
        }

        void CreateBoxFrame(GameObject parent, Vector3 o, float w, float h, float d, Color color)
        {
            Vector3[] corners = {
                o + new Vector3(0,0,0), o + new Vector3(w,0,0), o + new Vector3(w,0,d), o + new Vector3(0,0,d), o + new Vector3(0,0,0), // bottom
                o + new Vector3(0,h,0), o + new Vector3(w,h,0), o + new Vector3(w,h,d), o + new Vector3(0,h,d), o + new Vector3(0,h,0), // top
            };

            var lr = new GameObject("BoxFrame").AddComponent<LineRenderer>();
            lr.transform.SetParent(parent.transform, false);
            lr.positionCount = 4;
            // bottom face
            var bottom = new Vector3[] { corners[0], corners[1], corners[2], corners[3] };
            // top face
            var top = new Vector3[] { corners[5], corners[6], corners[7], corners[8] };

            // 用4条竖线连接
            for (int i = 0; i < 4; i++)
            {
                var v = new GameObject($"Edge{i}").AddComponent<LineRenderer>();
                v.transform.SetParent(parent.transform, false);
                v.positionCount = 2;
                v.SetPosition(0, bottom[i]);
                v.SetPosition(1, top[i]);
                v.startWidth = lineWidth * 2;
                v.endWidth = lineWidth * 2;
                v.material = lineMaterial;
                v.startColor = color;
                v.endColor = color;
                v.useWorldSpace = true;
            }
        }
    }
}
