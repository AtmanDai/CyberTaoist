using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 主游戏UI控制器 - 管理所有游戏UI元素
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance { get; private set; }

    [Header("Player UI")]
    public Slider healthSlider;
    public Image healthFill;
    public Slider energySlider;
    public Image energyFill;
    public Text playerHealthText;

    [Header("Boss UI")]
    public GameObject bossHealthPanel;
    public Slider bossHealthSlider;
    public Text bossNameText;
    public string bossName = "赛博恶灵·参孙重锤";

    [Header("Game State UI")]
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Combat Feedback")]
    public Text damageText;
    public float damageTextDuration = 1f;

    [Header("Spell Cast UI")]
    public Text spellNameText;
    public Image spellCastOverlay;
    public float spellTextDuration = 2f;

    [Header("QTE UI")]
    public GameObject qtePanel;
    public Text qteSequenceText;
    public Text qteTimerText;
    public Image[] qteProgressIndicators;

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
        // 初始化隐藏
        HideAllPanels();

        // 订阅事件
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast += OnSpellCast;
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast -= OnSpellCast;
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void HideAllPanels()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (bossHealthPanel != null) bossHealthPanel.SetActive(false);
        if (qtePanel != null) qtePanel.SetActive(false);
        if (spellNameText != null) spellNameText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 游戏状态变化回调
    /// </summary>
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Paused:
                ShowPausePanel();
                break;
            case GameState.Normal:
                if (pausePanel != null && pausePanel.activeSelf)
                    pausePanel.SetActive(false);
                break;
            case GameState.SpellCast:
                ShowSpellCastOverlay();
                break;
        }
    }

    /// <summary>
    /// 术式释放回调
    /// </summary>
    private void OnSpellCast(SpellType spellType)
    {
        SpellData data = SpellSystem.Instance?.GetSpellData(spellType);
        if (data != null)
        {
            ShowSpellName(data.spellName);
        }
    }

    #region Player UI

    /// <summary>
    /// 更新玩家血量UI
    /// </summary>
    public void UpdatePlayerHealth(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.value = current / max;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        // 根据血量变色
        if (healthFill != null)
        {
            float percent = current / max;
            healthFill.color = Color.Lerp(Color.red, Color.green, percent);
        }
    }

    /// <summary>
    /// 更新玩家能量UI
    /// </summary>
    public void UpdatePlayerEnergy(float current, float max)
    {
        if (energySlider != null)
        {
            energySlider.value = current / max;
        }

        // 能量满时发光效果
        if (energyFill != null)
        {
            if (current >= max)
            {
                energyFill.color = Color.yellow;
            }
            else
            {
                energyFill.color = Color.cyan;
            }
        }
    }

    #endregion

    #region Boss UI

    /// <summary>
    /// 显示Boss血条
    /// </summary>
    public void ShowBossHealth(float current, float max)
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(true);
        }

        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = current / max;
        }

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }
    }

    /// <summary>
    /// 隐藏Boss血条
    /// </summary>
    public void HideBossHealth()
    {
        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 更新Boss血量
    /// </summary>
    public void UpdateBossHealth(float current, float max)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.value = current / max;
        }
    }

    #endregion

    #region Game State Panels

    /// <summary>
    /// 显示暂停面板
    /// </summary>
    public void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    /// <summary>
    /// 显示胜利面板
    /// </summary>
    public void ShowVictoryPanel()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 显示失败面板
    /// </summary>
    public void ShowDefeatPanel()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);
        }
    }

    #endregion

    #region Combat Feedback

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void ShowDamageNumber(float damage, Vector3 worldPosition)
    {
        if (damageText == null) return;

        // 转换世界坐标到UI坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        damageText.transform.position = screenPos;
        damageText.text = Mathf.CeilToInt(damage).ToString();
        damageText.gameObject.SetActive(true);

        StartCoroutine(HideDamageText());
    }

    private IEnumerator HideDamageText()
    {
        yield return new WaitForSeconds(damageTextDuration);
        if (damageText != null)
        {
            damageText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示术式名称
    /// </summary>
    public void ShowSpellName(string name)
    {
        if (spellNameText == null) return;

        spellNameText.text = name;
        spellNameText.gameObject.SetActive(true);

        StartCoroutine(HideSpellName());
    }

    private IEnumerator HideSpellName()
    {
        yield return new WaitForSecondsRealtime(spellTextDuration);
        if (spellNameText != null)
        {
            spellNameText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 显示术式释放遮罩
    /// </summary>
    public void ShowSpellCastOverlay()
    {
        if (spellCastOverlay != null)
        {
            spellCastOverlay.gameObject.SetActive(true);
            StartCoroutine(HideSpellCastOverlay());
        }
    }

    private IEnumerator HideSpellCastOverlay()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (spellCastOverlay != null)
        {
            spellCastOverlay.gameObject.SetActive(false);
        }
    }

    #endregion

    #region QTE UI

    /// <summary>
    /// 显示QTE面板
    /// </summary>
    public void ShowQTEPanel(string sequence, float timeLimit)
    {
        if (qtePanel != null)
        {
            qtePanel.SetActive(true);
        }

        if (qteSequenceText != null)
        {
            qteSequenceText.text = sequence;
        }

        StartCoroutine(UpdateQTETimer(timeLimit));
    }

    private IEnumerator UpdateQTETimer(float timeLimit)
    {
        float timer = timeLimit;
        while (timer > 0 && qtePanel != null && qtePanel.activeSelf)
        {
            timer -= Time.unscaledDeltaTime;
            if (qteTimerText != null)
            {
                qteTimerText.text = timer.ToString("F1");
            }
            yield return null;
        }
    }

    /// <summary>
    /// 更新QTE进度
    /// </summary>
    public void UpdateQTEProgress(int progress)
    {
        if (qteProgressIndicators == null) return;

        for (int i = 0; i < qteProgressIndicators.Length; i++)
        {
            if (qteProgressIndicators[i] != null)
            {
                qteProgressIndicators[i].color = i < progress ? Color.green : Color.gray;
            }
        }
    }

    /// <summary>
    /// 隐藏QTE面板
    /// </summary>
    public void HideQTEPanel()
    {
        if (qtePanel != null)
        {
            qtePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 显示QTE结果
    /// </summary>
    public void ShowQTEResult(bool success)
    {
        if (qteSequenceText != null)
        {
            qteSequenceText.text = success ? "成功!" : "失败!";
            qteSequenceText.color = success ? Color.green : Color.red;
        }

        StartCoroutine(HideQTEAfterDelay());
    }

    private IEnumerator HideQTEAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        HideQTEPanel();
    }

    #endregion
}
