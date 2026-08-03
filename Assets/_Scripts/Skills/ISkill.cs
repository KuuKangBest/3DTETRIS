namespace TDTTetris.Skills
{
    /// <summary>
    /// 技能接口 — 所有技能必须实现
    /// 技能系统通过此接口统一管理技能的激活、冷却、升级
    /// </summary>
    public interface ISkill
    {
        /// <summary>技能唯一标识</summary>
        string SkillID { get; }

        /// <summary>显示名称</summary>
        string DisplayName { get; }

        /// <summary>技能描述</summary>
        string Description { get; }

        /// <summary>冷却时间（秒）</summary>
        float Cooldown { get; }

        /// <summary>当前是否可用</summary>
        bool IsReady { get; }

        /// <summary>激活技能</summary>
        void Activate();

        /// <summary>每帧更新（用于持续型技能）</summary>
        void Tick(float deltaTime);

        /// <summary>重置冷却</summary>
        void ResetCooldown();
    }
}
