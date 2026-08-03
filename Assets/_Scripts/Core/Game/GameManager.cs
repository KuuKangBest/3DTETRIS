using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TDTTetris.Core
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        Waiting,    // 等待开始
        Playing,    // 游戏中
        Paused,     // 暂停
        GameOver    // 结束
    }

    /// <summary>
    /// 游戏管理器 — 核心游戏循环
    /// 负责协调Board、BlockFactory、EliminationSystem
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("配置")]
        [SerializeField] private GameConfig config;

        [Header("核心组件")]
        [SerializeField] private Board board;
        [SerializeField] private BlockFactory blockFactory;
        [SerializeField] private EliminationSystem eliminationSystem;

        [Header("游戏状态")]
        [SerializeField] private GameState currentState = GameState.Waiting;
        [SerializeField] private int score;
        [SerializeField] private int eliminatedCount;
        [SerializeField] private float gameTime;

        // 活跃方块列表
        private List<Block> activeBlocks = new List<Block>();

        // 事件
        public UnityEvent<GameState> OnStateChanged;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent OnBlockPlaced;
        public UnityEvent<int> OnEliminated;   // 消除格子数

        // 属性
        public GameState CurrentState => currentState;
        public int Score => score;
        public int EliminatedCount => eliminatedCount;
        public float GameTime => gameTime;

        private void Start()
        {
            ValidateReferences();
        }

        private void Update()
        {
            if (currentState != GameState.Playing) return;

            gameTime += Time.deltaTime;

            // 检查是否有活跃方块
            bool anyActive = false;
            for (int i = activeBlocks.Count - 1; i >= 0; i--)
            {
                if (activeBlocks[i] == null)
                {
                    activeBlocks.RemoveAt(i);
                    continue;
                }
                if (activeBlocks[i].IsActive)
                    anyActive = true;
            }

            // 没有活跃方块时 → 放置完成 → 消除 → 生成新方块
            if (!anyActive)
            {
                ProcessEliminationAndSpawn();
            }
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            score = 0;
            eliminatedCount = 0;
            gameTime = 0f;
            activeBlocks.Clear();

            ChangeState(GameState.Playing);
            SpawnNewBlock();
        }

        /// <summary>
        /// 暂停/恢复
        /// </summary>
        public void TogglePause()
        {
            if (currentState == GameState.Playing)
                ChangeState(GameState.Paused);
            else if (currentState == GameState.Paused)
                ChangeState(GameState.Playing);
        }

        /// <summary>
        /// 游戏结束
        /// </summary>
        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            Debug.Log($"游戏结束！得分: {score}, 消除: {eliminatedCount}, 时间: {gameTime:F1}s");
        }

        /// <summary>
        /// 生成新方块
        /// </summary>
        private void SpawnNewBlock()
        {
            // 先检查是否有方块到达失败高度
            if (board.HasReachedFailHeight(config.FailHeight))
            {
                GameOver();
                return;
            }

            var block = blockFactory.SpawnBlock();
            if (block != null)
            {
                activeBlocks.Add(block);
            }
            else
            {
                // 无法生成 → 游戏结束
                GameOver();
            }
        }

        /// <summary>
        /// 处理消除逻辑并生成新方块
        /// </summary>
        private void ProcessEliminationAndSpawn()
        {
            // 1. 检查消除
            var toEliminate = eliminationSystem.CheckAllEliminations();
            if (toEliminate.Count > 0)
            {
                // 计分：基础分 + 连消奖励（在消除前检查，因为消除后格子会清空）
                int points = toEliminate.Count * 10;

                // 检查是否有面消除（消除前检查FaceEliminable标志）
                int faceElimCount = 0;
                foreach (var p in toEliminate)
                {
                    var cell = board.GetCell(p);
                    if (cell.IsOccupied && cell.Flags.HasFlag(EliminationFlags.FaceEliminable))
                        faceElimCount++;
                }
                if (faceElimCount >= board.Width * board.Depth)
                    points += 500; // 面消除奖励

                // 执行消除
                int count = eliminationSystem.ExecuteEliminations(toEliminate);
                eliminatedCount += count;
                score += points;

                OnEliminated?.Invoke(count);
                OnScoreChanged?.Invoke(score);
            }

            OnBlockPlaced?.Invoke();

            // 2. 生成新方块（延迟或立即）
            SpawnNewBlock();
        }

        /// <summary>
        /// 手动添加方块（供技能系统调用）
        /// </summary>
        public void AddBlock(Block block)
        {
            if (block != null)
                activeBlocks.Add(block);
        }

        /// <summary>
        /// 手动消除指定位置的格子（供技能系统调用）
        /// </summary>
        public void EliminateCells(List<Vector3Int> cells)
        {
            int count = eliminationSystem.ExecuteEliminations(cells);
            eliminatedCount += count;
            score += count * 15; // 手动消除给更多分
            OnEliminated?.Invoke(count);
            OnScoreChanged?.Invoke(score);
        }

        private void ChangeState(GameState newState)
        {
            currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void ValidateReferences()
        {
            if (config == null)
            {
                config = Resources.Load<GameConfig>("GameConfig");
                if (config != null) Debug.Log("GameManager: 从 Resources 自动加载 GameConfig");
                else Debug.LogError("GameManager: GameConfig 未找到！");
            }
            if (board == null)
                board = FindObjectOfType<Board>();
            if (blockFactory == null)
                blockFactory = FindObjectOfType<BlockFactory>();
            if (eliminationSystem == null)
                eliminationSystem = FindObjectOfType<EliminationSystem>();
        }
    }
}
