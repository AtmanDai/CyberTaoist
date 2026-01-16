using UnityEngine;
using System;

/// <summary>
/// 游戏状态管理器 - 控制游戏的不同阶段状态
/// </summary>
public enum GameState
{
    Normal,           // 键鼠战斗
    DomainExpansion,  // 结印中(慢动作)
    SpellCast,        // 术式演出
    Paused            // 暂停菜单
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Current State")]
    [SerializeField] private GameState currentState = GameState.Normal;
    public GameState CurrentState => currentState;

    // 状态变化事件
    public event Action<GameState, GameState> OnStateChanged;

    [Header("Time Scale Settings")]
    public float domainTimeScale = 0.5f;   // 领域展开时的慢动作倍率
    public float spellCastTimeScale = 0f;  // 术式演出时暂停
    public float normalTimeScale = 1f;

    private float previousTimeScale = 1f;

    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetState(GameState.Normal);
    }

    /// <summary>
    /// 切换游戏状态
    /// </summary>
    public void SetState(GameState newState)
    {
        if (currentState == newState) return;

        GameState previousState = currentState;
        currentState = newState;

        // 应用状态效果
        ApplyStateEffects(newState);

        // 触发状态变化事件
        OnStateChanged?.Invoke(previousState, newState);

        Debug.Log($"[GameState] {previousState} → {newState}");
    }

    private void ApplyStateEffects(GameState state)
    {
        switch (state)
        {
            case GameState.Normal:
                Time.timeScale = normalTimeScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                break;

            case GameState.DomainExpansion:
                Time.timeScale = domainTimeScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;
                break;

            case GameState.SpellCast:
                previousTimeScale = Time.timeScale;
                Time.timeScale = spellCastTimeScale;
                Time.fixedDeltaTime = 0.02f;
                break;

            case GameState.Paused:
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
                Time.fixedDeltaTime = 0.02f;
                break;
        }
    }

    /// <summary>
    /// 检查是否可以进入领域展开
    /// </summary>
    public bool CanEnterDomain()
    {
        return currentState == GameState.Normal;
    }

    /// <summary>
    /// 检查是否可以进行结印操作
    /// </summary>
    public bool CanPerformSeal()
    {
        return currentState == GameState.DomainExpansion;
    }

    /// <summary>
    /// 暂停/继续游戏
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            SetState(GameState.Normal);
        }
        else if (currentState == GameState.Normal)
        {
            SetState(GameState.Paused);
        }
    }

    void Update()
    {
        // ESC键暂停
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }
}
