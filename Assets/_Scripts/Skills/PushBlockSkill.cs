using UnityEngine;

namespace TDTTetris.Skills
{
    /// <summary>
    /// 推方块技能 — 将前方方块沿玩家视线方向推动一格
    /// 示例技能，展示ISkill接口的用法
    /// </summary>
    public class PushBlockSkill : MonoBehaviour, ISkill
    {
        [Header("技能参数")]
        [SerializeField] private string skillID = "push_block";
        [SerializeField] private string displayName = "推动方块";
        [SerializeField] private string description = "将前方方块沿视线方向推动一格";
        [SerializeField] private float cooldown = 3f;
        [SerializeField] private float range = 4f;

        [Header("引用")]
        [SerializeField] private Player.PlayerController player;

        private float cooldownRemaining;

        public string SkillID => skillID;
        public string DisplayName => displayName;
        public string Description => description;
        public float Cooldown => cooldown;

        public bool IsReady => cooldownRemaining <= 0f;

        public void Activate()
        {
            if (!IsReady) return;

            var cam = Camera.main;
            if (cam == null) return;

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range))
            {
                var block = hit.collider.GetComponent<Core.Block>();
                if (block != null && block.IsActive)
                {
                    var pushDir = cam.transform.forward;
                    pushDir.y = 0;
                    pushDir.Normalize();

                    var gridDir = new Vector3Int(
                        Mathf.RoundToInt(pushDir.x),
                        0,
                        Mathf.RoundToInt(pushDir.z)
                    );

                    if (block.TryMove(gridDir))
                    {
                        cooldownRemaining = cooldown;
                        Debug.Log($"[PushBlockSkill] 方块已推动到 {block.GridPosition}");
                    }
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (cooldownRemaining > 0)
                cooldownRemaining -= deltaTime;
        }

        public void ResetCooldown()
        {
            cooldownRemaining = 0f;
        }
    }
}
