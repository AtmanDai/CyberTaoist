using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Simplified spell data structure
/// </summary>
[System.Serializable]
public class SpellData
{
    public SpellType spellType;
    public string spellName;
    public string description;
    public float damage;
    public float duration;
    public float cooldown;
    public GameObject effectPrefab;
    public AudioClip soundEffect;
}

/// <summary>
/// Simplified Spell System - Three gestures with immediate release and 10s cooldown
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
    [Tooltip("Universal cooldown for all skills (10 seconds)")]
    public float universalCooldown = 10f;
    private Dictionary<SpellType, float> cooldownTimers = new Dictionary<SpellType, float>();

    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public float fireballSpeed = 10f;
    public float fireballDamage = 50f;

    [Header("Defense Settings")]
    public float defenseDuration = 3f;

    [Header("Active Effects")]
    public bool isInvincible = false;
    public float speedMultiplier = 1f;

    // Events
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

        // Subscribe to gesture events
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
    /// Initialize spell data for three simplified gestures
    /// </summary>
    private void InitializeSpellData()
    {
        if (spellDataList.Count == 0)
        {
            // Rock - Defense
            spellDataList.Add(new SpellData
            {
                spellType = SpellType.Rock,
                spellName = "Rock Defense",
                description = "Create a protective barrier, become invincible briefly",
                damage = 0f,
                duration = defenseDuration,
                cooldown = universalCooldown
            });

            // Thumbs Up - Fireballs
            spellDataList.Add(new SpellData
            {
                spellType = SpellType.ThumbsUp,
                spellName = "Fireball",
                description = "Shoot a fireball at enemies",
                damage = fireballDamage,
                duration = 0f,
                cooldown = universalCooldown
            });

            // Fist - Normal Attack
            spellDataList.Add(new SpellData
            {
                spellType = SpellType.Fist,
                spellName = "Power Strike",
                description = "Perform a powerful melee attack",
                damage = 75f,
                duration = 0f,
                cooldown = universalCooldown
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
        // Update cooldown timers
        var keys = new List<SpellType>(cooldownTimers.Keys);
        foreach (var key in keys)
        {
            if (cooldownTimers[key] > 0)
            {
                cooldownTimers[key] -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Handle spell determined event - immediate spell release
    /// </summary>
    private void HandleSpellDetermined(SpellType spell)
    {
        if (spell == SpellType.None)
        {
            return;
        }

        CastSpell(spell);
    }

    /// <summary>
    /// Cast spell immediately
    /// </summary>
    public void CastSpell(SpellType spellType)
    {
        if (!CanCastSpell(spellType))
        {
            float remaining = GetCooldownRemaining(spellType);
            Debug.Log($"[SpellSystem] {spellType} on cooldown: {remaining:F1}s remaining");
            return;
        }

        SpellData data = GetSpellData(spellType);
        if (data == null)
        {
            Debug.LogWarning($"[SpellSystem] Spell data not found: {spellType}");
            return;
        }

        // Execute spell immediately
        ExecuteSpellEffect(data);
        
        // Set cooldown (10 seconds for all skills)
        SetSpellCooldown(spellType, universalCooldown);
        
        OnSpellCast?.Invoke(spellType);
        Debug.Log($"[SpellSystem] Cast: {data.spellName}");
    }

    /// <summary>
    /// Execute spell effect based on type
    /// </summary>
    private void ExecuteSpellEffect(SpellData data)
    {
        switch (data.spellType)
        {
            case SpellType.Rock:
                ExecuteRockDefense(data);
                break;
            case SpellType.ThumbsUp:
                ExecuteFireball(data);
                break;
            case SpellType.Fist:
                ExecutePowerStrike(data);
                break;
        }

        // Play effect prefab if available
        if (data.effectPrefab != null && playerTransform != null)
        {
            Instantiate(data.effectPrefab, playerTransform.position, Quaternion.identity);
        }

        OnSpellComplete?.Invoke();
    }

    /// <summary>
    /// Rock - Defense: Brief invincibility
    /// </summary>
    private void ExecuteRockDefense(SpellData data)
    {
        Debug.Log(">>> Rock Defense Activated! <<<");
        StartCoroutine(InvincibilityCoroutine(data.duration));
    }

    private IEnumerator InvincibilityCoroutine(float duration)
    {
        isInvincible = true;
        Debug.Log("[SpellSystem] Defense active!");
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        Debug.Log("[SpellSystem] Defense ended");
    }

    /// <summary>
    /// Thumbs Up - Fireball: Ranged projectile attack
    /// </summary>
    private void ExecuteFireball(SpellData data)
    {
        Debug.Log(">>> Fireball Launched! <<<");
        
        if (playerTransform == null) return;

        // Determine direction based on player facing
        float direction = playerTransform.localScale.x > 0 ? 1f : -1f;
        Vector2 spawnPos = (Vector2)playerTransform.position + new Vector2(direction * 1f, 0.5f);
        
        if (fireballPrefab != null)
        {
            GameObject fireball = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            Projectile proj = fireball.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.damage = data.damage;
                proj.damagePlayer = false;
                proj.damageEnemies = true;
                proj.SetDirection(new Vector2(direction, 0));
            }
        }
        else
        {
            // Fallback: damage nearby enemies if no prefab
            DamageNearbyEnemies(data.damage, 5f);
        }
    }

    /// <summary>
    /// Fist - Power Strike: Powerful melee attack
    /// </summary>
    private void ExecutePowerStrike(SpellData data)
    {
        Debug.Log(">>> Power Strike! <<<");
        DamageNearbyEnemies(data.damage, 3f);
    }

    /// <summary>
    /// Damage all enemies within range
    /// </summary>
    private void DamageNearbyEnemies(float damage, float range)
    {
        if (playerTransform == null) return;

        // Find and damage all enemies in range
        EnemyBase[] enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);
        foreach (var enemy in enemies)
        {
            if (Vector2.Distance(playerTransform.position, enemy.transform.position) <= range)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Also check for boss
        BossController boss = FindFirstObjectByType<BossController>();
        if (boss != null && Vector2.Distance(playerTransform.position, boss.transform.position) <= range)
        {
            boss.TakeDamage(damage);
        }
    }

    /// <summary>
    /// Check if spell can be cast (not on cooldown)
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
    /// Set spell cooldown
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
    /// Get spell data
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
    /// Get remaining cooldown time for a spell
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
