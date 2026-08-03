using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TDTTetris.UI
{
    /// <summary>
    /// 游戏UI — HUD显示
    /// 负责分数、状态、技能冷却等信息的展示
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("核心引用")]
        [SerializeField] private Core.GameManager gameManager;
        [SerializeField] private Skills.SkillSystem skillSystem;

        [Header("主面板")]
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;

        [Header("信息显示")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text eliminatedText;
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private TMP_Text stateText;

        [Header("技能冷却")]
        [SerializeField] private Slider skill1Cooldown;
        [SerializeField] private Slider skill2Cooldown;
        [SerializeField] private Slider skill3Cooldown;
        [SerializeField] private Slider skill4Cooldown;

        [Header("游戏结束")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<Core.GameManager>();

            if (gameManager != null)
            {
                gameManager.OnStateChanged.AddListener(OnGameStateChanged);
                gameManager.OnScoreChanged.AddListener(OnScoreChanged);
                gameManager.OnEliminated.AddListener(OnEliminated);
            }

            // 初始状态
            if (hudPanel != null) hudPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(false);
            if (gameOverPanel != null) gameOverPanel.SetActive(false);

            if (restartButton != null)
                restartButton.onClick.AddListener(() => gameManager?.StartGame());
            if (quitButton != null)
                quitButton.onClick.AddListener(() => Application.Quit());
        }

        private void Update()
        {
            if (gameManager != null && gameManager.CurrentState == Core.GameState.Playing)
            {
                UpdateTimeDisplay();
                UpdateSkillCooldowns();
            }

            // 暂停快捷键
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameManager?.TogglePause();
            }
        }

        private void OnGameStateChanged(Core.GameState state)
        {
            if (hudPanel != null)
                hudPanel.SetActive(state == Core.GameState.Playing);

            if (pausePanel != null)
                pausePanel.SetActive(state == Core.GameState.Paused);

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(state == Core.GameState.GameOver);
                if (state == Core.GameState.GameOver && finalScoreText != null)
                    finalScoreText.text = $"最终得分: {gameManager.Score}\n消除: {gameManager.EliminatedCount}";
            }

            if (stateText != null)
                stateText.text = state.ToString();
        }

        private void OnScoreChanged(int score)
        {
            if (scoreText != null)
                scoreText.text = $"得分: {score}";
        }

        private void OnEliminated(int count)
        {
            if (eliminatedText != null)
                eliminatedText.text = $"消除: {gameManager.EliminatedCount}";
        }

        private void UpdateTimeDisplay()
        {
            if (timeText != null && gameManager != null)
            {
                int mins = Mathf.FloorToInt(gameManager.GameTime / 60);
                int secs = Mathf.FloorToInt(gameManager.GameTime % 60);
                timeText.text = $"时间: {mins:D2}:{secs:D2}";
            }
        }

        private void UpdateSkillCooldowns()
        {
            if (skillSystem == null) return;

            var sliders = new[] { skill1Cooldown, skill2Cooldown, skill3Cooldown, skill4Cooldown };
            for (int i = 0; i < sliders.Length; i++)
            {
                if (sliders[i] != null)
                {
                    // 获取第i个技能的冷却 — 简化处理用索引
                    float ratio = 0;
                    // TODO: 通过skillSystem.GetCooldownRatioByIndex(i) 获取
                    sliders[i].value = 1f - ratio;
                }
            }
        }
    }
}
