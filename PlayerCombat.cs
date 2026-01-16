using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerCombat : MonoBehaviour
{
    [Header("Energy System")]
    public float maxEnergy = 100f;
    public float currentEnergy = 0f;
    public float energyGainPerHit = 20f;
    public Slider energySlider;

    [Header("Domain Expansion")]
    public GameObject domainOverlay; // 拖入 Canvas 里的黑色 Panel
    public bool isDomainActive = false;
    public float domainDuration = 5f; // 领域持续时间
    private float domainTimer = 0f;

    void Start()
    {
        currentEnergy = 0;
        UpdateUI();
        if(domainOverlay) domainOverlay.SetActive(false);
    }

    void Update()
    {
        // 1. 测试用：左键点击模拟普通攻击命中
        if (Input.GetMouseButtonDown(0) && !isDomainActive)
        {
            GainEnergy(energyGainPerHit);
            Debug.Log("平A藏大！" + energyGainPerHit);
        }

        // 2. 开启术式展开 (F键)
        if (Input.GetKeyDown(KeyCode.F) && currentEnergy >= maxEnergy && !isDomainActive)
        {
            ActivateDomain();
        }

        // 3. 领域倒计时逻辑
        if (isDomainActive)
        {
            domainTimer = domainTimer - Time.unscaledDeltaTime; // 注意：子弹时间下要用 unscaled
            if (domainTimer <= 0)
            {
                DeactivateDomain();
            }
        }
    }

    void GainEnergy(float amount)
    {
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (energySlider) energySlider.value = currentEnergy / maxEnergy;
    }

    void ActivateDomain()
    {
        isDomainActive = true;
        currentEnergy = 0; // 消耗全部咒力
        domainTimer = domainDuration;
        UpdateUI();

        // --- 视觉表现 ---
        if(domainOverlay) domainOverlay.SetActive(true);
        domainOverlay.GetComponent<Image>().DOFade(0.8f, 0.5f).SetUpdate(true);
        
        // --- 子弹时间 ---
        Time.timeScale = 0.1f; // 游戏速度变慢 10 倍
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // 保证物理计算同步

        Debug.Log(">>> 术式展开：子午档案 <<<");
    }

    public void DeactivateDomain()
    {
        if (!isDomainActive) return;

        isDomainActive = false;
        
        // --- 恢复状态 ---
        if(domainOverlay) domainOverlay.SetActive(false);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        Debug.Log("<<< 术式终止 >>>");
    }
}