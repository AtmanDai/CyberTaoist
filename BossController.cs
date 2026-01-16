using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Boss控制器 - 赛博恶灵·参孙重锤
/// HP: 1000, 单阶段战斗
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public bool isDead = false;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform target;

    [Header("Combo Attack")]
    public float comboDamage = 30f;
    public int comboHits = 3;
    public float comboWindup = 0.5f;
    public float comboCooldown = 5f;
    private float comboTimer = 0f;

    [Header("Summon Phase - <70% HP")]
    public GameObject glitchPrefab;
    public int summonCount = 3;
    public bool hasSummoned = false;

    [Header("Barrage Phase - <40% HP")]
    public GameObject projectilePrefab;
    public int projectileCount = 20;
    public float projectileSpeed = 8f;
    public bool hasBarraged = false;

    [Header("QTE Phase - <20% HP")]
    public float qteChargeTime = 3f;
    public float qteTimeLimit = 10f;
    public float qteDamageOnFail = 200f;
    public float stunDurationOnSuccess = 5f;
    public bool hasQTEActivated = false;
    private bool isQTEActive = false;

    [Header("QTE Sequence")]
    public SealType[] requiredSequence = { SealType.Dragon, SealType.Ox, SealType.Rabbit };
    private int qteProgress = 0;

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color normalColor = new Color(0.5f, 0f, 0.5f);  // 紫色
    public Color enragedColor = Color.red;
    public Color stunColor = Color.gray;

    [Header("State")]
    public bool isStunned = false;
    public float stunTimer = 0f;
    private bool isAttacking = false;

    // 事件
    public event System.Action OnBossDeath;
    public event System.Action OnQTEStart;
    public event System.Action<bool> OnQTEEnd;  // true = 成功, false = 失败

    void Start()
    {
        currentHealth = maxHealth;
        comboTimer = comboCooldown;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        // 寻找玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    void Update()
    {
        if (isDead) return;

        // 更新眩晕状态
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                if (spriteRenderer != null)
                    spriteRenderer.color = normalColor;
                Debug.Log("[Boss] 眩晕结束!");
            }
            return;
        }

        // 更新攻击冷却
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
        }

        // QTE状态特殊处理
        if (isQTEActive) return;

        // 检查阶段触发
        CheckPhaseTransitions();

        // AI行为
        PerformAI();
    }

    /// <summary>
    /// 检查阶段转换
    /// </summary>
    private void CheckPhaseTransitions()
    {
        float healthPercent = currentHealth / maxHealth;

        // 召唤阶段 - <70% HP
        if (healthPercent < 0.7f && !hasSummoned)
        {
            StartCoroutine(SummonPhase());
        }

        // 弹幕阶段 - <40% HP
        if (healthPercent < 0.4f && !hasBarraged)
        {
            StartCoroutine(BarragePhase());
        }

        // QTE阶段 - <20% HP
        if (healthPercent < 0.2f && !hasQTEActivated)
        {
            StartCoroutine(QTEPhase());
        }
    }

    /// <summary>
    /// AI行为
    /// </summary>
    private void PerformAI()
    {
        if (isAttacking || target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // 追击玩家
        if (distanceToTarget > 2f)
        {
            MoveTowardsTarget();
        }
        // 攻击
        else if (comboTimer <= 0)
        {
            StartCoroutine(ComboAttack());
        }
    }

    /// <summary>
    /// 朝目标移动
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // 面向目标
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    /// <summary>
    /// 近战三连击
    /// </summary>
    private IEnumerator ComboAttack()
    {
        isAttacking = true;
        comboTimer = comboCooldown;

        Debug.Log("[Boss] 三连击准备!");

        // 前摇
        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;

        yield return new WaitForSeconds(comboWindup);

        // 三次攻击
        for (int i = 0; i < comboHits; i++)
        {
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance < 3f)
                {
                    PlayerCombatController playerCombat = target.GetComponent<PlayerCombatController>();
                    if (playerCombat != null)
                    {
                        playerCombat.TakeDamage(comboDamage);
                        Debug.Log($"[Boss] 三连击第{i + 1}击命中!");
                    }
                }
                else
                {
                    Debug.Log($"[Boss] 三连击第{i + 1}击落空!");
                }
            }
            yield return new WaitForSeconds(0.3f);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        isAttacking = false;
    }

    /// <summary>
    /// 召唤小怪阶段
    /// </summary>
    private IEnumerator SummonPhase()
    {
        hasSummoned = true;
        isAttacking = true;

        Debug.Log("[Boss] 召唤故障者!");

        // 视觉提示
        if (spriteRenderer != null)
            spriteRenderer.color = Color.magenta;

        yield return new WaitForSeconds(1f);

        // 生成小怪
        for (int i = 0; i < summonCount; i++)
        {
            if (glitchPrefab != null)
            {
                Vector2 spawnPos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
                Instantiate(glitchPrefab, spawnPos, Quaternion.identity);
            }
            yield return new WaitForSeconds(0.3f);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        isAttacking = false;
        Debug.Log("[Boss] 召唤完成!");
    }

    /// <summary>
    /// 全屏弹幕阶段
    /// </summary>
    private IEnumerator BarragePhase()
    {
        hasBarraged = true;
        isAttacking = true;

        Debug.Log("[Boss] 全屏弹幕!");

        // 视觉提示
        if (spriteRenderer != null)
            spriteRenderer.color = enragedColor;

        yield return new WaitForSeconds(0.5f);

        // 8方向发射弹幕
        int directionsCount = 8;
        int projectilesPerDirection = projectileCount / directionsCount;

        for (int wave = 0; wave < projectilesPerDirection; wave++)
        {
            for (int i = 0; i < directionsCount; i++)
            {
                float angle = (360f / directionsCount) * i;
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                if (projectilePrefab != null)
                {
                    GameObject proj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                    Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = direction * projectileSpeed;
                    }
                    Destroy(proj, 5f);  // 5秒后自动销毁
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        if (spriteRenderer != null)
            spriteRenderer.color = normalColor;

        isAttacking = false;
        Debug.Log("[Boss] 弹幕结束!");
    }

    /// <summary>
    /// QTE阶段 - 领域压制
    /// </summary>
    private IEnumerator QTEPhase()
    {
        hasQTEActivated = true;
        isQTEActive = true;
        isAttacking = true;
        qteProgress = 0;

        Debug.Log("[Boss] 领域压制·QTE开始!");
        Debug.Log($"[Boss] 请在{qteTimeLimit}秒内结印: 龙-牛-兔");

        OnQTEStart?.Invoke();

        // Boss停止移动，蓄力
        if (spriteRenderer != null)
            spriteRenderer.color = Color.black;

        // 订阅印记事件
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSealAdded += OnQTESealAdded;
        }

        // 等待蓄力
        yield return new WaitForSeconds(qteChargeTime);

        // 等待QTE完成或超时
        float qteTimer = qteTimeLimit;
        while (qteTimer > 0 && qteProgress < requiredSequence.Length)
        {
            qteTimer -= Time.deltaTime;
            yield return null;
        }

        // 取消订阅
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSealAdded -= OnQTESealAdded;
        }

        // 判断结果
        bool success = (qteProgress >= requiredSequence.Length);

        if (success)
        {
            // 成功 - Boss眩晕
            Debug.Log("[Boss] QTE成功! Boss眩晕!");
            Stun(stunDurationOnSuccess);
            OnQTEEnd?.Invoke(true);
        }
        else
        {
            // 失败 - 玩家受到大量伤害
            Debug.Log("[Boss] QTE失败! 受到领域压制!");
            if (target != null)
            {
                PlayerCombatController playerCombat = target.GetComponent<PlayerCombatController>();
                if (playerCombat != null)
                {
                    playerCombat.TakeDamage(qteDamageOnFail);
                }
            }
            OnQTEEnd?.Invoke(false);
        }

        isQTEActive = false;
        isAttacking = false;
    }

    /// <summary>
    /// QTE印记添加回调
    /// </summary>
    private void OnQTESealAdded(int slotIndex, SealType sealType)
    {
        if (!isQTEActive) return;

        // 检查是否匹配要求的序列
        if (qteProgress < requiredSequence.Length)
        {
            if (sealType == requiredSequence[qteProgress])
            {
                qteProgress++;
                Debug.Log($"[Boss] QTE进度: {qteProgress}/{requiredSequence.Length}");
            }
            else
            {
                // 输入错误，重置进度
                qteProgress = 0;
                Debug.Log("[Boss] QTE输入错误，进度重置!");
            }
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        
        Debug.Log($"[Boss] 受到 {damage} 伤害, 剩余: {currentHealth}/{maxHealth}");

        // 闪烁效果
        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator FlashDamage()
    {
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }
    }

    /// <summary>
    /// 眩晕
    /// </summary>
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        
        if (spriteRenderer != null)
            spriteRenderer.color = stunColor;
        
        Debug.Log($"[Boss] 眩晕 {duration} 秒!");
    }

    /// <summary>
    /// 死亡
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnBossDeath?.Invoke();
        
        Debug.Log("[Boss] 赛博恶灵·参孙重锤 被击败!");

        // 死亡动画
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // 闪烁
        for (int i = 0; i < 5; i++)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = (i % 2 == 0) ? Color.white : normalColor;
            }
            yield return new WaitForSeconds(0.2f);
        }

        // 淡出
        float fadeTime = 1f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1 - (elapsed / fadeTime);
                spriteRenderer.color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    /// <summary>
    /// 获取Boss血量百分比
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }
}
