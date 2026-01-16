using UnityEngine;
using System.Collections;

/// <summary>
/// 重构体 - 大型坦克单位
/// HP: 350, 攻击: 冲撞50, AI: 每5秒朝玩家冲锋一次，每次持续2秒
/// </summary>
public class ReconstructorEnemy : EnemyBase
{
    [Header("Reconstructor Settings")]
    public float chargeCooldown = 5f;         // 冲锋冷却
    public float chargeDuration = 2f;         // 冲锋持续时间
    public float chargeSpeed = 10f;           // 冲锋速度
    public float chargeWindup = 1f;           // 冲锋前摇

    private float chargeTimer = 0f;
    private bool isCharging = false;
    private Vector2 chargeDirection;

    [Header("Charge Visual")]
    public Color normalColor = Color.gray;
    public Color chargeColor = Color.yellow;
    public Color windupColor = Color.red;

    protected override void Start()
    {
        // 设置重构体属性
        maxHealth = 350f;
        damage = 50f;
        moveSpeed = 1.5f;  // 正常移动较慢
        
        base.Start();
        
        // 设置视觉 - 灰色大方块
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        // 初始冷却
        chargeTimer = chargeCooldown;
    }

    protected override void Update()
    {
        base.Update();

        // 更新冲锋计时器
        if (!isCharging && chargeTimer > 0)
        {
            chargeTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// AI行为：缓慢追击 + 定期冲锋
    /// </summary>
    protected override void PerformAI()
    {
        if (target == null) return;

        if (isCharging) return;

        // 检查是否可以发起冲锋
        if (chargeTimer <= 0)
        {
            StartCoroutine(PerformCharge());
            return;
        }

        // 缓慢追击
        MoveTowardsTarget();
    }

    /// <summary>
    /// 执行冲锋
    /// </summary>
    private IEnumerator PerformCharge()
    {
        isCharging = true;

        // 1. 前摇 - 锁定方向
        if (target != null)
        {
            chargeDirection = (target.position - transform.position).normalized;
        }

        // 视觉提示 - 变红
        if (spriteRenderer != null)
        {
            spriteRenderer.color = windupColor;
        }

        Debug.Log("[Reconstructor] 冲锋蓄力!");

        yield return new WaitForSeconds(chargeWindup);

        // 2. 冲锋阶段
        if (spriteRenderer != null)
        {
            spriteRenderer.color = chargeColor;
        }

        Debug.Log("[Reconstructor] 冲锋开始!");

        float chargeElapsed = 0f;
        while (chargeElapsed < chargeDuration)
        {
            // 沿锁定方向冲锋
            transform.position += (Vector3)(chargeDirection * chargeSpeed * Time.deltaTime);
            chargeElapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 冲锋结束
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        Debug.Log("[Reconstructor] 冲锋结束!");

        isCharging = false;
        chargeTimer = chargeCooldown;
    }

    /// <summary>
    /// 冲锋时的碰撞检测
    /// </summary>
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // 冲锋时造成额外伤害
            if (isCharging)
            {
                PlayerCombatController playerCombat = collision.gameObject.GetComponent<PlayerCombatController>();
                if (playerCombat != null)
                {
                    playerCombat.TakeDamage(damage);
                    Debug.Log("[Reconstructor] 冲撞命中!");
                }
            }
        }
    }

    /// <summary>
    /// 冲锋时的触发器检测（可选，用于穿过物体）
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || !isCharging) return;

        if (other.CompareTag("Player"))
        {
            PlayerCombatController playerCombat = other.GetComponent<PlayerCombatController>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(damage);
                Debug.Log("[Reconstructor] 冲撞命中!");
            }
        }
    }
}
