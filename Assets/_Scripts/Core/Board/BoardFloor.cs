using UnityEngine;

namespace TDTTetris.Core
{
    /// <summary>
    /// 棋盘地板 — 启动时自动根据 Board 尺寸生成
    /// CellSize 改变无需手动调整
    /// </summary>
    public class BoardFloor : MonoBehaviour
    {
        [SerializeField] private Board board;
        [SerializeField] private Color floorColor = new Color(0.25f, 0.35f, 0.45f, 0.6f);

        private void Start()
        {
            if (board == null) board = GetComponent<Board>();
            CreateFloor();
        }

        private void CreateFloor()
        {
            float w = board.Width * board.CellSize;
            float d = board.Depth * board.CellSize;
            float thickness = board.CellSize * 0.2f;

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition = new Vector3(w * 0.5f, -thickness * 0.5f, d * 0.5f);
            floor.transform.localScale = new Vector3(w + 1f, thickness, d + 1f);

            var mat = new Material(Shader.Find("Standard"));
            mat.color = floorColor;
            // 半透明
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 1);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            floor.GetComponent<Renderer>().material = mat;

            Debug.Log($"[BoardFloor] 地板 {w:F0}x{d:F0} 自动生成");
        }
    }
}
