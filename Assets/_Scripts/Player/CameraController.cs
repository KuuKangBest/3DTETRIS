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
        [SerializeField] private Vector3 followOffset = new Vector3(0, 1.5f, -3f);

        [Header("旋转")]
        [SerializeField] private float sensitivityX = 3f;
        [SerializeField] private float sensitivityY = 3f;
        [SerializeField] private float smoothing = 12f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;
        [SerializeField] private float rotationSmoothTime = 0.1f;

        [Header("碰撞")]
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minDistance = 1f;

        // 内部状态
        private float yaw;
        private float pitch;
        private Vector3 currentVelocity = Vector3.zero;
        private float pitchVelocity;
        private float yawVelocity;

        public bool CloseMode { get; set; }
        private float targetDistance => CloseMode ? 2.5f : 5f;

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
            float dist = targetDistance;

            // 理想相机位置
            Vector3 desiredOffset = transform.rotation * new Vector3(0, followOffset.y, -dist);
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
