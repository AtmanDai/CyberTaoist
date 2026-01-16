using UnityEngine;

/// <summary>
/// 赛博斧手 - 中型近战单位
/// HP: 250, 攻击: 斧头劈砍30, AI: 保持5格距离，绕圈走位
/// </summary>
public class CyberAxeman : EnemyBase
{
    [Header("Axeman Settings")]
    public float preferredDistance = 5f;      // 理想距离
    public float attackRange = 2f;            // 攻击范围
    public float attackCooldown = 2f;         // 攻击冷却
    private float attackTimer = 0f;

    [Header("Circling Behavior")]
    public float circleSpeed = 2f;            // 绕圈速度
    public float circleDirection = 1f;        // 1 = 顺时针, -1 = 逆时针

    [Header("Attack Animation")]
    public float attackWindup = 0.5f;         // 攻击前摇
    private bool isAttacking = false;

    protected override void Start()
    {
        // 设置赛博斧手属性
        maxHealth = 250f;
        damage = 30f;
        moveSpeed = 3f;
        
        base.Start();
        
        // 设置视觉 - 蓝色方块
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }

        // 随机化绕圈方向
        circleDirection = Random.value > 0.5f ? 1f : -1f;
    }

    protected override void Update()
    {
        base.Update();

        // 更新攻击计时器
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// AI行为：保持距离 + 绕圈 + 攻击
    /// </summary>
    protected override void PerformAI()
    {
        if (target == null || isAttacking) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // 根据距离决定行为
        if (distanceToTarget > preferredDistance + 1f)
        {
            // 太远 - 接近
            MoveTowardsTarget();
        }
        else if (distanceToTarget < preferredDistance - 1f)
        {
            // 太近 - 后退
            MoveAwayFromTarget();
        }
        else
        {
            // 理想距离 - 绕圈走位
            CircleAroundTarget();
        }

        // 如果在攻击范围内且冷却完成，发起攻击
        if (distanceToTarget <= attackRange && attackTimer <= 0)
        {
            StartCoroutine(PerformAxeAttack());
        }
    }

    /// <summary>
    /// 远离目标
    /// </summary>
    private void MoveAwayFromTarget()
    {
        if (target == null) return;

        Vector2 direction = (transform.position - target.position).normalized;
        transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 绕目标转圈
    /// </summary>
    private void CircleAroundTarget()
    {
        if (target == null) return;

        Vector2 toTarget = target.position - transform.position;
        Vector2 perpendicular = new Vector2(-toTarget.y, toTarget.x).normalized;
        
        transform.position += (Vector3)(perpendicular * circleSpeed * circleDirection * Time.deltaTime);

        // 面向目标
        if (toTarget.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    /// <summary>
    /// 执行斧头攻击
    /// </summary>
    private System.Collections.IEnumerator PerformAxeAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        // 前摇 - 蓄力动画
        if (spriteRenderer != null)
        {
            Color chargeColor = new Color(0.5f, 0.5f, 1f);
            spriteRenderer.color = chargeColor;
        }

        Debug.Log("[CyberAxeman] 斧头蓄力!");

        yield return new WaitForSeconds(attackWindup);

        // 检查玩家是否还在攻击范围内
        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget <= attackRange)
            {
                AttackPlayer();
                Debug.Log("[CyberAxeman] 斧头劈砍!");
            }
            else
            {
                Debug.Log("[CyberAxeman] 玩家闪避了攻击!");
            }
        }

        // 恢复颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.blue;
        }

        isAttacking = false;
    }
}
