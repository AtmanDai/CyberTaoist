using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家战斗控制器 - 整合攻击、能量和领域展开系统
/// </summary>
public class PlayerCombatController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;
    public bool isDead = false;

    [Header("Energy System")]
    public float maxEnergy = 100f;
    public float currentEnergy = 0f;
    public float energyGainPerHit = 20f;
    public Slider energySlider;

    [Header("Attack Settings")]
    public float attackDamage = 25f;
    public float attackRange = 2f;
    public float attackCooldown = 0.3f;
    private float attackTimer = 0f;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("Domain Expansion")]
    public bool isDomainActive = false;
    public float domainDuration = 10f;  // 领域持续10秒
    private float domainTimer = 0f;

    [Header("Visual Effects")]
    public GameObject domainOverlay;       // 领域视觉效果
    public ParticleSystem sealingEffect;   // 结印特效

    [Header("References")]
    public SealSlotManager sealSlotManager;
    public HandSignReceiver handSignReceiver;

    // 事件
    public event System.Action OnDomainEnter;
    public event System.Action OnDomainExit;
    public event System.Action OnPlayerDeath;
    public event System.Action<float> OnDamageTaken;

    void Start()
    {
        currentHealth = maxHealth;
        currentEnergy = 0;
        UpdateUI();
        
        if (domainOverlay != null) 
            domainOverlay.SetActive(false);
    }

    void Update()
    {
        // 更新攻击冷却
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 1. 普通攻击 (左键)
        if (Input.GetMouseButtonDown(0) && !isDomainActive && attackTimer <= 0)
        {
            PerformAttack();
        }

        // 2. 开启领域展开 (F键)
        if (Input.GetKeyDown(KeyCode.F) && currentEnergy >= maxEnergy && !isDomainActive)
        {
            ActivateDomain();
        }

        // 3. 领域倒计时逻辑
        if (isDomainActive)
        {
            domainTimer -= Time.unscaledDeltaTime;
            
            // 处理手势输入
            ProcessHandSignInput();
            
            if (domainTimer <= 0)
            {
                DeactivateDomain();
            }
        }
    }

    /// <summary>
    /// 执行普通攻击
    /// </summary>
    private void PerformAttack()
    {
        attackTimer = attackCooldown;
        
        Debug.Log("[Combat] 挥剑攻击!");

        // 检测攻击范围内的敌人
        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
            
            foreach (Collider2D enemy in hitEnemies)
            {
                // 尝试获取敌人组件并造成伤害
                EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase != null)
                {
                    enemyBase.TakeDamage(attackDamage);
                    GainEnergy(energyGainPerHit);
                }

                BossController boss = enemy.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage(attackDamage);
                    GainEnergy(energyGainPerHit);
                }
            }
        }
    }

    /// <summary>
    /// 处理手势输入（领域展开时）
    /// </summary>
    private void ProcessHandSignInput()
    {
        if (handSignReceiver == null || sealSlotManager == null) return;
        
        if (handSignReceiver.hasNewInput)
        {
            int signID = handSignReceiver.latestSignID;
            string signName = handSignReceiver.latestSignName;
            
            Debug.Log($"[Combat] 接收到手势: ID={signID}, Name={signName}");
            
            // 检查是否是祈印（结束信号）
            if (signID == sealSlotManager.endSealSignID)
            {
                // 触发术式释放并结束领域
                sealSlotManager.TriggerSpellRelease();
                DeactivateDomain();
            }
            else
            {
                // 尝试添加印记
                sealSlotManager.TryAddSealBySignID(signID);
            }
            
            // 消费输入
            handSignReceiver.hasNewInput = false;
        }
    }

    /// <summary>
    /// 获得能量
    /// </summary>
    public void GainEnergy(float amount)
    {
        if (isDomainActive) return;  // 领域期间不获得能量
        
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateUI();
        
        Debug.Log($"[Combat] 获得 {amount} 咒力, 当前: {currentEnergy}/{maxEnergy}");
    }

    /// <summary>
    /// 消耗能量
    /// </summary>
    public void ConsumeEnergy(float amount)
    {
        currentEnergy = Mathf.Max(0, currentEnergy - amount);
        UpdateUI();
    }

    /// <summary>
    /// 激活领域展开
    /// </summary>
    public void ActivateDomain()
    {
        if (isDomainActive) return;
        
        isDomainActive = true;
        domainTimer = domainDuration;
        
        // 消耗能量（释放术式时根据印记数量消耗）
        // currentEnergy = 0;
        
        // 切换游戏状态
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetState(GameState.DomainExpansion);
        }

        // 视觉效果
        if (domainOverlay != null)
            domainOverlay.SetActive(true);
        
        if (sealingEffect != null)
            sealingEffect.Play();

        OnDomainEnter?.Invoke();
        Debug.Log(">>> 领域展开：子午档案 <<<");
    }

    /// <summary>
    /// 结束领域展开
    /// </summary>
    public void DeactivateDomain()
    {
        if (!isDomainActive) return;

        isDomainActive = false;

        // 根据已消耗的印记数量扣除能量
        if (sealSlotManager != null)
        {
            int filledSlots = sealSlotManager.GetFilledSlotCount();
            float energyCost = (maxEnergy / 3f) * filledSlots;
            ConsumeEnergy(energyCost);
        }

        // 恢复游戏状态
        if (GameStateManager.Instance != null)
        {
            // 如果正在释放术式，由SpellSystem负责切换状态
            if (GameStateManager.Instance.CurrentState == GameState.DomainExpansion)
            {
                GameStateManager.Instance.SetState(GameState.Normal);
            }
        }

        // 关闭视觉效果
        if (domainOverlay != null)
            domainOverlay.SetActive(false);
        
        if (sealingEffect != null)
            sealingEffect.Stop();

        OnDomainExit?.Invoke();
        Debug.Log("<<< 领域关闭 >>>");
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // 检查无敌状态
        if (SpellSystem.Instance != null && SpellSystem.Instance.isInvincible)
        {
            Debug.Log("[Combat] 金刚不坏身！伤害无效!");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnDamageTaken?.Invoke(damage);
        UpdateUI();

        Debug.Log($"[Combat] 玩家受到 {damage} 伤害, 剩余生命: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 治疗
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
        
        Debug.Log($"[Combat] 玩家恢复 {amount} 生命, 当前: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 玩家死亡
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnPlayerDeath?.Invoke();
        
        // 结束领域（如果激活中）
        if (isDomainActive)
        {
            DeactivateDomain();
        }

        Debug.Log("[Combat] 玩家死亡!");
        // TODO: 显示游戏结束界面
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    private void UpdateUI()
    {
        if (energySlider != null)
            energySlider.value = currentEnergy / maxEnergy;
        
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    /// <summary>
    /// 可视化攻击范围
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}
