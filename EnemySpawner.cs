using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 敌人生成器 - 管理敌人波次和生成
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs")]
    public GameObject glitchPrefab;       // 故障者
    public GameObject cyberAxemanPrefab;  // 赛博斧手
    public GameObject reconstructorPrefab; // 重构体
    public GameObject bossPrefab;          // Boss

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;        // 生成点（屏幕边缘）
    public bool autoSpawn = true;
    
    [Header("Spawn Timers")]
    public float glitchInterval = 3f;      // 故障者每3秒1个
    public float axemanInterval = 5f;      // 赛博斧手每5秒1个
    public float reconstructorInterval = 20f; // 重构体每20秒1个
    
    private float glitchTimer = 0f;
    private float axemanTimer = 0f;
    private float reconstructorTimer = 0f;

    [Header("Spawn Limits")]
    public int maxGlitches = 10;
    public int maxAxemen = 5;
    public int maxReconstructors = 2;

    private List<GameObject> activeEnemies = new List<GameObject>();

    [Header("Boss Settings")]
    public bool bossSpawned = false;
    public float bossSpawnDelay = 60f;     // 60秒后生成Boss
    private float bossTimer = 0f;

    [Header("Wave System (Optional)")]
    public bool useWaveSystem = false;
    public int currentWave = 0;
    public int enemiesPerWave = 5;
    public float timeBetweenWaves = 10f;

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
        // 初始化计时器
        glitchTimer = glitchInterval;
        axemanTimer = axemanInterval;
        reconstructorTimer = reconstructorInterval;
        bossTimer = bossSpawnDelay;

        // 如果没有设置生成点，创建默认生成点
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            CreateDefaultSpawnPoints();
        }
    }

    void Update()
    {
        if (!autoSpawn) return;

        // 清理已销毁的敌人
        CleanupDestroyedEnemies();

        // 更新生成计时器
        UpdateSpawnTimers();

        // Boss生成计时
        if (!bossSpawned)
        {
            bossTimer -= Time.deltaTime;
            if (bossTimer <= 0)
            {
                SpawnBoss();
            }
        }
    }

    /// <summary>
    /// 更新生成计时器
    /// </summary>
    private void UpdateSpawnTimers()
    {
        // 故障者生成
        glitchTimer -= Time.deltaTime;
        if (glitchTimer <= 0 && CountEnemiesOfType<GlitchEnemy>() < maxGlitches)
        {
            SpawnEnemy(EnemyType.Glitch);
            glitchTimer = glitchInterval;
        }

        // 赛博斧手生成
        axemanTimer -= Time.deltaTime;
        if (axemanTimer <= 0 && CountEnemiesOfType<CyberAxeman>() < maxAxemen)
        {
            SpawnEnemy(EnemyType.CyberAxeman);
            axemanTimer = axemanInterval;
        }

        // 重构体生成
        reconstructorTimer -= Time.deltaTime;
        if (reconstructorTimer <= 0 && CountEnemiesOfType<ReconstructorEnemy>() < maxReconstructors)
        {
            SpawnEnemy(EnemyType.Reconstructor);
            reconstructorTimer = reconstructorInterval;
        }
    }

    /// <summary>
    /// 敌人类型枚举
    /// </summary>
    public enum EnemyType
    {
        Glitch,
        CyberAxeman,
        Reconstructor,
        Boss
    }

    /// <summary>
    /// 生成敌人
    /// </summary>
    public GameObject SpawnEnemy(EnemyType type)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null)
        {
            Debug.LogWarning($"[Spawner] 缺少 {type} 预制体!");
            return null;
        }

        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);

        Debug.Log($"[Spawner] 生成 {type} 在 {spawnPos}");
        return enemy;
    }

    /// <summary>
    /// 在指定位置生成敌人
    /// </summary>
    public GameObject SpawnEnemyAt(EnemyType type, Vector2 position)
    {
        GameObject prefab = GetPrefabForType(type);
        if (prefab == null) return null;

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        activeEnemies.Add(enemy);
        return enemy;
    }

    /// <summary>
    /// 生成Boss
    /// </summary>
    public void SpawnBoss()
    {
        if (bossSpawned || bossPrefab == null)
        {
            Debug.LogWarning("[Spawner] Boss已生成或缺少预制体!");
            return;
        }

        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        bossSpawned = true;

        // 订阅Boss死亡事件
        BossController bossController = boss.GetComponent<BossController>();
        if (bossController != null)
        {
            bossController.OnBossDeath += OnBossDefeated;
        }

        Debug.Log("[Spawner] Boss出现! 赛博恶灵·参孙重锤!");
    }

    /// <summary>
    /// Boss被击败回调
    /// </summary>
    private void OnBossDefeated()
    {
        Debug.Log("[Spawner] Boss被击败! 战斗胜利!");
        // TODO: 触发胜利事件
    }

    /// <summary>
    /// 获取敌人类型对应的预制体
    /// </summary>
    private GameObject GetPrefabForType(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Glitch: return glitchPrefab;
            case EnemyType.CyberAxeman: return cyberAxemanPrefab;
            case EnemyType.Reconstructor: return reconstructorPrefab;
            case EnemyType.Boss: return bossPrefab;
            default: return null;
        }
    }

    /// <summary>
    /// 获取随机生成位置
    /// </summary>
    private Vector2 GetRandomSpawnPosition()
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = Random.Range(0, spawnPoints.Length);
            return spawnPoints[index].position;
        }

        // 如果没有生成点，在屏幕边缘随机生成
        Camera cam = Camera.main;
        if (cam != null)
        {
            int side = Random.Range(0, 4);  // 0=左, 1=右, 2=上, 3=下
            Vector2 viewportPos = Vector2.zero;

            switch (side)
            {
                case 0: // 左
                    viewportPos = new Vector2(-0.1f, Random.value);
                    break;
                case 1: // 右
                    viewportPos = new Vector2(1.1f, Random.value);
                    break;
                case 2: // 上
                    viewportPos = new Vector2(Random.value, 1.1f);
                    break;
                case 3: // 下
                    viewportPos = new Vector2(Random.value, -0.1f);
                    break;
            }

            return cam.ViewportToWorldPoint(viewportPos);
        }

        // 默认位置
        return new Vector2(Random.Range(-10f, 10f), Random.Range(-5f, 5f));
    }

    /// <summary>
    /// 创建默认生成点
    /// </summary>
    private void CreateDefaultSpawnPoints()
    {
        // 创建4个屏幕边缘的生成点
        GameObject spawnPointsParent = new GameObject("SpawnPoints");
        spawnPointsParent.transform.parent = transform;

        Camera cam = Camera.main;
        if (cam == null) return;

        List<Transform> points = new List<Transform>();

        // 屏幕四边各两个点
        Vector2[] viewportPositions = {
            new Vector2(-0.1f, 0.3f),
            new Vector2(-0.1f, 0.7f),
            new Vector2(1.1f, 0.3f),
            new Vector2(1.1f, 0.7f),
            new Vector2(0.3f, 1.1f),
            new Vector2(0.7f, 1.1f),
            new Vector2(0.3f, -0.1f),
            new Vector2(0.7f, -0.1f)
        };

        for (int i = 0; i < viewportPositions.Length; i++)
        {
            GameObject point = new GameObject($"SpawnPoint_{i}");
            point.transform.parent = spawnPointsParent.transform;
            point.transform.position = cam.ViewportToWorldPoint(new Vector3(
                viewportPositions[i].x, 
                viewportPositions[i].y, 
                10
            ));
            points.Add(point.transform);
        }

        spawnPoints = points.ToArray();
    }

    /// <summary>
    /// 统计特定类型敌人数量
    /// </summary>
    private int CountEnemiesOfType<T>() where T : Component
    {
        int count = 0;
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null && enemy.GetComponent<T>() != null)
            {
                count++;
            }
        }
        return count;
    }

    /// <summary>
    /// 清理已销毁的敌人
    /// </summary>
    private void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(e => e == null);
    }

    /// <summary>
    /// 清除所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    /// <summary>
    /// 获取活跃敌人数量
    /// </summary>
    public int GetActiveEnemyCount()
    {
        CleanupDestroyedEnemies();
        return activeEnemies.Count;
    }

    /// <summary>
    /// 暂停/恢复自动生成
    /// </summary>
    public void SetAutoSpawn(bool enable)
    {
        autoSpawn = enable;
    }
}
