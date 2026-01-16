using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 印记槽UI控制器 - 显示3个印记槽的可视化
/// </summary>
public class SealSlotUI : MonoBehaviour
{
    [Header("Slot References")]
    public Image[] slotImages;         // 3个槽位的图片
    public Text[] slotTexts;           // 3个槽位的文字(显示汉字)
    public Image[] slotFrames;         // 槽位边框

    [Header("Slot Colors")]
    public Color emptySlotColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
    public Color dragonColor = Color.red;
    public Color oxColor = new Color(1f, 0.84f, 0f); // 金色
    public Color rabbitColor = Color.blue;

    [Header("Animation Settings")]
    public float fillAnimDuration = 0.3f;
    public float pulseScale = 1.2f;
    public AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Seal Success Effect")]
    public GameObject sealSuccessEffectPrefab;
    public AudioClip sealSuccessSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 订阅事件
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSealAdded += OnSealAdded;
            SealSlotManager.Instance.OnSlotsCleared += OnSlotsCleared;
        }

        // 初始化所有槽位为空
        ClearAllSlots();
    }

    void OnDestroy()
    {
        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSealAdded -= OnSealAdded;
            SealSlotManager.Instance.OnSlotsCleared -= OnSlotsCleared;
        }
    }

    /// <summary>
    /// 印记添加回调
    /// </summary>
    private void OnSealAdded(int slotIndex, SealType sealType)
    {
        if (slotIndex < 0 || slotIndex >= slotImages.Length) return;

        // 设置槽位颜色和文字
        SetSlotContent(slotIndex, sealType);

        // 播放动画
        StartCoroutine(PlayFillAnimation(slotIndex));

        // 播放音效
        PlaySealSound();

        // 生成特效
        SpawnSealEffect(slotIndex);
    }

    /// <summary>
    /// 槽位清空回调
    /// </summary>
    private void OnSlotsCleared()
    {
        ClearAllSlots();
    }

    /// <summary>
    /// 设置槽位内容
    /// </summary>
    private void SetSlotContent(int index, SealType sealType)
    {
        Color color = GetSealColor(sealType);
        string text = SealSlotManager.GetSealChineseName(sealType);

        if (slotImages[index] != null)
        {
            slotImages[index].color = color;
        }

        if (slotTexts != null && index < slotTexts.Length && slotTexts[index] != null)
        {
            slotTexts[index].text = text;
            slotTexts[index].color = Color.white;
        }
    }

    /// <summary>
    /// 获取印记颜色
    /// </summary>
    private Color GetSealColor(SealType sealType)
    {
        switch (sealType)
        {
            case SealType.Dragon: return dragonColor;
            case SealType.Ox: return oxColor;
            case SealType.Rabbit: return rabbitColor;
            default: return emptySlotColor;
        }
    }

    /// <summary>
    /// 清空所有槽位
    /// </summary>
    public void ClearAllSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] != null)
            {
                slotImages[i].color = emptySlotColor;
            }

            if (slotTexts != null && i < slotTexts.Length && slotTexts[i] != null)
            {
                slotTexts[i].text = "";
            }
        }
    }

    /// <summary>
    /// 播放填充动画
    /// </summary>
    private IEnumerator PlayFillAnimation(int slotIndex)
    {
        if (slotImages[slotIndex] == null) yield break;

        Transform slotTransform = slotImages[slotIndex].transform;
        Vector3 originalScale = slotTransform.localScale;
        
        float elapsed = 0f;

        while (elapsed < fillAnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;  // 使用unscaledTime因为可能在子弹时间
            float t = elapsed / fillAnimDuration;
            
            // 缩放动画
            float scale = 1f + (pulseScale - 1f) * Mathf.Sin(t * Mathf.PI);
            slotTransform.localScale = originalScale * scale;

            yield return null;
        }

        slotTransform.localScale = originalScale;
    }

    /// <summary>
    /// 播放印记成功音效
    /// </summary>
    private void PlaySealSound()
    {
        if (audioSource != null && sealSuccessSound != null)
        {
            audioSource.PlayOneShot(sealSuccessSound);
        }
    }

    /// <summary>
    /// 生成印记成功特效
    /// </summary>
    private void SpawnSealEffect(int slotIndex)
    {
        if (sealSuccessEffectPrefab == null) return;
        if (slotImages[slotIndex] == null) return;

        Vector3 position = slotImages[slotIndex].transform.position;
        GameObject effect = Instantiate(sealSuccessEffectPrefab, position, Quaternion.identity, transform);
        Destroy(effect, 2f);
    }

    /// <summary>
    /// 更新槽位显示（手动刷新）
    /// </summary>
    public void RefreshDisplay()
    {
        if (SealSlotManager.Instance == null) return;

        for (int i = 0; i < slotImages.Length; i++)
        {
            SealType sealType = SealSlotManager.Instance.GetSealAt(i);
            if (sealType != SealType.None)
            {
                SetSlotContent(i, sealType);
            }
            else
            {
                if (slotImages[i] != null)
                {
                    slotImages[i].color = emptySlotColor;
                }
                if (slotTexts != null && i < slotTexts.Length && slotTexts[i] != null)
                {
                    slotTexts[i].text = "";
                }
            }
        }
    }
}
