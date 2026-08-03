using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 角色物理马达 — 纯移动逻辑
    /// 速度以格/秒为单位，默认一格 = 1 世界单位
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("移动 (格/秒)")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float acceleration = 15f;
        [SerializeField] private float airAcceleration = 5f;

        [Header("跳跃")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float coyoteTime = 0.12f;
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("重力")]
        [SerializeField] private float gravity = 25f;
        [SerializeField] private float maxFallSpeed = 40f;
        [SerializeField] private float groundStickForce = 8f;

        private CharacterController controller;
        private Transform cachedTransform;
        private Vector3 velocity;
        private float verticalVelocity;
        private bool isGrounded;
        private float lastGroundedTime;
        private float lastJumpPressedTime;
        private bool jumpConsumed;
        private Core.GameConfig config;

        public bool IsGrounded => isGrounded;
        public Vector3 Velocity => velocity + Vector3.up * verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            cachedTransform = transform;
            config = Resources.Load<Core.GameConfig>("GameConfig");
            if (config != null)
            {
                walkSpeed = config.PlayerMoveSpeed;
                gravity = config.PlayerGravity;
            }
        }

        public void Move(Vector3 inputDir, bool sprint, bool jumpPressed)
        {
            UpdateGround();
            HandleJump(jumpPressed);
            ApplyGravity();
            ApplyHorizontal(inputDir, sprint);
            ApplyMotion();
        }

        private void UpdateGround()
        {
            bool wasGrounded = isGrounded;
            isGrounded = controller.isGrounded;

            if (isGrounded)
            {
                lastGroundedTime = Time.time;
                if (!wasGrounded && verticalVelocity < -2f)
                    verticalVelocity = -2f; // 落地缓冲
            }
        }

        private void HandleJump(bool jumpPressed)
        {
            if (jumpPressed) lastJumpPressedTime = Time.time;

            bool coyoteOk = Time.time - lastGroundedTime <= coyoteTime;
            bool bufferOk = Time.time - lastJumpPressedTime <= jumpBufferTime;
            bool canJump = (isGrounded || coyoteOk) && !jumpConsumed;

            if (canJump && bufferOk)
            {
                verticalVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);
                jumpConsumed = true;
                isGrounded = false;
                lastGroundedTime = float.MinValue;
            }

            if (isGrounded) jumpConsumed = false;
        }

        private void ApplyGravity()
        {
            if (isGrounded && verticalVelocity < 0)
                verticalVelocity = -groundStickForce;
            else
            {
                verticalVelocity -= gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
            }
        }

        private void ApplyHorizontal(Vector3 inputDir, bool sprint)
        {
            float targetSpeed = sprint ? walkSpeed * sprintMultiplier : walkSpeed;
            float accel = isGrounded ? acceleration : airAcceleration;
            Vector3 targetVel = inputDir * targetSpeed;
            velocity = Vector3.MoveTowards(velocity, targetVel, accel * Time.deltaTime);
        }

        private void ApplyMotion()
        {
            controller.Move((velocity + Vector3.up * verticalVelocity) * Time.deltaTime);
        }

        // 扩展 API
        public void AddImpulse(Vector3 impulse)
        {
            velocity += new Vector3(impulse.x, 0, impulse.z);
            verticalVelocity += impulse.y;
        }
        public void SetVerticalVelocity(float v) => verticalVelocity = v;
        public void DetachFromGround() { isGrounded = false; lastGroundedTime = float.MinValue; if (verticalVelocity < 0) verticalVelocity = 0; }
        public void Teleport(Vector3 pos)
        {
            controller.enabled = false;
            cachedTransform.position = pos;
            controller.enabled = true;
            velocity = Vector3.zero;
            verticalVelocity = 0;
        }
    }
}
