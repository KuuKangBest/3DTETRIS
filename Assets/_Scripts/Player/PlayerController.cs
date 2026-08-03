using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 第三人称玩家控制器
    /// 在棋盘上行走、跳跃、飞行/喷射（技能扩展点）
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float sprintSpeed = 8f;
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = 15f;

        [Header("飞行/喷射 (技能扩展)")]
        [SerializeField] private bool canFly = false;
        [SerializeField] private float flySpeed = 6f;
        [SerializeField] private float maxFlyTime = 3f;
        [SerializeField] private float flyCooldown = 5f;
        private float currentFlyTime;
        private float flyCooldownTimer;
        private bool isFlying;

        [Header("视角")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 60f;

        [Header("交互")]
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private float pushForce = 10f;

        // 组件引用
        private CharacterController controller;
        private Camera playerCamera;

        // 内部状态
        private Vector3 velocity;
        private float pitch;
        private float yaw;
        private bool isGrounded;

        // 属性（供技能系统读取）
        public bool IsGrounded => isGrounded;
        public bool IsFlying => isFlying;
        public bool CanFly { get => canFly; set => canFly = value; }
        public CharacterController Controller => controller;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();

            if (playerCamera == null)
                playerCamera = Camera.main;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (playerCamera == null) return;

            HandleLook();
            HandleMovement();
            HandleFlight();
            HandleInteraction();
        }

        #region 视角控制

        private void HandleLook()
        {
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0, yaw, 0);
            if (playerCamera != null)
                playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        #endregion

        #region 移动控制

        private void HandleMovement()
        {
            // 地面检测
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
                velocity.y = -2f; // 保持贴地

            // 输入
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            var moveDir = transform.right * horizontal + transform.forward * vertical;
            moveDir.Normalize();

            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
            controller.Move(speed * Time.deltaTime * moveDir);

            // 跳跃
            if (Input.GetButtonDown("Jump") && isGrounded && !isFlying)
            {
                velocity.y = Mathf.Sqrt(jumpForce * 2f * gravity);
            }

            // 如果不是飞行状态，应用重力
            if (!isFlying)
            {
                velocity.y -= gravity * Time.deltaTime;
            }

            controller.Move(velocity * Time.deltaTime);
        }

        #endregion

        #region 飞行/喷射（技能系统扩展点）

        private void HandleFlight()
        {
            if (!canFly) return;

            // 冷却计时
            if (!isFlying && flyCooldownTimer > 0)
                flyCooldownTimer -= Time.deltaTime;

            // 激活飞行
            if (Input.GetKeyDown(KeyCode.F) && !isFlying && flyCooldownTimer <= 0)
            {
                StartFlying();
            }

            if (isFlying)
            {
                currentFlyTime -= Time.deltaTime;

                // 飞行移动
                float upDown = 0;
                if (Input.GetKey(KeyCode.Space)) upDown = 1;
                if (Input.GetKey(KeyCode.LeftControl)) upDown = -1;

                var flyDir = transform.forward * Input.GetAxis("Vertical")
                           + transform.right * Input.GetAxis("Horizontal")
                           + Vector3.up * upDown;
                flyDir.Normalize();

                controller.Move(flySpeed * Time.deltaTime * flyDir);
                velocity.y = 0;

                // 飞行时间耗尽或手动取消
                if (currentFlyTime <= 0 || Input.GetKeyDown(KeyCode.F))
                    StopFlying();
            }
        }

        private void StartFlying()
        {
            isFlying = true;
            currentFlyTime = maxFlyTime;
            velocity.y = 0;
            // 可在此触发飞行特效
        }

        private void StopFlying()
        {
            isFlying = false;
            flyCooldownTimer = flyCooldown;
            // 可在此触发落地特效
        }

        /// <summary>
        /// 解锁飞行能力（技能系统调用）
        /// </summary>
        public void UnlockFlight(float duration = -1f, float cooldown = -1f)
        {
            canFly = true;
            if (duration > 0) maxFlyTime = duration;
            if (cooldown > 0) flyCooldown = cooldown;
        }

        #endregion

        #region 交互

        private void HandleInteraction()
        {
            // 射线检测前方方块
            if (Input.GetMouseButtonDown(0)) // 左键
            {
                TryPushBlock();
            }

            if (Input.GetMouseButtonDown(1)) // 右键
            {
                TryDestroyBlock();
            }
        }

        /// <summary>
        /// 尝试推动前方方块
        /// </summary>
        private void TryPushBlock()
        {
            if (playerCamera == null) return;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                out var hit, interactionRange))
            {
                var block = hit.collider.GetComponent<TDTTetris.Core.Block>();
                if (block != null && block.IsActive)
                {
                    var pushDir = playerCamera.transform.forward;
                    pushDir.y = 0;
                    pushDir.Normalize();
                    var gridDir = new Vector3Int(
                        Mathf.RoundToInt(pushDir.x),
                        0,
                        Mathf.RoundToInt(pushDir.z)
                    );
                    block.TryMove(gridDir);
                }
            }
        }

        /// <summary>
        /// 尝试消除前方方块（技能）
        /// </summary>
        private void TryDestroyBlock()
        {
            if (playerCamera == null) return;

            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward,
                out var hit, interactionRange))
            {
                // 检查是否是已放置的方块（非活跃方块）
                var block = hit.collider.GetComponent<TDTTetris.Core.Block>();
                if (block != null && !block.IsActive)
                {
                    // TODO: 通过技能系统处理
                    Debug.Log("消除方块（技能系统接口）");
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 传送玩家到棋盘上的指定位置
        /// </summary>
        public void Teleport(Vector3 worldPosition)
        {
            controller.enabled = false;
            transform.position = worldPosition;
            controller.enabled = true;
        }

        /// <summary>
        /// 设置玩家可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
                r.enabled = visible;
        }

        #endregion
    }
}
