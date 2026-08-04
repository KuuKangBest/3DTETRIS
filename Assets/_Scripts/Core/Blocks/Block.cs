using UnityEngine;

namespace TDTTetris.Core
{
    public class Block : MonoBehaviour
    {
        [Header("方块属性")]
        [SerializeField] private Vector3Int[] shapeOffsets;
        [SerializeField] private EliminationFlags flags = EliminationFlags.All;
        [SerializeField] private Color blockColor = Color.white;

        [Header("落定延迟")]
        [SerializeField] private float lockDelay = 0.5f;  // 触底后可微调的时间

        // 内部状态
        private int rotationX, rotationY, rotationZ;
        private Board board;
        private GameConfig config;
        private float fallTimer;
        private float lockTimer;          // 触底后计时
        private bool isTouchingGround;    // 触底但未落定
        private bool hasPlaced;           // 已在棋盘注册

        public Vector3Int[] ShapeOffsets => shapeOffsets;
        public Vector3Int GridPosition => gridPosition;
        private Vector3Int gridPosition;
        public EliminationFlags Flags => flags;
        public Color BlockColor => blockColor;
        public bool IsActive => !hasPlaced;

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
            lockTimer = 0f;
            isTouchingGround = false;
            hasPlaced = false;

            CreateVisualCubes();
            CreateGhostPreview();
        }

        public Vector3Int CalculateLandingPosition()
        {
            var landing = gridPosition;
            while (board.CanPlace(landing + Vector3Int.down, shapeOffsets))
                landing += Vector3Int.down;

            // 检查每个cell
            foreach (var off in shapeOffsets)
            {
                var checkPos = landing + off;
                Debug.Log($"[Block] Ghost cell at {checkPos} — occupied: {board.IsOccupied(checkPos)}");
            }
            Debug.Log($"[Block] Ghost land at {landing}, below CanPlace: {!board.CanPlace(landing + Vector3Int.down, shapeOffsets)}");
            return landing;
        }

        private void Update()
        {
            if (hasPlaced || board == null || config == null) return;

            // 触底锁定时
            if (isTouchingGround)
            {
                lockTimer += Time.deltaTime;
                if (lockTimer >= lockDelay)
                {
                    LockInPlace();
                }
                return;
            }

            // 正常下落
            fallTimer += Time.deltaTime;
            if (fallTimer >= config.BaseFallInterval)
            {
                fallTimer = 0f;
                TryFall();
            }
        }

        public bool TryFall()
        {
            var newPos = gridPosition + Vector3Int.down;
            if (board.CanPlace(newPos, shapeOffsets))
            {
                gridPosition = newPos;
                UpdateVisualPosition();
                CreateGhostPreview();
                isTouchingGround = false;
                lockTimer = 0f;
                return true;
            }
            else
            {
                // 触底 — 开始落定倒计时
                isTouchingGround = true;
                lockTimer = 0f;
                return false;
            }
        }

        public bool TryMove(Vector3Int direction)
        {
            var newPos = gridPosition + direction;
            if (board.CanPlace(newPos, shapeOffsets))
            {
                gridPosition = newPos;
                UpdateVisualPosition();
                CreateGhostPreview();

                // 移动后如果下方有空间，取消锁定
                if (board.CanPlace(gridPosition + Vector3Int.down, shapeOffsets))
                {
                    isTouchingGround = false;
                    lockTimer = 0f;
                }

                return true;
            }
            return false;
        }

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
                RefreshVisuals();

                // 旋转后如果下方有空间，取消锁定
                if (board.CanPlace(gridPosition + Vector3Int.down, shapeOffsets))
                {
                    isTouchingGround = false;
                    lockTimer = 0f;
                }

                return true;
            }
            return false;
        }

        public void HardDrop()
        {
            while (TryFall()) { }
        }

        private void LockInPlace()
        {
            hasPlaced = true;
            isTouchingGround = false;

            // 清除幽灵
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var c = transform.GetChild(i);
                if (c.name.StartsWith("Ghost"))
                    Destroy(c.gameObject);
            }

            // 注册到棋盘
            board.PlaceBlock(gridPosition, shapeOffsets, flags, blockColor);
            Debug.Log($"[Block] LockInPlace at grid {gridPosition}, cells count={shapeOffsets.Length}, " +
                      $"sample IsOccupied={board.IsOccupied(gridPosition + shapeOffsets[0])}");
        }

        public void ForceFall()
        {
            fallTimer = config.BaseFallInterval;
        }

        public void CreateGhostPreview()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var c = transform.GetChild(i);
                if (c.name.StartsWith("Ghost"))
                    Destroy(c.gameObject);
            }

            if (hasPlaced) return;

            var landing = CalculateLandingPosition();
            float cs = board.CellSize * 0.95f;

            foreach (var off in shapeOffsets)
            {
                Vector3 worldPos = board.GridToWorld(landing + off);
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Ghost_{off.x}_{off.y}_{off.z}";
                cube.transform.SetParent(transform, false);
                cube.transform.position = worldPos;
                cube.transform.localScale = Vector3.one * cs;
                Destroy(cube.GetComponent<BoxCollider>());

                var mat = new Material(Shader.Find("Standard"));
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 1);
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.renderQueue = 3000;
                mat.color = new Color(1f, 1f, 1f, 0.5f);
                cube.GetComponent<Renderer>().material = mat;
            }
        }

        private void CreateVisualCubes()
        {
            float cs = board.CellSize * 0.95f;
            foreach (var off in shapeOffsets)
            {
                Vector3 worldPos = board.GridToWorld(gridPosition + off);
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"Cell_{off.x}_{off.y}_{off.z}";
                cube.transform.SetParent(transform, false);
                cube.transform.position = worldPos;
                cube.transform.localScale = Vector3.one * cs;

                var mat = new Material(Shader.Find("Standard"));
                mat.color = blockColor;
                cube.GetComponent<Renderer>().material = mat;
            }
        }

        private void UpdateVisualPosition()
        {
            for (int i = 0; i < shapeOffsets.Length && i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.name.StartsWith("Cell"))
                    child.position = board.GridToWorld(gridPosition + shapeOffsets[i]);
            }
        }

        private void RefreshVisuals()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
            CreateVisualCubes();
            CreateGhostPreview();
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
