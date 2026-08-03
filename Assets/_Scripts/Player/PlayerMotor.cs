using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 角色物理马达 — 纯移动逻辑，与输入解耦
    /// 处理重力、跳跃、地面检测，易于扩展新能力
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float acceleration = 20f;    // 地面加速度
        [SerializeField] private float airAcceleration = 8f;  // 空中加速度

        [Header("跳跃")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float coyoteTime = 0.15f;    // 离开边缘后仍可跳跃的时间
        [SerializeField] private float jumpBufferTime = 0.1f; // 提前按跳跃的缓冲时间

        [Header("重力")]
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float maxFallSpeed = 30f;
        [SerializeField] private float groundStickForce = 5f; // 下坡时贴地力度

        [Header("地面检测")]
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float groundCheckOffset = 0.1f;
        [SerializeField] private float slopeLimit = 45f;

        // 组件
        private CharacterController controller;
        private Transform cachedTransform;

        // 速度
        private Vector3 velocity;          // 水平速度
        private float verticalVelocity;    // 垂直速度

        // 状态
        private bool isGrounded;
        private float lastGroundedTime;
        private float lastJumpPressedTime;
        private bool jumpConsumed;

        // 属性
        public Vector3 Velocity => velocity + Vector3.up * verticalVelocity;
        public bool IsGrounded => isGrounded;
        public Vector3 HorizontalVelocity => velocity;
        public float CurrentSpeed => walkSpeed;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            cachedTransform = transform;
        }

        /// <summary>
        /// 核心移动 — 每帧由 PlayerController 调用
        /// </summary>
        /// <param name="inputDir">世界空间移动方向（已归一化）</param>
        /// <param name="sprint">是否冲刺</param>
        /// <param name="jumpPressed">本帧是否按下跳跃</param>
        public void Move(Vector3 inputDir, bool sprint, bool jumpPressed)
        {
            UpdateGroundState();
            HandleJump(jumpPressed);
            ApplyGravity();
            ApplyHorizontalMovement(inputDir, sprint);
            ApplyFinalMotion();
        }

        #region 地面检测

        private void UpdateGroundState()
        {
            bool wasGrounded = isGrounded;
            isGrounded = controller.isGrounded;

            if (isGrounded && !wasGrounded)
            {
                // 着陆时重置垂直速度，防止从高处落地后反弹
                if (verticalVelocity < -2f)
                    verticalVelocity = -2f;
            }

            if (isGrounded)
                lastGroundedTime = Time.time;
        }

        /// <summary>是否在 coyote time 窗口内</summary>
        private bool WithinCoyoteTime => Time.time - lastGroundedTime <= coyoteTime;

        #endregion

        #region 跳跃

        private void HandleJump(bool jumpPressed)
        {
            if (jumpPressed)
                lastJumpPressedTime = Time.time;

            bool canJump = (isGrounded || WithinCoyoteTime) && !jumpConsumed;
            bool wantJump = Time.time - lastJumpPressedTime <= jumpBufferTime;

            if (canJump && wantJump)
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
                jumpConsumed = true;
                isGrounded = false;
                lastGroundedTime = float.MinValue;
            }

            if (isGrounded)
                jumpConsumed = false;
        }

        #endregion

        #region 重力

        private void ApplyGravity()
        {
            if (isGrounded && verticalVelocity < 0)
            {
                // 贴地时用小的向下力保证贴地（处理下坡）
                verticalVelocity = -groundStickForce;
            }
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
            }
        }

        #endregion

        #region 水平移动

        private void ApplyHorizontalMovement(Vector3 inputDir, bool sprint)
        {
            float targetSpeed = sprint ? walkSpeed * sprintMultiplier : walkSpeed;
            float accel = isGrounded ? acceleration : airAcceleration;

            // 计算目标速度
            Vector3 targetVelocity = inputDir * targetSpeed;

            // 平滑加速/减速
            velocity = Vector3.MoveTowards(velocity, targetVelocity, accel * Time.deltaTime);
        }

        #endregion

        #region 最终位移

        private void ApplyFinalMotion()
        {
            Vector3 motion = (velocity + Vector3.up * verticalVelocity) * Time.deltaTime;
            controller.Move(motion);
        }

        #endregion

        #region 扩展API

        /// <summary>添加瞬时速度（用于冲刺、弹跳等技能）</summary>
        public void AddImpulse(Vector3 impulse)
        {
            velocity += new Vector3(impulse.x, 0, impulse.z);
            verticalVelocity += impulse.y;
        }

        /// <summary>设置垂直速度（用于飞行等技能）</summary>
        public void SetVerticalVelocity(float value)
        {
            verticalVelocity = value;
        }

        /// <summary>强制脱离地面</summary>
        public void DetachFromGround()
        {
            isGrounded = false;
            lastGroundedTime = float.MinValue;
            if (verticalVelocity < 0) verticalVelocity = 0;
        }

        /// <summary>获取加速度（用于动画）</summary>
        public float GetCurrentAcceleration()
        {
            return isGrounded ? acceleration : airAcceleration;
        }

        /// <summary>传送</summary>
        public void Teleport(Vector3 position)
        {
            controller.enabled = false;
            cachedTransform.position = position;
            controller.enabled = true;
            velocity = Vector3.zero;
            verticalVelocity = 0;
        }

        #endregion
    }
}
