using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 第三人称相机控制器 — 独立于角色移动
    /// 平滑跟随 + 碰撞检测防止穿墙
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("目标")]
        [SerializeField] private Transform followTarget;  // 跟随的Player Transform
        [SerializeField] private Vector3 followOffset = new Vector3(0, 1.8f, -5f);

        [Header("旋转")]
        [SerializeField] private float sensitivity = 2f;
        [SerializeField] private float smoothing = 10f;
        [SerializeField] private float minPitch = -30f;
        [SerializeField] private float maxPitch = 60f;

        [Header("碰撞")]
        [SerializeField] private LayerMask obstructionMask = ~0;
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private float minDistance = 1f;

        [Header("臂长切换（进入棋盘内部时拉近）")]
        [SerializeField] private float closeDistance = 2.5f;
        [SerializeField] private float farDistance = 5f;
        private float targetDistance;

        private float yaw;
        private float pitch;
        private Vector3 smoothPosition;
        private Vector3 smoothForward;

        public bool CloseMode { get; set; }

        private void Awake()
        {
            if (followTarget == null)
                followTarget = transform.parent;

            targetDistance = farDistance;
            smoothPosition = transform.position;
            smoothForward = transform.forward;

            // 初始角度
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
            // 从 InputHandler 读取（如果存在）或直接读鼠标
            Vector2 lookDelta = Vector2.zero;
            var input = GetComponentInParent<InputHandler>();
            if (input != null)
                lookDelta = input.LookInput;

            yaw += lookDelta.x * sensitivity;
            pitch -= lookDelta.y * sensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            var targetRotation = Quaternion.Euler(pitch, yaw, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothing * Time.deltaTime);
        }

        private void HandleFollow()
        {
            targetDistance = CloseMode ? closeDistance : farDistance;

            // 计算理想位置（根据当前旋转 + 偏移）
            Vector3 idealOffset = followTarget.rotation * new Vector3(0, followOffset.y, -targetDistance);
            Vector3 targetPosition = followTarget.position + idealOffset;

            // 碰撞检测：从目标发射射线，确保相机不会被墙挡住
            Vector3 dirToCamera = (targetPosition - followTarget.position).normalized;
            float checkDistance = Vector3.Distance(targetPosition, followTarget.position);

            if (Physics.SphereCast(followTarget.position, collisionRadius, dirToCamera, out var hit, checkDistance, obstructionMask))
            {
                targetPosition = followTarget.position + dirToCamera * Mathf.Max(hit.distance - collisionRadius, minDistance);
            }

            smoothPosition = Vector3.Lerp(smoothPosition, targetPosition, smoothing * Time.deltaTime);
            transform.position = smoothPosition;
        }
    }
}
