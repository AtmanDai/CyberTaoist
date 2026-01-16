using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// 游戏管理器 - 管理游戏全局状态和流程
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    public bool isGameOver = false;
    public bool isVictory = false;
    public bool isPaused = false;

    [Header("Player Reference")]
    public PlayerCombatController player;

    [Header("Boss Reference")]
    public BossController boss;

    [Header("Managers")]
    public EnemySpawner enemySpawner;
    public GameUI gameUI;

    // 事件
    public event Action OnGameStart;
    public event Action OnGameOver;
    public event Action OnVictory;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// 初始化游戏
    /// </summary>
    private void InitializeGame()
    {
        isGameOver = false;
        isVictory = false;
        isPaused = false;
        Time.timeScale = 1f;

        // 查找引用
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerCombatController>();
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if (gameUI == null)
        {
            gameUI = FindFirstObjectByType<GameUI>();
        }

        // 订阅事件
        SubscribeToEvents();

        OnGameStart?.Invoke();
        Debug.Log("[GameManager] 游戏开始!");
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        // 玩家死亡事件
        if (player != null)
        {
            player.OnPlayerDeath += HandlePlayerDeath;
        }

        // Boss生成时订阅死亡事件
        // Boss会在EnemySpawner中生成，需要动态订阅
    }

    void Update()
    {
        // 检查Boss是否存在
        if (boss == null)
        {
            boss = FindFirstObjectByType<BossController>();
            if (boss != null)
            {
                boss.OnBossDeath += HandleBossDefeated;
                
                // 切换到Boss战BGM
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayBossBGM();
                }

                // 显示Boss血条
                if (gameUI != null)
                {
                    gameUI.ShowBossHealth(boss.currentHealth, boss.maxHealth);
                }
            }
        }

        // 更新Boss血条
        if (boss != null && gameUI != null)
        {
            gameUI.UpdateBossHealth(boss.currentHealth, boss.maxHealth);
        }

        // 更新玩家UI
        if (player != null && gameUI != null)
        {
            gameUI.UpdatePlayerHealth(player.currentHealth, player.maxHealth);
            gameUI.UpdatePlayerEnergy(player.currentEnergy, player.maxEnergy);
        }
    }

    /// <summary>
    /// 处理玩家死亡
    /// </summary>
    private void HandlePlayerDeath()
    {
        if (isGameOver) return;

        isGameOver = true;
        isVictory = false;

        Debug.Log("[GameManager] 玩家阵亡，游戏结束!");

        // 停止敌人生成
        if (enemySpawner != null)
        {
            enemySpawner.SetAutoSpawn(false);
        }

        // 显示失败界面
        if (gameUI != null)
        {
            gameUI.ShowDefeatPanel();
        }

        // 播放失败音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeat();
        }

        OnGameOver?.Invoke();
    }

    /// <summary>
    /// 处理Boss被击败
    /// </summary>
    private void HandleBossDefeated()
    {
        if (isGameOver) return;

        isGameOver = true;
        isVictory = true;

        Debug.Log("[GameManager] Boss被击败，胜利!");

        // 停止敌人生成
        if (enemySpawner != null)
        {
            enemySpawner.SetAutoSpawn(false);
        }

        // 清除所有敌人
        if (enemySpawner != null)
        {
            enemySpawner.ClearAllEnemies();
        }

        // 隐藏Boss血条
        if (gameUI != null)
        {
            gameUI.HideBossHealth();
            gameUI.ShowVictoryPanel();
        }

        // 播放胜利音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictory();
        }

        OnVictory?.Invoke();
    }

    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        if (isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;

        if (gameUI != null)
        {
            gameUI.ShowPausePanel();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseBGM();
        }

        OnGamePaused?.Invoke();
    }

    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeBGM();
        }

        OnGameResumed?.Invoke();
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[GameManager] 退出游戏");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 获取游戏统计
    /// </summary>
    public GameStats GetGameStats()
    {
        return new GameStats
        {
            playerHealth = player != null ? player.currentHealth : 0,
            bossHealth = boss != null ? boss.currentHealth : 0,
            enemiesDefeated = enemySpawner != null ? 0 : 0, // 需要跟踪
            isVictory = isVictory
        };
    }
}

/// <summary>
/// 游戏统计数据
/// </summary>
[System.Serializable]
public struct GameStats
{
    public float playerHealth;
    public float bossHealth;
    public int enemiesDefeated;
    public bool isVictory;
}
