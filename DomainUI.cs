using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 领域展开UI控制器 - 管理领域展开时的视觉效果
/// </summary>
public class DomainUI : MonoBehaviour
{
    [Header("Domain Overlay")]
    public Image domainOverlay;             // 全屏遮罩
    public Color domainColor = new Color(0f, 0f, 0.5f, 0.3f);  // 蓝色半透明
    
    [Header("Edge Glow")]
    public Image edgeGlow;                  // 边缘光晕
    public Color glowColor = Color.blue;
    public float glowPulseSpeed = 2f;
    public float glowMinAlpha = 0.3f;
    public float glowMaxAlpha = 0.8f;

    [Header("Timer Display")]
    public Text timerText;                  // 倒计时文字
    public Image timerFill;                 // 倒计时进度条
    public float domainDuration = 10f;

    [Header("Hint Display")]
    public Text hintText;                   // 提示文字
    public string hintMessage = "请结印 - 龙/牛/兔";
    
    [Header("Hand Sign Hints")]
    public Image[] handSignHintImages;      // 手印示意图

    [Header("Animation Settings")]
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;

    private bool isDomainActive = false;
    private float domainTimer = 0f;
    private Coroutine pulseCoroutine;

    void Start()
    {
        // 初始隐藏
        SetDomainUIActive(false);

        // 订阅事件
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged += OnGameStateChanged;
        }
    }

    void OnDestroy()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }

    void Update()
    {
        if (isDomainActive)
        {
            UpdateDomainTimer();
        }
    }

    /// <summary>
    /// 游戏状态变化回调
    /// </summary>
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        if (newState == GameState.DomainExpansion)
        {
            ActivateDomainUI();
        }
        else if (previousState == GameState.DomainExpansion)
        {
            DeactivateDomainUI();
        }
    }

    /// <summary>
    /// 激活领域UI
    /// </summary>
    public void ActivateDomainUI()
    {
        isDomainActive = true;
        domainTimer = domainDuration;
        
        SetDomainUIActive(true);
        StartCoroutine(FadeIn());
        
        // 开始光晕脉冲
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        pulseCoroutine = StartCoroutine(PulseGlow());

        // 设置提示文字
        if (hintText != null)
        {
            hintText.text = hintMessage;
        }
    }

    /// <summary>
    /// 关闭领域UI
    /// </summary>
    public void DeactivateDomainUI()
    {
        isDomainActive = false;
        
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        StartCoroutine(FadeOut());
    }

    /// <summary>
    /// 设置UI元素可见性
    /// </summary>
    private void SetDomainUIActive(bool active)
    {
        if (domainOverlay != null)
            domainOverlay.gameObject.SetActive(active);
        
        if (edgeGlow != null)
            edgeGlow.gameObject.SetActive(active);
        
        if (timerText != null)
            timerText.gameObject.SetActive(active);
        
        if (timerFill != null)
            timerFill.gameObject.SetActive(active);
        
        if (hintText != null)
            hintText.gameObject.SetActive(active);

        if (handSignHintImages != null)
        {
            foreach (var img in handSignHintImages)
            {
                if (img != null)
                    img.gameObject.SetActive(active);
            }
        }
    }

    /// <summary>
    /// 更新领域计时器
    /// </summary>
    private void UpdateDomainTimer()
    {
        domainTimer -= Time.unscaledDeltaTime;
        
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(domainTimer).ToString();
        }

        if (timerFill != null)
        {
            timerFill.fillAmount = domainTimer / domainDuration;
        }
    }

    /// <summary>
    /// 淡入效果
    /// </summary>
    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeInDuration;

            if (domainOverlay != null)
            {
                Color c = domainColor;
                c.a = domainColor.a * t;
                domainOverlay.color = c;
            }

            yield return null;
        }

        if (domainOverlay != null)
        {
            domainOverlay.color = domainColor;
        }
    }

    /// <summary>
    /// 淡出效果
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = domainOverlay != null ? domainOverlay.color : domainColor;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - (elapsed / fadeOutDuration);

            if (domainOverlay != null)
            {
                Color c = startColor;
                c.a = startColor.a * t;
                domainOverlay.color = c;
            }

            yield return null;
        }

        SetDomainUIActive(false);
    }

    /// <summary>
    /// 边缘光晕脉冲效果
    /// </summary>
    private IEnumerator PulseGlow()
    {
        while (isDomainActive)
        {
            if (edgeGlow != null)
            {
                float t = (Mathf.Sin(Time.unscaledTime * glowPulseSpeed) + 1f) / 2f;
                float alpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, t);
                
                Color c = glowColor;
                c.a = alpha;
                edgeGlow.color = c;
            }

            yield return null;
        }
    }

    /// <summary>
    /// 显示QTE提示
    /// </summary>
    public void ShowQTEHint(string sequence)
    {
        if (hintText != null)
        {
            hintText.text = $"结印对抗! {sequence}";
            hintText.color = Color.yellow;
        }
    }

    /// <summary>
    /// 设置领域持续时间
    /// </summary>
    public void SetDuration(float duration)
    {
        domainDuration = duration;
    }
}
