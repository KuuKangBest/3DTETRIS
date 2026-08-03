using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 玩家控制器（编排层）— 协调 InputHandler / PlayerMotor / CameraController
    /// 和 SkillSystem，不直接处理物理或输入细节
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("组件引用")]
        [SerializeField] private InputHandler input;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private CameraController cameraController;

        [Header("角色朝向")]
        [SerializeField] private float rotationSpeed = 720f;  // 度/秒

        [Header("技能扩展")]
        [SerializeField] private Skills.SkillSystem skillSystem;

        // 属性
        public PlayerMotor Motor => motor;
        public CameraController CameraCtrl => cameraController;
        public bool IsSprinting => input != null && input.SprintHeld;
        public bool IsGrounded => motor != null && motor.IsGrounded;

        private void Awake()
        {
            // 自动查找组件
            if (input == null) input = GetComponent<InputHandler>();
            if (motor == null) motor = GetComponent<PlayerMotor>();
            if (cameraController == null) cameraController = GetComponentInChildren<CameraController>();
            if (skillSystem == null) skillSystem = GetComponent<Skills.SkillSystem>();
        }

        private void Update()
        {
            if (input == null || motor == null) return;

            HandleMovement();
            HandleRotation();
            HandleSkills();
        }

        private void HandleMovement()
        {
            // 将 2D 输入转为相机相对方向
            Vector3 moveDir = Vector3.zero;
            if (cameraController != null && input.MoveInput.sqrMagnitude > 0.01f)
            {
                Transform cam = cameraController.transform;
                Vector3 forward = cam.forward;
                Vector3 right = cam.right;
                forward.y = 0;
                right.y = 0;
                forward.Normalize();
                right.Normalize();

                moveDir = (forward * input.MoveInput.y + right * input.MoveInput.x).normalized;
            }

            motor.Move(moveDir, input.SprintHeld, input.JumpPressed);
        }

        private void HandleRotation()
        {
            // 角色朝向移动方向（如果有输入的话）
            if (input.MoveInput.sqrMagnitude > 0.01f && cameraController != null)
            {
                Transform cam = cameraController.transform;
                Vector3 forward = cam.forward;
                forward.y = 0;
                if (forward.sqrMagnitude < 0.001f) return;

                forward.Normalize();
                Vector3 right = cam.right;
                right.y = 0;
                right.Normalize();

                Vector3 targetDir = (forward * input.MoveInput.y + right * input.MoveInput.x).normalized;
                Quaternion targetRot = Quaternion.LookRotation(targetDir);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        private void HandleSkills()
        {
            if (skillSystem == null) return;
            if (input.SkillPressed > 0)
                skillSystem.ActivateSkillByIndex(input.SkillPressed - 1);
        }

        /// <summary>传送玩家</summary>
        public void Teleport(Vector3 position)
        {
            motor?.Teleport(position);
        }
    }
}
