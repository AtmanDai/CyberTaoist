using UnityEngine;

/// <summary>
/// 投射物控制器 - 用于Boss弹幕等
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    public float damage = 20f;
    public float lifetime = 5f;
    public float speed = 8f;
    
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Color projectileColor = Color.magenta;
    public TrailRenderer trail;

    [Header("Behavior")]
    public bool destroyOnHit = true;
    public bool damagePlayer = true;
    public bool damageEnemies = false;

    private Vector2 direction;
    private Rigidbody2D rb;

    void Start()
    {
        // 自动销毁
        Destroy(gameObject, lifetime);

        // 设置视觉
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = projectileColor;
        }

        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>
    /// 设置飞行方向
    /// </summary>
    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }

        // 旋转面向方向
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void Update()
    {
        // 如果没有Rigidbody，手动移动
        if (rb == null)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家碰撞
        if (damagePlayer && other.CompareTag("Player"))
        {
            PlayerCombatController playerCombat = other.GetComponent<PlayerCombatController>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(damage);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }

        // 检测敌人碰撞（如果是玩家发射的）
        if (damageEnemies && other.CompareTag("Enemy"))
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }

        // 检测墙壁
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 相同的碰撞处理逻辑
        if (damagePlayer && collision.gameObject.CompareTag("Player"))
        {
            PlayerCombatController playerCombat = collision.gameObject.GetComponent<PlayerCombatController>();
            if (playerCombat != null)
            {
                playerCombat.TakeDamage(damage);
            }

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}
