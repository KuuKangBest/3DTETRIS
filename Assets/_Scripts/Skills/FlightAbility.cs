using UnityEngine;

namespace TDTTetris.Skills
{
    /// <summary>
    /// 喷气飞行 — 长按消耗能量推动角色
    /// 能量不足时自动停止，不飞行时自动回复
    /// </summary>
    public class FlightAbility : MonoBehaviour
    {
        [Header("飞行参数")]
        [SerializeField] private KeyCode flyKey = KeyCode.Space;
        [SerializeField] private float thrustForce = 15f;        // 推力
        [SerializeField] private float maxSpeed = 12f;           // 最大飞行速度

        [Header("能量")]
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float drainRate = 35f;         // 每秒消耗
        [SerializeField] private float rechargeRate = 20f;       // 每秒回复
        [SerializeField] private float rechargeDelay = 0.5f;    // 停止飞行后延迟回复

        private float currentEnergy;
        private float rechargeTimer;
        private Player.PlayerMotor motor;
        private CharacterController controller;
        private Camera playerCamera;
        private bool isFlying;

        // 缓动能量显示值
        private float displayEnergy;
        public float EnergyRatio => displayEnergy / maxEnergy;
        public bool IsFlying => isFlying;

        private void Awake()
        {
            motor = GetComponent<Player.PlayerMotor>();
            controller = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null) playerCamera = Camera.main;
            currentEnergy = maxEnergy;
            displayEnergy = maxEnergy;
        }

        private void Update()
        {
            HandleRecharge();
            HandleFlight();
            SmoothDisplayEnergy();
        }

        private void HandleRecharge()
        {
            if (!isFlying)
            {
                rechargeTimer += Time.deltaTime;
                if (rechargeTimer >= rechargeDelay)
                {
                    currentEnergy = Mathf.Min(currentEnergy + rechargeRate * Time.deltaTime, maxEnergy);
                }
            }
        }

        private void HandleFlight()
        {
            bool wantFly = Input.GetKey(flyKey) && currentEnergy > 0;

            if (wantFly)
            {
                isFlying = true;
                rechargeTimer = 0;
                currentEnergy -= drainRate * Time.deltaTime;

                // 推力方向 = 相机前方
                Vector3 thrustDir;
                if (playerCamera != null)
                {
                    thrustDir = playerCamera.transform.forward;
                    thrustDir.y = 0;
                    thrustDir.Normalize();
                }
                else
                {
                    thrustDir = transform.forward;
                }

                // 向上分量
                thrustDir.y = 0.4f; // 轻微上升
                thrustDir.Normalize();

                motor.AddImpulse(thrustDir * thrustForce * Time.deltaTime);
            }
            else
            {
                isFlying = false;
            }
        }

        private void SmoothDisplayEnergy()
        {
            float speed = (isFlying ? 8f : 3f); // 消耗快显示也快，回复慢显示缓
            displayEnergy = Mathf.Lerp(displayEnergy, currentEnergy, speed * Time.deltaTime);
        }
    }
}
