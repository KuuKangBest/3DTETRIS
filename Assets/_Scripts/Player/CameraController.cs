using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 第三人称相机 — 独立于角色层级
    /// 通过引用获取输入和跟随目标，不依赖父子关系
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Transform followTarget;
        [SerializeField] private InputHandler inputHandler;

        [Header("偏移")]
        [SerializeField] private float baseYOffset = 0.2f;     // 平视时的高度偏移
        [SerializeField] private float yOffsetPitchFactor = 0.03f; // 每度俯角增加的高度
        [SerializeField] private float zDistance = 8f;         // 后方距离

        [Header("旋转")]
        [SerializeField] private float sensitivityX = 3f;
        [SerializeField] private float sensitivityY = 3f;
        [SerializeField] private float smoothing = 12f;
        [SerializeField] private float minPitch = -35f;
        [SerializeField] private float maxPitch = 50f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("遮挡处理")]
        [SerializeField] private bool fadeObstructions = true;   // 挡镜头的方块变半透明
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private float fadeAlpha = 0.3f;         // 遮挡时透明度

        // 内部状态
        private float yaw;
        private float pitch;
        private Vector3 currentVelocity = Vector3.zero;
        private float pitchVelocity;
        private float yawVelocity;
        private System.Collections.Generic.Dictionary<Renderer, Material> fadedRenderers
            = new System.Collections.Generic.Dictionary<Renderer, Material>();

        public bool CloseMode { get; set; }

        private void Awake()
        {
            // 回退：没设引用时尝试查找
            if (followTarget == null)
            {
                var pc = FindObjectOfType<PlayerController>();
                if (pc != null) followTarget = pc.transform;
            }
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<InputHandler>();
            }

            var euler = transform.eulerAngles;
            yaw = euler.y;
            pitch = euler.x > 180 ? euler.x - 360 : euler.x;
        }

        private void LateUpdate()
        {
            if (followTarget == null) return;

            HandleRotation();
            HandleFollow();
        }

        private void HandleRotation()
        {
            // 优先从 InputHandler 读取，否则直接读鼠标
            float mx, my;
            if (inputHandler != null && inputHandler.CursorLocked)
            {
                var look = inputHandler.LookInput;
                mx = look.x;
                my = look.y;
            }
            else
            {
                mx = Input.GetAxis("Mouse X");
                my = Input.GetAxis("Mouse Y");
            }

            yaw += mx * sensitivityX;
            pitch -= my * sensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            // 平滑旋转
            float smoothPitch = Mathf.SmoothDampAngle(
                transform.eulerAngles.x > 180 ? transform.eulerAngles.x - 360 : transform.eulerAngles.x,
                pitch, ref pitchVelocity, rotationSmoothTime);
            float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, yaw, ref yawVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(smoothPitch, smoothYaw, 0);
        }

        private void HandleFollow()
        {
            float pitchBelowHorizon = Mathf.Max(0, -pitch);
            float dynamicY = baseYOffset + pitchBelowHorizon * yOffsetPitchFactor;
            float dist = CloseMode ? 2.5f : zDistance;

            Vector3 desiredOffset = transform.rotation * new Vector3(0, dynamicY, -dist);
            Vector3 targetPos = followTarget.position + desiredOffset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.08f);

            // 遮挡物半透明
            if (fadeObstructions)
                HandleObstructionFade();
        }

        private void HandleObstructionFade()
        {
            var toRestore = new System.Collections.Generic.List<Renderer>(fadedRenderers.Keys);
            var toFade = new System.Collections.Generic.List<Renderer>();

            Vector3 dir = (transform.position - followTarget.position).normalized;
            float dist = Vector3.Distance(transform.position, followTarget.position);
            var hits = Physics.RaycastAll(followTarget.position, dir, dist, obstructionMask);

            foreach (var hit in hits)
            {
                var r = hit.collider.GetComponent<Renderer>();
                if (r == null || r == followTarget.GetComponentInChildren<Renderer>()) continue;

                if (!fadedRenderers.ContainsKey(r))
                    toFade.Add(r);
                else
                    toRestore.Remove(r);
            }

            // 恢复不再遮挡的
            foreach (var r in toRestore)
                RestoreRenderer(r);

            // 新遮挡物变半透明
            foreach (var r in toFade)
                FadeRenderer(r);
        }

        private void FadeRenderer(Renderer r)
        {
            var orig = r.material;
            var faded = new Material(orig);
            faded.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            faded.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            faded.SetInt("_ZWrite", 1);
            faded.EnableKeyword("_ALPHABLEND_ON");
            faded.renderQueue = 3000;

            Color c = faded.color;
            c.a = fadeAlpha;
            faded.color = c;

            r.material = faded;
            fadedRenderers[r] = orig;
        }

        private void RestoreRenderer(Renderer r)
        {
            if (fadedRenderers.TryGetValue(r, out var orig))
            {
                r.material = orig;
                fadedRenderers.Remove(r);
            }
        }
    }
}
