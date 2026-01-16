using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 生命值系统 - 用于玩家和敌人的生命值管理
/// </summary>
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI (Optional)")]
    public Slider healthSlider;
    public Image healthFillImage;
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    [Header("Damage Settings")]
    public float invincibilityDuration = 0.5f;  // 受伤后无敌时间
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    // 事件
    public event Action<float, float> OnHealthChanged;  // (当前血量, 最大血量)
    public event Action<float> OnDamageTaken;           // 受到的伤害值
    public event Action OnDeath;
    public event Action<float> OnHeal;

    [Header("State")]
    public bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    void Update()
    {
        // 更新无敌计时器
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
            }
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        if (isInvincible) return;

        // 检查术式系统的无敌状态
        if (SpellSystem.Instance != null && SpellSystem.Instance.isInvincible)
        {
            Debug.Log("[Health] 无敌状态，伤害无效!");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnDamageTaken?.Invoke(damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        UpdateUI();
        
        Debug.Log($"[Health] {gameObject.name} 受到 {damage} 伤害, 剩余生命: {currentHealth}/{maxHealth}");

        // 触发无敌时间
        if (invincibilityDuration > 0)
        {
            StartInvincibility();
        }

        // 检查死亡
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

        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        float actualHeal = currentHealth - previousHealth;
        
        if (actualHeal > 0)
        {
            OnHeal?.Invoke(actualHeal);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            UpdateUI();
            
            Debug.Log($"[Health] {gameObject.name} 恢复 {actualHeal} 生命, 当前生命: {currentHealth}/{maxHealth}");
        }
    }

    /// <summary>
    /// 开始无敌时间
    /// </summary>
    private void StartInvincibility()
    {
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnDeath?.Invoke();
        
        Debug.Log($"[Health] {gameObject.name} 死亡!");
    }

    /// <summary>
    /// 重置生命值
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;
        invincibilityTimer = 0;
        UpdateUI();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth / maxHealth;
        }

        if (healthFillImage != null)
        {
            float healthPercent = currentHealth / maxHealth;
            healthFillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent);
        }
    }

    /// <summary>
    /// 获取生命值百分比
    /// </summary>
    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    /// <summary>
    /// 设置临时无敌
    /// </summary>
    public void SetTemporaryInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = duration;
    }
}
