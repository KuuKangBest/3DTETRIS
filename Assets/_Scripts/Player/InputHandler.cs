using UnityEngine;

namespace TDTTetris.Player
{
    /// <summary>
    /// 输入处理器 — 收集输入，与移动逻辑解耦
    /// 方便后续接入 Input System 包或自定义按键映射
    /// </summary>
    public class InputHandler : MonoBehaviour
    {
        [Header("按键绑定 (可配置)")]
        [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode skill1Key = KeyCode.Alpha1;
        [SerializeField] private KeyCode skill2Key = KeyCode.Alpha2;
        [SerializeField] private KeyCode skill3Key = KeyCode.Alpha3;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        // 输出
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool InteractPressed { get; private set; }
        public int SkillPressed { get; private set; } // 0=无, 1-4=技能

        // 鼠标锁定
        public bool CursorLocked { get; private set; } = true;

        private void Start()
        {
            LockCursor(true);
        }

        private void Update()
        {
            ReadInput();
        }

        private void ReadInput()
        {
            // 鼠标锁定切换
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.BackQuote))
                LockCursor(!CursorLocked);

            if (!CursorLocked)
            {
                MoveInput = Vector2.zero;
                LookInput = Vector2.zero;
                SprintHeld = false;
                JumpPressed = false;
                InteractPressed = false;
                SkillPressed = 0;
                return;
            }

            // 移动输入 (WASD)
            MoveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            MoveInput = Vector2.ClampMagnitude(MoveInput, 1f); // 限制对角线速度

            // 视角
            LookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // 按钮
            SprintHeld = Input.GetKey(sprintKey);
            JumpPressed = Input.GetKeyDown(jumpKey);
            InteractPressed = Input.GetKeyDown(interactKey);

            // 技能
            SkillPressed = 0;
            if (Input.GetKeyDown(skill1Key)) SkillPressed = 1;
            else if (Input.GetKeyDown(skill2Key)) SkillPressed = 2;
            else if (Input.GetKeyDown(skill3Key)) SkillPressed = 3;
        }

        public void LockCursor(bool locked)
        {
            CursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
