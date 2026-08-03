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
        [SerializeField] private float baseYOffset = 0.5f;
        [SerializeField] private float yOffsetPitchFactor = 0.025f;
        [SerializeField] private float zDistance = 3f;

        [Header("旋转")]
        [SerializeField] private float sensitivityX = 2f;
        [SerializeField] private float sensitivityY = 2f;
        [SerializeField] private float smoothing = 12f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("遮挡处理")]
        [SerializeField] private bool fadeObstructions = true;
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private float fadeAlpha = 0.3f;
        [SerializeField] private float fadeCheckSize = 1.5f;
        [SerializeField] private float fadeForwardOffset = 0.6f; // 射线从人物前方出发

        // 内部状态
        private float yaw;
        private float pitch;
        private Vector3 currentVelocity = Vector3.zero;
        private float pitchVelocity;
        private float yawVelocity;
        private System.Collections.Generic.Dictionary<Renderer, Material> fadedRenderers
            = new System.Collections.Generic.Dictionary<Renderer, Material>();
        private System.Collections.Generic.Dictionary<Renderer, float> fadeTargets
            = new System.Collections.Generic.Dictionary<Renderer, float>(); // 1=opaque, fadeAlpha=transparent

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
            if (fadeObstructions) UpdateFade();
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
            Vector3 dir = (transform.position - followTarget.position).normalized;
            float dist = Vector3.Distance(transform.position, followTarget.position);
            Vector3 right = Vector3.Cross(dir, Vector3.up).normalized;
            Vector3 up = Vector3.Cross(right, dir).normalized;
            float s = fadeCheckSize * 0.5f;

            // 4条射线 — 从角色前方出发
            Vector3 fwd = dir * fadeForwardOffset;
            Vector3[] origins = {
                followTarget.position + right * s + up * s + fwd,
                followTarget.position - right * s - up * s + fwd,
                followTarget.position + right * s - up * s + fwd,
                followTarget.position - right * s + up * s + fwd,
            };

            var seen = new System.Collections.Generic.HashSet<Renderer>();
            foreach (var origin in origins)
            {
                foreach (var hit in Physics.RaycastAll(origin, dir, dist, obstructionMask))
                {
                    var r = hit.collider.GetComponent<Renderer>();
                    if (r == null || r.transform.IsChildOf(followTarget)) continue;
                    seen.Add(r);
                }
            }

            // 设置目标透明度
            foreach (var r in seen)
            {
                fadeTargets[r] = fadeAlpha;
                PrepareFadeMaterial(r);
            }

            // 不再遮挡的恢复为不透明
            var toRestore = new System.Collections.Generic.List<Renderer>();
            foreach (var kv in fadeTargets)
            {
                if (!seen.Contains(kv.Key))
                    toRestore.Add(kv.Key);
            }
            foreach (var r in toRestore)
                fadeTargets[r] = 1f;
        }

        // 每帧在 LateUpdate 中调用，平滑过渡
        private void UpdateFade()
        {
            float dt = Time.deltaTime * 5f; // 渐变速度
            var done = new System.Collections.Generic.List<Renderer>();

            foreach (var kv in fadeTargets)
            {
                var r = kv.Key;
                if (r == null || r.material == null) { done.Add(r); continue; }

                float current = r.material.color.a;
                float target = kv.Value;
                float next = Mathf.Lerp(current, target, dt);

                Color c = r.material.color;
                c.a = next;
                r.material.color = c;

                // 到达目标 → 如果是恢复为不透明就清理
                if (Mathf.Abs(next - 1f) < 0.02f && Mathf.Abs(target - 1f) < 0.01f)
                {
                    if (fadedRenderers.TryGetValue(r, out var orig))
                    {
                        r.material = orig;
                        fadedRenderers.Remove(r);
                    }
                    done.Add(r);
                }
            }

            foreach (var r in done)
                fadeTargets.Remove(r);
        }

        private void PrepareFadeMaterial(Renderer r)
        {
            if (fadedRenderers.ContainsKey(r)) return;

            var orig = r.material;
            var faded = new Material(orig);
            faded.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            faded.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            faded.SetInt("_ZWrite", 1);
            faded.EnableKeyword("_ALPHABLEND_ON");
            faded.renderQueue = 3000;
            faded.color = orig.color; // 保持原色，只透明

            fadedRenderers[r] = orig;
            r.material = faded;
        }
    }
}
