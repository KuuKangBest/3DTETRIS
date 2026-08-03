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

        [Header("碰撞")]
        [SerializeField] private LayerMask obstructionMask = ~(1 << 2); // 排除 IgnoreRaycast 层
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minDistance = 1.5f;

        // 内部状态
        private float yaw;
        private float pitch;
        private Vector3 currentVelocity = Vector3.zero;
        private float pitchVelocity;
        private float yawVelocity;

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

            // 强制排除 IgnoreRaycast 层（暗墙），相机可自由穿出
            obstructionMask &= ~(1 << LayerMask.NameToLayer("Ignore Raycast"));
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
            // 动态 Y 偏移：俯角越大，相机越高（能看到脚和地面）
            float pitchBelowHorizon = Mathf.Max(0, -pitch);
            float dynamicY = baseYOffset + pitchBelowHorizon * yOffsetPitchFactor;

            float dist = CloseMode ? 2.5f : zDistance;

            // 根据当前旋转计算偏移
            Vector3 desiredOffset = transform.rotation * new Vector3(0, dynamicY, -dist);
            Vector3 targetPos = followTarget.position + desiredOffset;

            // 碰撞避让
            Vector3 dir = (targetPos - followTarget.position).normalized;
            float checkDist = Vector3.Distance(targetPos, followTarget.position);
            if (Physics.SphereCast(followTarget.position, collisionRadius, dir, out var hit, checkDist, obstructionMask))
            {
                targetPos = followTarget.position + dir * Mathf.Max(hit.distance - collisionRadius, minDistance);
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, 0.08f);
        }
    }
}
