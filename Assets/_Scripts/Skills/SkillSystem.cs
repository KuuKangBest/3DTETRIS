using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TDTTetris.Skills
{
    /// <summary>
    /// 技能系统 — 统一管理所有玩家技能
    /// 支持技能注册、冷却管理、升级
    /// </summary>
    public class SkillSystem : MonoBehaviour
    {
        [Header("技能列表")]
        [SerializeField] private List<SkillSlot> skillSlots = new List<SkillSlot>();

        // 已注册的技能
        private Dictionary<string, SkillSlot> skillRegistry = new Dictionary<string, SkillSlot>();

        // 事件
        public UnityEvent<string> OnSkillActivated;
        public UnityEvent<string> OnSkillReady;
        public UnityEvent<string, float> OnCooldownUpdated; // skillID, remainingRatio

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var slot in skillSlots)
            {
                if (slot.Skill != null)
                {
                    slot.Skill.Tick(dt);
                    if (!slot.Skill.IsReady)
                        slot.CooldownRemaining -= dt;
                }
            }
        }

        /// <summary>
        /// 激活指定技能
        /// </summary>
        public bool ActivateSkill(string skillID)
        {
            if (!skillRegistry.TryGetValue(skillID, out var slot)) return false;
            if (slot.Skill == null || !slot.Skill.IsReady) return false;
            if (slot.CooldownRemaining > 0) return false;

            slot.Skill.Activate();
            slot.CooldownRemaining = slot.Skill.Cooldown;
            OnSkillActivated?.Invoke(skillID);

            return true;
        }

        /// <summary>
        /// 按索引激活技能
        /// </summary>
        public bool ActivateSkillByIndex(int index)
        {
            if (index < 0 || index >= skillSlots.Count) return false;
            var slot = skillSlots[index];
            return ActivateSkill(slot.Skill.SkillID);
        }

        /// <summary>
        /// 注册新技能
        /// </summary>
        public void RegisterSkill(ISkill skill, KeyCode hotkey = KeyCode.None)
        {
            if (skillRegistry.ContainsKey(skill.SkillID))
            {
                Debug.LogWarning($"SkillSystem: 技能 '{skill.SkillID}' 已注册");
                return;
            }

            var slot = new SkillSlot
            {
                Skill = skill,
                Hotkey = hotkey,
                CooldownRemaining = 0f,
                Level = 1
            };

            skillSlots.Add(slot);
            skillRegistry[skill.SkillID] = slot;
        }

        /// <summary>
        /// 移除技能
        /// </summary>
        public void UnregisterSkill(string skillID)
        {
            if (skillRegistry.TryGetValue(skillID, out var slot))
            {
                skillSlots.Remove(slot);
                skillRegistry.Remove(skillID);
            }
        }

        /// <summary>
        /// 升级技能
        /// </summary>
        public void UpgradeSkill(string skillID)
        {
            if (skillRegistry.TryGetValue(skillID, out var slot))
                slot.Level++;
        }

        /// <summary>
        /// 获取技能冷却剩余比例 (0-1)
        /// </summary>
        public float GetCooldownRatio(string skillID)
        {
            if (skillRegistry.TryGetValue(skillID, out var slot) && slot.Skill != null)
                return slot.Skill.Cooldown > 0 ? slot.CooldownRemaining / slot.Skill.Cooldown : 0;
            return 0;
        }
    }

    /// <summary>
    /// 技能槽位 — 存储技能实例及其状态
    /// </summary>
    [System.Serializable]
    public class SkillSlot
    {
        public ISkill Skill;
        public KeyCode Hotkey;
        public float CooldownRemaining;
        public int Level;
    }
}
