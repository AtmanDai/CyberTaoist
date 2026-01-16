using UnityEngine;
using System.Collections;

/// <summary>
/// 术式特效系统 - 管理各种术式的视觉效果
/// </summary>
public class SpellEffects : MonoBehaviour
{
    public static SpellEffects Instance { get; private set; }

    [Header("Effect Prefabs")]
    public GameObject dragonFirePrefab;      // 离火聚龙特效
    public GameObject goldenShieldPrefab;    // 金刚不坏身特效
    public GameObject speedTrailPrefab;      // 神行千里特效
    public GameObject healingAuraPrefab;     // 三元归一特效

    [Header("Player Reference")]
    public Transform playerTransform;

    [Header("Effect Settings")]
    public float dragonFireDuration = 1.5f;
    public float shieldDuration = 5f;
    public float speedTrailDuration = 10f;

    [Header("Particle Colors")]
    public Color dragonColor = Color.red;
    public Color oxColor = new Color(1f, 0.84f, 0f);
    public Color rabbitColor = Color.blue;

    [Header("Trail Renderer")]
    public TrailRenderer playerTrail;

    [Header("Line Renderer (Shield)")]
    public LineRenderer shieldCircle;
    public int circleSegments = 32;
    public float shieldRadius = 1.5f;

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
        // 自动寻找玩家
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // 订阅术式事件
        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast += OnSpellCast;
        }

        // 初始化残影
        if (playerTrail != null)
        {
            playerTrail.enabled = false;
        }

        // 初始化护盾圆环
        if (shieldCircle != null)
        {
            InitializeShieldCircle();
            shieldCircle.enabled = false;
        }
    }

    void OnDestroy()
    {
        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast -= OnSpellCast;
        }
    }

    /// <summary>
    /// 术式释放回调
    /// </summary>
    private void OnSpellCast(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.DragonDragonDragon:
                PlayDragonFireEffect();
                break;
            case SpellType.OxOxOx:
                PlayGoldenShieldEffect();
                break;
            case SpellType.RabbitRabbitRabbit:
                PlaySpeedTrailEffect();
                break;
            case SpellType.DragonOxRabbit:
                PlayHealingEffect();
                break;
            default:
                PlayGenericEffect();
                break;
        }
    }

    #region Spell Effects

    /// <summary>
    /// 离火聚龙 - 火龙特效
    /// </summary>
    public void PlayDragonFireEffect()
    {
        Debug.Log("[Effects] 离火聚龙特效!");

        if (dragonFirePrefab != null && playerTransform != null)
        {
            // 生成火龙预制体
            GameObject effect = Instantiate(dragonFirePrefab, playerTransform.position, Quaternion.identity);
            Destroy(effect, dragonFireDuration);
        }
        else
        {
            // 简易粒子效果
            StartCoroutine(SimpleDragonEffect());
        }
    }

    private IEnumerator SimpleDragonEffect()
    {
        if (playerTransform == null) yield break;

        // 创建简易火焰粒子
        GameObject effectObj = new GameObject("DragonFireEffect");
        effectObj.transform.position = playerTransform.position;

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = dragonColor;
        main.startSize = 0.5f;
        main.startLifetime = 0.5f;
        main.startSpeed = 10f;
        main.duration = dragonFireDuration;
        main.loop = false;

        var emission = ps.emission;
        emission.rateOverTime = 50;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 30f;

        ps.Play();

        yield return new WaitForSeconds(dragonFireDuration);
        Destroy(effectObj);
    }

    /// <summary>
    /// 金刚不坏身 - 金色护盾特效
    /// </summary>
    public void PlayGoldenShieldEffect()
    {
        Debug.Log("[Effects] 金刚不坏身特效!");

        if (goldenShieldPrefab != null && playerTransform != null)
        {
            GameObject effect = Instantiate(goldenShieldPrefab, playerTransform);
            Destroy(effect, shieldDuration);
        }
        else if (shieldCircle != null)
        {
            StartCoroutine(ShieldCircleEffect());
        }
        else
        {
            StartCoroutine(SimpleShieldEffect());
        }
    }

    private IEnumerator ShieldCircleEffect()
    {
        shieldCircle.enabled = true;
        shieldCircle.startColor = oxColor;
        shieldCircle.endColor = new Color(oxColor.r, oxColor.g, oxColor.b, 0.5f);

        float elapsed = 0f;
        while (elapsed < shieldDuration)
        {
            elapsed += Time.deltaTime;
            
            // 更新圆环位置跟随玩家
            if (playerTransform != null)
            {
                UpdateShieldCirclePosition();
            }

            // 脉冲效果
            float pulse = 1 + 0.1f * Mathf.Sin(elapsed * 5f);
            UpdateShieldCircle(shieldRadius * pulse);

            yield return null;
        }

        shieldCircle.enabled = false;
    }

    private void InitializeShieldCircle()
    {
        shieldCircle.positionCount = circleSegments + 1;
        UpdateShieldCircle(shieldRadius);
    }

    private void UpdateShieldCircle(float radius)
    {
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (float)i / circleSegments * 360f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            shieldCircle.SetPosition(i, pos);
        }
    }

    private void UpdateShieldCirclePosition()
    {
        if (shieldCircle != null && playerTransform != null)
        {
            shieldCircle.transform.position = playerTransform.position;
        }
    }

    private IEnumerator SimpleShieldEffect()
    {
        // 创建简易护盾
        GameObject shieldObj = new GameObject("GoldenShield");
        SpriteRenderer sr = shieldObj.AddComponent<SpriteRenderer>();
        
        // 创建圆形精灵
        Texture2D tex = new Texture2D(64, 64);
        for (int x = 0; x < 64; x++)
        {
            for (int y = 0; y < 64; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(32, 32));
                if (dist > 28 && dist < 32)
                {
                    tex.SetPixel(x, y, oxColor);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 16);
        sr.color = oxColor;

        float elapsed = 0f;
        while (elapsed < shieldDuration)
        {
            elapsed += Time.deltaTime;
            
            if (playerTransform != null)
            {
                shieldObj.transform.position = playerTransform.position;
            }

            // 脉冲缩放
            float scale = 3f + 0.5f * Mathf.Sin(elapsed * 3f);
            shieldObj.transform.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(shieldObj);
    }

    /// <summary>
    /// 神行千里 - 移动残影特效
    /// </summary>
    public void PlaySpeedTrailEffect()
    {
        Debug.Log("[Effects] 神行千里特效!");

        if (speedTrailPrefab != null && playerTransform != null)
        {
            GameObject effect = Instantiate(speedTrailPrefab, playerTransform);
            Destroy(effect, speedTrailDuration);
        }
        else if (playerTrail != null)
        {
            StartCoroutine(TrailEffect());
        }
        else
        {
            StartCoroutine(SimpleSpeedEffect());
        }
    }

    private IEnumerator TrailEffect()
    {
        playerTrail.enabled = true;
        playerTrail.startColor = rabbitColor;
        playerTrail.endColor = new Color(rabbitColor.r, rabbitColor.g, rabbitColor.b, 0f);

        yield return new WaitForSeconds(speedTrailDuration);

        playerTrail.enabled = false;
    }

    private IEnumerator SimpleSpeedEffect()
    {
        // 周期性生成残影
        float elapsed = 0f;
        float spawnInterval = 0.05f;
        float nextSpawnTime = 0f;

        while (elapsed < speedTrailDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextSpawnTime && playerTransform != null)
            {
                SpawnAfterImage();
                nextSpawnTime = elapsed + spawnInterval;
            }

            yield return null;
        }
    }

    private void SpawnAfterImage()
    {
        if (playerTransform == null) return;

        GameObject afterImage = new GameObject("AfterImage");
        afterImage.transform.position = playerTransform.position;
        afterImage.transform.localScale = playerTransform.localScale;

        SpriteRenderer sr = afterImage.AddComponent<SpriteRenderer>();
        SpriteRenderer playerSR = playerTransform.GetComponent<SpriteRenderer>();
        
        if (playerSR != null)
        {
            sr.sprite = playerSR.sprite;
            sr.color = new Color(rabbitColor.r, rabbitColor.g, rabbitColor.b, 0.5f);
        }
        else
        {
            // 简易方块残影
            sr.color = new Color(rabbitColor.r, rabbitColor.g, rabbitColor.b, 0.5f);
        }

        // 淡出并销毁
        StartCoroutine(FadeOutAndDestroy(afterImage, 0.3f));
    }

    private IEnumerator FadeOutAndDestroy(GameObject obj, float duration)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(obj);
            yield break;
        }

        Color startColor = sr.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(obj);
    }

    /// <summary>
    /// 三元归一 - 治疗光环特效
    /// </summary>
    public void PlayHealingEffect()
    {
        Debug.Log("[Effects] 三元归一特效!");

        if (healingAuraPrefab != null && playerTransform != null)
        {
            GameObject effect = Instantiate(healingAuraPrefab, playerTransform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            StartCoroutine(SimpleHealingEffect());
        }
    }

    private IEnumerator SimpleHealingEffect()
    {
        if (playerTransform == null) yield break;

        // 创建治疗粒子
        GameObject effectObj = new GameObject("HealingEffect");
        effectObj.transform.position = playerTransform.position;

        ParticleSystem ps = effectObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = Color.green;
        main.startSize = 0.3f;
        main.startLifetime = 1f;
        main.startSpeed = 2f;
        main.duration = 1.5f;
        main.loop = false;
        main.gravityModifier = -0.5f;  // 向上飘

        var emission = ps.emission;
        emission.rateOverTime = 30;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1f;

        ps.Play();

        yield return new WaitForSeconds(2f);
        Destroy(effectObj);
    }

    /// <summary>
    /// 通用术式特效
    /// </summary>
    public void PlayGenericEffect()
    {
        Debug.Log("[Effects] 通用术式特效!");
        
        if (playerTransform != null)
        {
            StartCoroutine(GenericSpellEffect());
        }
    }

    private IEnumerator GenericSpellEffect()
    {
        // 简单的扩散圆环
        GameObject effectObj = new GameObject("GenericSpellEffect");
        effectObj.transform.position = playerTransform.position;

        SpriteRenderer sr = effectObj.AddComponent<SpriteRenderer>();
        
        // 创建圆形
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                if (dist > 12 && dist < 16)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 8);

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 扩大并淡出
            effectObj.transform.localScale = Vector3.one * (1f + t * 3f);
            sr.color = new Color(1, 1, 1, 1 - t);

            yield return null;
        }

        Destroy(effectObj);
    }

    #endregion
}
