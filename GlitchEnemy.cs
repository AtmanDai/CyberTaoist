using UnityEngine;

/// <summary>
/// 故障者 - 小型近战单位
/// HP: 100, 攻击: 接触伤害20, AI: 直线追击玩家
/// </summary>
public class GlitchEnemy : EnemyBase
{
    [Header("Glitch Settings")]
    public float contactDamageInterval = 1f;  // 接触伤害间隔
    private float contactDamageTimer = 0f;

    protected override void Start()
    {
        // 设置故障者属性
        maxHealth = 100f;
        damage = 20f;
        moveSpeed = 4f;
        
        base.Start();
        
        // 设置视觉 - 红色方块
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }
    }

    protected override void Update()
    {
        base.Update();

        // 更新接触伤害计时器
        if (contactDamageTimer > 0)
        {
            contactDamageTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// AI行为：直线追击玩家
    /// </summary>
    protected override void PerformAI()
    {
        MoveTowardsTarget();
    }

    /// <summary>
    /// 接触伤害处理
    /// </summary>
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        
        if (collision.gameObject.CompareTag("Player") && contactDamageTimer <= 0)
        {
            AttackPlayer();
            contactDamageTimer = contactDamageInterval;
            Debug.Log("[GlitchEnemy] 接触伤害!");
        }
    }
}
