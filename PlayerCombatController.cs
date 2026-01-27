using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player Combat Controller - Simplified version
/// Processes hand gestures directly for spell casting
/// </summary>
public class PlayerCombatController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public Slider healthSlider;
    public bool isDead = false;

    [Header("Attack Settings")]
    public float attackDamage = 25f;
    public float attackRange = 2f;
    public float attackCooldown = 0.3f;
    private float attackTimer = 0f;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("References")]
    public HandSignReceiver handSignReceiver;
    public SealSlotManager sealSlotManager;

    // Events
    public event System.Action OnPlayerDeath;
    public event System.Action<float> OnDamageTaken;

    // Legacy compatibility
    public bool isDomainActive = false;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
        
        // Find references if not assigned
        if (handSignReceiver == null)
        {
            handSignReceiver = FindFirstObjectByType<HandSignReceiver>();
        }
        if (sealSlotManager == null)
        {
            sealSlotManager = FindFirstObjectByType<SealSlotManager>();
        }
    }

    void Update()
    {
        // Update attack cooldown
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // Process hand gesture input for spells
        ProcessHandSignInput();

        // Keyboard fallback: mouse left click for basic attack
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0)
        {
            PerformAttack();
        }
    }

    /// <summary>
    /// Process hand gesture input for immediate spell casting
    /// </summary>
    private void ProcessHandSignInput()
    {
        if (handSignReceiver == null || sealSlotManager == null) return;
        
        if (handSignReceiver.hasNewInput)
        {
            int signID = handSignReceiver.latestSignID;
            string signName = handSignReceiver.latestSignName;
            
            Debug.Log($"[Combat] Gesture received: ID={signID}, Name={signName}");
            
            // Try to trigger spell immediately
            sealSlotManager.TryAddSealBySignID(signID);
            
            // Consume input
            handSignReceiver.hasNewInput = false;
        }
    }

    /// <summary>
    /// Perform basic attack
    /// </summary>
    private void PerformAttack()
    {
        attackTimer = attackCooldown;
        
        Debug.Log("[Combat] Basic attack!");

        if (attackPoint != null)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
            
            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase != null)
                {
                    enemyBase.TakeDamage(attackDamage);
                }

                BossController boss = enemy.GetComponent<BossController>();
                if (boss != null)
                {
                    boss.TakeDamage(attackDamage);
                }
            }
        }
    }

    /// <summary>
    /// Take damage
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Check invincibility from spell system
        if (SpellSystem.Instance != null && SpellSystem.Instance.isInvincible)
        {
            Debug.Log("[Combat] Defense active! Damage blocked!");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnDamageTaken?.Invoke(damage);
        UpdateUI();

        Debug.Log($"[Combat] Player took {damage} damage, HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal player
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
        
        Debug.Log($"[Combat] Player healed {amount}, HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Player death
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnPlayerDeath?.Invoke();
        Debug.Log("[Combat] Player died!");
    }

    /// <summary>
    /// Update UI
    /// </summary>
    private void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
    }

    /// <summary>
    /// Visualize attack range in editor
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

    // Legacy compatibility methods
    public void ActivateDomain() { }
    public void DeactivateDomain() { }
    public void GainEnergy(float amount) { }
    public void ConsumeEnergy(float amount) { }
}
