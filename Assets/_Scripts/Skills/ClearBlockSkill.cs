using System.Collections.Generic;
using UnityEngine;

namespace TDTTetris.Skills
{
    /// <summary>
    /// 清除方块技能 — 清除前方小范围内的方块
    /// 示例技能，展示ISkill接口的用法
    /// </summary>
    public class ClearBlockSkill : MonoBehaviour, ISkill
    {
        [Header("技能参数")]
        [SerializeField] private string skillID = "clear_block";
        [SerializeField] private string displayName = "清除方块";
        [SerializeField] private string description = "清除前方小范围内的方块";
        [SerializeField] private float cooldown = 8f;
        [SerializeField] private float range = 3f;
        [SerializeField] private float clearRadius = 1.5f;

        [Header("引用")]
        [SerializeField] private Core.Board board;
        [SerializeField] private Core.GameManager gameManager;

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

            // 射线检测命中点
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range))
            {
                if (board == null)
                    board = FindObjectOfType<Core.Board>();

                // 在世界空间中查找范围内的方块
                var toClear = new List<Vector3Int>();
                var worldCenter = hit.point;

                for (int x = 0; x < board.Width; x++)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        for (int z = 0; z < board.Depth; z++)
                        {
                            var cell = board.GetCell(x, y, z);
                            if (!cell.IsOccupied) continue;

                            var cellWorld = board.GridToWorld(x, y, z);
                            if (Vector3.Distance(cellWorld, worldCenter) <= clearRadius)
                            {
                                toClear.Add(new Vector3Int(x, y, z));
                            }
                        }
                    }
                }

                if (toClear.Count > 0)
                {
                    if (gameManager == null)
                        gameManager = FindObjectOfType<Core.GameManager>();

                    gameManager?.EliminateCells(toClear);
                    cooldownRemaining = cooldown;
                    Debug.Log($"[ClearBlockSkill] 清除了 {toClear.Count} 个方块");
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
