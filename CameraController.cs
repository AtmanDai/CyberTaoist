using UnityEngine;
using System.Collections;

/// <summary>
/// 相机控制器 - 管理相机跟随、震动和缩放效果
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10);
    public bool followTarget = true;

    [Header("Shake Settings")]
    public float defaultShakeDuration = 0.3f;
    public float defaultShakeMagnitude = 0.3f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 originalPosition;

    [Header("Zoom Settings")]
    public float defaultZoom = 5f;
    public float zoomedInAmount = 3f;
    public float zoomSpeed = 5f;
    private float targetZoom;
    private Camera cam;

    [Header("Domain Expansion Effect")]
    public float domainZoom = 4f;
    public float domainZoomDuration = 0.5f;

    [Header("Spell Cast Effect")]
    public float spellZoom = 2.5f;
    public float spellZoomDuration = 0.3f;

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
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }

        targetZoom = defaultZoom;
        if (cam != null)
        {
            cam.orthographicSize = defaultZoom;
        }

        // 自动寻找玩家
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        // 订阅事件
        SubscribeToEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    void LateUpdate()
    {
        // 跟随目标
        if (followTarget && target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;
        }

        // 应用震动效果
        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0;  // 2D游戏只在XY平面震动
            transform.position += shakeOffset;
            
            shakeDuration -= Time.unscaledDeltaTime;
        }

        // 应用缩放
        if (cam != null)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, zoomSpeed * Time.unscaledDeltaTime);
        }
    }

    /// <summary>
    /// 订阅事件
    /// </summary>
    private void SubscribeToEvents()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
        }

        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast += OnSpellCast;
            SpellSystem.Instance.OnSpellComplete += OnSpellComplete;
        }
    }

    /// <summary>
    /// 取消订阅事件
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
        }

        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast -= OnSpellCast;
            SpellSystem.Instance.OnSpellComplete -= OnSpellComplete;
        }
    }

    /// <summary>
    /// 游戏状态变化回调
    /// </summary>
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        switch (newState)
        {
            case GameState.DomainExpansion:
                ZoomTo(domainZoom, domainZoomDuration);
                break;
            case GameState.SpellCast:
                ZoomTo(spellZoom, spellZoomDuration);
                Shake(0.5f, 0.4f);
                break;
            case GameState.Normal:
                ZoomTo(defaultZoom, 0.5f);
                break;
        }
    }

    /// <summary>
    /// 术式释放回调
    /// </summary>
    private void OnSpellCast(SpellType spellType)
    {
        // 根据术式类型不同，产生不同强度的震动
        switch (spellType)
        {
            case SpellType.DragonDragonDragon:
                Shake(0.5f, 0.6f);  // 离火聚龙 - 强烈震动
                break;
            case SpellType.OxOxOx:
                Shake(0.2f, 0.2f);  // 金刚不坏身 - 轻微震动
                break;
            case SpellType.RabbitRabbitRabbit:
                Shake(0.1f, 0.1f);  // 神行千里 - 轻微震动
                break;
            default:
                Shake(0.3f, 0.3f);
                break;
        }
    }

    /// <summary>
    /// 术式完成回调
    /// </summary>
    private void OnSpellComplete()
    {
        ZoomTo(defaultZoom, 0.5f);
    }

    #region Public Methods

    /// <summary>
    /// 触发相机震动
    /// </summary>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        shakeDuration = duration < 0 ? defaultShakeDuration : duration;
        shakeMagnitude = magnitude < 0 ? defaultShakeMagnitude : magnitude;
    }

    /// <summary>
    /// 缩放到指定大小
    /// </summary>
    public void ZoomTo(float zoom, float duration = 0.5f)
    {
        targetZoom = zoom;
        // 如果需要即时缩放，可以直接设置
        // StartCoroutine(SmoothZoom(zoom, duration));
    }

    /// <summary>
    /// 立即设置缩放
    /// </summary>
    public void SetZoomImmediate(float zoom)
    {
        targetZoom = zoom;
        if (cam != null)
        {
            cam.orthographicSize = zoom;
        }
    }

    /// <summary>
    /// 重置到默认缩放
    /// </summary>
    public void ResetZoom()
    {
        targetZoom = defaultZoom;
    }

    /// <summary>
    /// 拉近镜头（用于术式演出）
    /// </summary>
    public void ZoomIn()
    {
        ZoomTo(zoomedInAmount);
    }

    /// <summary>
    /// 恢复默认镜头
    /// </summary>
    public void ZoomOut()
    {
        ZoomTo(defaultZoom);
    }

    /// <summary>
    /// 设置跟随目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// 启用/禁用跟随
    /// </summary>
    public void SetFollow(bool enable)
    {
        followTarget = enable;
    }

    #endregion

    /// <summary>
    /// 平滑缩放协程
    /// </summary>
    private IEnumerator SmoothZoom(float targetSize, float duration)
    {
        if (cam == null) yield break;

        float startSize = cam.orthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0, 1, t);
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        cam.orthographicSize = targetSize;
    }

    /// <summary>
    /// 慢动作进入时的镜头拉近
    /// </summary>
    public void DomainExpansionCamera()
    {
        ZoomTo(domainZoom, domainZoomDuration);
    }

    /// <summary>
    /// 伤害震动
    /// </summary>
    public void DamageShake()
    {
        Shake(0.15f, 0.2f);
    }

    /// <summary>
    /// 击杀震动
    /// </summary>
    public void KillShake()
    {
        Shake(0.25f, 0.35f);
    }
}
