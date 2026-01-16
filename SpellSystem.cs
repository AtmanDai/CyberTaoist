using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 术式数据结构
/// </summary>
[System.Serializable]
public class SpellData
{
    public SpellType spellType;
    public string spellName;        // 术式名称
    public string description;      // 描述
    public float damage;            // 伤害
    public float duration;          // 持续时间
    public float cooldown;          // 冷却时间
    public GameObject effectPrefab; // 特效预制体
    public AudioClip soundEffect;   // 音效
}

/// <summary>
/// 术式系统 - 管理所有术式的释放和效果
/// </summary>
public class SpellSystem : MonoBehaviour
{
    public static SpellSystem Instance { get; private set; }

    [Header("Spell Data")]
    public List<SpellData> spellDataList = new List<SpellData>();
    private Dictionary<SpellType, SpellData> spellDictionary = new Dictionary<SpellType, SpellData>();

    [Header("Player Reference")]
    public Transform playerTransform;
    public PlayerCombatController playerCombat;

    [Header("Spell Cooldowns")]
    private Dictionary<SpellType, float> cooldownTimers = new Dictionary<SpellType, float>();

    [Header("Visual Settings")]
    public float spellCastDuration = 1.5f;  // 术式演出时间
    public float chargeUpTime = 0.3f;       // 蓄力时间

    [Header("Active Effects")]
    public bool isInvincible = false;
    public float speedMultiplier = 1f;

    // 事件
    public event System.Action<SpellType> OnSpellCast;
    public event System.Action OnSpellComplete;

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
        InitializeSpellData();
        BuildSpellDictionary();

        // 订阅印记槽事件
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSpellDetermined += HandleSpellDetermined;
        }
    }

    void OnDestroy()
    {
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSpellDetermined -= HandleSpellDetermined;
        }
    }

    /// <summary>
    /// 初始化术式数据（如果没有在Inspector中设置）
    /// </summary>
    private void InitializeSpellData()
    {
        if (spellDataList.Count == 0)
        {
            // 三同印术式
            spellDataList.Add(new SpellData
            {
                spellType = SpellType.DragonDragonDragon,
                spellName = "离火聚龙",
                description = "召唤火龙造成大范围伤害",
                damage = 200f,
                duration = 1.5f,
                cooldown = 5f
            });

            spellDataList.Add(new SpellData
            {
                spellType = SpellType.OxOxOx,
                spellName = "金刚不坏身",
                description = "获得5秒无敌并反弹伤害",
                damage = 0f,
                duration = 5f,
                cooldown = 5f
            });

            spellDataList.Add(new SpellData
            {
                spellType = SpellType.RabbitRabbitRabbit,
                spellName = "神行千里",
                description = "移动速度提升3倍",
                damage = 0f,
                duration = 10f,
                cooldown = 10f
            });

            // 三异印术式
            spellDataList.Add(new SpellData
            {
                spellType = SpellType.DragonOxRabbit,
                spellName = "三元归一",
                description = "恢复50%生命值",
                damage = 0f,
                duration = 0f,
                cooldown = 5f
            });

            // 双同印术式（待扩展）
            spellDataList.Add(new SpellData
            {
                spellType = SpellType.DragonDragonOx,
                spellName = "炎铠",
                description = "火焰护盾，造成接触伤害",
                damage = 50f,
                duration = 3f,
                cooldown = 5f
            });

            spellDataList.Add(new SpellData
            {
                spellType = SpellType.DragonDragonRabbit,
                spellName = "火遁·疾",
                description = "快速冲刺并留下火焰轨迹",
                damage = 80f,
                duration = 1f,
                cooldown = 5f
            });
        }
    }

    private void BuildSpellDictionary()
    {
        spellDictionary.Clear();
        foreach (var data in spellDataList)
        {
            if (!spellDictionary.ContainsKey(data.spellType))
            {
                spellDictionary.Add(data.spellType, data);
            }
        }
    }

    void Update()
    {
        // 更新冷却计时器 - 只在正常游戏状态下更新冷却
        // 使用deltaTime确保在慢动作期间冷却正常计算
        if (GameStateManager.Instance == null || 
            GameStateManager.Instance.CurrentState == GameState.Normal)
        {
            var keys = new List<SpellType>(cooldownTimers.Keys);
            foreach (var key in keys)
            {
                if (cooldownTimers[key] > 0)
                {
                    cooldownTimers[key] -= Time.deltaTime;
                }
            }
        }
    }

    /// <summary>
    /// 处理术式确定事件
    /// </summary>
    private void HandleSpellDetermined(SpellType spell)
    {
        if (spell == SpellType.None)
        {
            Debug.Log("[SpellSystem] 术式组合无效");
            return;
        }

        CastSpell(spell);
    }

    /// <summary>
    /// 释放术式
    /// </summary>
    public void CastSpell(SpellType spellType)
    {
        if (!CanCastSpell(spellType))
        {
            Debug.Log($"[SpellSystem] {spellType} 冷却中或无法释放");
            return;
        }

        SpellData data = GetSpellData(spellType);
        if (data == null)
        {
            Debug.LogWarning($"[SpellSystem] 未找到术式数据: {spellType}");
            return;
        }

        StartCoroutine(ExecuteSpellSequence(data));
    }

    private IEnumerator ExecuteSpellSequence(SpellData data)
    {
        // 1. 进入术式演出状态
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.SpellCast);
        }

        OnSpellCast?.Invoke(data.spellType);
        Debug.Log($"[SpellSystem] 开始释放: {data.spellName}");

        // 2. 蓄力阶段（使用unscaledTime因为游戏暂停了）
        float chargeTimer = 0f;
        while (chargeTimer < chargeUpTime)
        {
            chargeTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 3. 释放术式效果
        ExecuteSpellEffect(data);

        // 4. 播放特效
        if (data.effectPrefab != null && playerTransform != null)
        {
            Instantiate(data.effectPrefab, playerTransform.position, Quaternion.identity);
        }

        // 5. 术式演出时间
        float castTimer = 0f;
        while (castTimer < spellCastDuration)
        {
            castTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 6. 设置冷却
        SetSpellCooldown(data.spellType, data.cooldown);

        // 7. 清空印记槽
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.ClearSlots();
        }

        // 8. 返回正常状态
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.Normal);
        }

        OnSpellComplete?.Invoke();
        Debug.Log($"[SpellSystem] {data.spellName} 释放完成");
    }

    /// <summary>
    /// 执行具体的术式效果
    /// </summary>
    private void ExecuteSpellEffect(SpellData data)
    {
        switch (data.spellType)
        {
            case SpellType.DragonDragonDragon:
                ExecuteDragonSpell(data);
                break;
            case SpellType.OxOxOx:
                ExecuteOxSpell(data);
                break;
            case SpellType.RabbitRabbitRabbit:
                ExecuteRabbitSpell(data);
                break;
            case SpellType.DragonOxRabbit:
                ExecuteHealSpell(data);
                break;
            default:
                ExecuteGenericSpell(data);
                break;
        }
    }

    /// <summary>
    /// 离火聚龙 - 火龙特效, 200伤害
    /// </summary>
    private void ExecuteDragonSpell(SpellData data)
    {
        Debug.Log($">>> {data.spellName} 发动! <<<");
        
        // 对场景中所有敌人造成伤害
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            enemy.TakeDamage(data.damage);
        }

        // 对Boss造成伤害
        BossController boss = FindFirstObjectByType<BossController>();
        if (boss != null)
        {
            boss.TakeDamage(data.damage);
        }
    }

    /// <summary>
    /// 金刚不坏身 - 5秒无敌+反伤
    /// </summary>
    private void ExecuteOxSpell(SpellData data)
    {
        Debug.Log($">>> {data.spellName} 发动! <<<");
        StartCoroutine(InvincibilityCoroutine(data.duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        Debug.Log("[SpellSystem] 无敌效果结束");
    }

    /// <summary>
    /// 神行千里 - 移速x3
    /// </summary>
    private void ExecuteRabbitSpell(SpellData data)
    {
        Debug.Log($">>> {data.spellName} 发动! <<<");
        StartCoroutine(SpeedBoostCoroutine(3f, data.duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        yield return new WaitForSeconds(duration);
        speedMultiplier = 1f;
        Debug.Log("[SpellSystem] 加速效果结束");
    }

    /// <summary>
    /// 三元归一 - 回血50%
    /// </summary>
    private void ExecuteHealSpell(SpellData data)
    {
        Debug.Log($">>> {data.spellName} 发动! <<<");
        
        if (playerCombat != null)
        {
            playerCombat.Heal(playerCombat.maxHealth * 0.5f);
        }
    }

    /// <summary>
    /// 通用术式效果
    /// </summary>
    private void ExecuteGenericSpell(SpellData data)
    {
        Debug.Log($">>> {data.spellName} 发动! <<<");
        
        if (data.damage > 0)
        {
            // 对周围敌人造成伤害
            EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
            foreach (var enemy in enemies)
            {
                enemy.TakeDamage(data.damage);
            }
        }
    }

    /// <summary>
    /// 检查术式是否可以释放
    /// </summary>
    public bool CanCastSpell(SpellType spellType)
    {
        if (!cooldownTimers.ContainsKey(spellType))
        {
            return true;
        }
        return cooldownTimers[spellType] <= 0;
    }

    /// <summary>
    /// 设置术式冷却
    /// </summary>
    private void SetSpellCooldown(SpellType spellType, float cooldown)
    {
        if (cooldownTimers.ContainsKey(spellType))
        {
            cooldownTimers[spellType] = cooldown;
        }
        else
        {
            cooldownTimers.Add(spellType, cooldown);
        }
    }

    /// <summary>
    /// 获取术式数据
    /// </summary>
    public SpellData GetSpellData(SpellType spellType)
    {
        if (spellDictionary.TryGetValue(spellType, out SpellData data))
        {
            return data;
        }
        return null;
    }

    /// <summary>
    /// 获取术式剩余冷却时间
    /// </summary>
    public float GetCooldownRemaining(SpellType spellType)
    {
        if (cooldownTimers.TryGetValue(spellType, out float remaining))
        {
            return Mathf.Max(0, remaining);
        }
        return 0;
    }
}
