using UnityEngine;
using System;

/// <summary>
/// 敌人基类 - 所有敌人的基础组件
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float damage = 20f;
    public float moveSpeed = 3f;

    [Header("State")]
    public bool isDead = false;
    public bool isStunned = false;
    public float stunTimer = 0f;

    [Header("Target")]
    public Transform target;  // 通常是玩家

    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color damageFlashColor = Color.white;
    private Color originalColor;
    private float flashTimer = 0f;
    private float flashDuration = 0.1f;

    // 事件
    public event Action<float> OnDamageTaken;
    public event Action OnDeath;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // 自动寻找玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                target = player.transform;
        }
    }

    protected virtual void Update()
    {
        // 更新眩晕状态
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
            }
            return;
        }

        // 更新闪烁效果
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0 && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        // 执行AI行为
        if (!isDead)
        {
            PerformAI();
        }
    }

    /// <summary>
    /// AI行为 - 子类重写
    /// </summary>
    protected virtual void PerformAI()
    {
        // 基础追击行为
        if (target != null)
        {
            MoveTowardsTarget();
        }
    }

    /// <summary>
    /// 朝目标移动
    /// </summary>
    protected virtual void MoveTowardsTarget()
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

        // 面向目标
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        OnDamageTaken?.Invoke(damage);

        // 闪烁效果
        FlashDamage();

        Debug.Log($"[Enemy] {gameObject.name} 受到 {damage} 伤害, 剩余: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 伤害闪烁效果
    /// </summary>
    protected void FlashDamage()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = damageFlashColor;
            flashTimer = flashDuration;
        }
    }

    /// <summary>
    /// 眩晕
    /// </summary>
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();

        Debug.Log($"[Enemy] {gameObject.name} 被击败!");

        // 简单的死亡效果 - 淡出后销毁
        StartCoroutine(DeathSequence());
    }

    protected virtual System.Collections.IEnumerator DeathSequence()
    {
        // 淡出效果
        float fadeTime = 0.5f;
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
    /// 攻击玩家
    /// </summary>
    protected virtual void AttackPlayer()
    {
        if (target == null) return;

        PlayerCombatController playerCombat = target.GetComponent<PlayerCombatController>();
        if (playerCombat != null)
        {
            playerCombat.TakeDamage(damage);
        }
    }

    /// <summary>
    /// 碰撞检测 - 接触伤害
    /// </summary>
    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
}
