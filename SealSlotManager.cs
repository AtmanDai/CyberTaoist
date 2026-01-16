using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 印记类型枚举 - 对应三种基础手印
/// </summary>
public enum SealType
{
    None = 0,
    Dragon = 1,  // 龙印(辰): 攻击属性, 红色
    Ox = 2,      // 牛印(丑): 防御属性, 金色
    Rabbit = 3   // 兔印(卯): 机动属性, 蓝色
}

/// <summary>
/// 术式类型枚举 - 9种术式组合
/// </summary>
public enum SpellType
{
    None,
    // 三同印 - 效果最强
    DragonDragonDragon,  // 离火聚龙 - 火龙特效, 200伤害
    OxOxOx,              // 金刚不坏身 - 5秒无敌+反伤
    RabbitRabbitRabbit,  // 神行千里 - 移速x3
    
    // 双同印 - 兼顾两种需求
    DragonDragonOx,      // 待补充
    DragonDragonRabbit,  // 待补充
    OxOxDragon,          // 待补充
    OxOxRabbit,          // 待补充
    RabbitRabbitDragon,  // 待补充
    RabbitRabbitOx,      // 待补充
    
    // 三异印 - 功能性技能
    DragonOxRabbit       // 三元归一 - 回血50%
}

/// <summary>
/// 印记槽管理器 - 管理3个印记槽的核心系统
/// </summary>
public class SealSlotManager : MonoBehaviour
{
    public static SealSlotManager Instance { get; private set; }

    [Header("Slot Settings")]
    public int maxSlots = 3;
    private SealType[] sealSlots;
    private int currentSlotIndex = 0;

    // 事件
    public event Action<int, SealType> OnSealAdded;       // 印记添加事件
    public event Action OnSlotsCleared;                    // 印记槽清空事件
    public event Action<SpellType> OnSpellDetermined;      // 术式确定事件

    [Header("Hand Sign ID Mapping")]
    // 对应 labels.csv 中的ID映射
    // 5: Tatsu(Dragon) -> 辰
    // 2: Ushi(Ox) -> 丑  
    // 4: U(Hare) -> 卯
    // 13: Gassho -> 祈
    public int dragonSignID = 5;
    public int oxSignID = 2;
    public int rabbitSignID = 4;
    public int endSealSignID = 13; // 祈印，用于提前结束

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
        sealSlots = new SealType[maxSlots];
        ClearSlots();
    }

    /// <summary>
    /// 根据手势ID添加印记
    /// </summary>
    public bool TryAddSealBySignID(int signID)
    {
        SealType sealType = ConvertSignIDToSealType(signID);
        
        if (sealType == SealType.None)
        {
            // 检查是否是祈印（结束信号）
            if (signID == endSealSignID)
            {
                TriggerSpellRelease();
                return true;
            }
            return false;
        }

        return AddSeal(sealType);
    }

    /// <summary>
    /// 将手势ID转换为印记类型
    /// </summary>
    private SealType ConvertSignIDToSealType(int signID)
    {
        if (signID == dragonSignID) return SealType.Dragon;
        if (signID == oxSignID) return SealType.Ox;
        if (signID == rabbitSignID) return SealType.Rabbit;
        return SealType.None;
    }

    /// <summary>
    /// 添加印记到槽位
    /// </summary>
    public bool AddSeal(SealType sealType)
    {
        if (currentSlotIndex >= maxSlots)
        {
            Debug.Log("[SealSlot] 印记槽已满!");
            return false;
        }

        sealSlots[currentSlotIndex] = sealType;
        OnSealAdded?.Invoke(currentSlotIndex, sealType);
        
        Debug.Log($"[SealSlot] 第{currentSlotIndex + 1}槽填入: {GetSealChineseName(sealType)}");
        
        currentSlotIndex++;

        // 检查是否已满，如果满了自动触发术式
        if (currentSlotIndex >= maxSlots)
        {
            TriggerSpellRelease();
        }

        return true;
    }

    /// <summary>
    /// 触发术式释放
    /// </summary>
    public void TriggerSpellRelease()
    {
        if (currentSlotIndex == 0)
        {
            Debug.Log("[SealSlot] 没有印记，无法释放术式");
            return;
        }

        SpellType spell = DetermineSpell();
        OnSpellDetermined?.Invoke(spell);
        
        Debug.Log($"[SealSlot] 术式确定: {spell}");
    }

    /// <summary>
    /// 根据印记组合确定术式
    /// </summary>
    private SpellType DetermineSpell()
    {
        // 如果印记不满3个，返回None
        if (currentSlotIndex < maxSlots)
        {
            // 可以选择根据已有印记释放弱化版术式，这里先返回None
            return SpellType.None;
        }

        SealType s1 = sealSlots[0];
        SealType s2 = sealSlots[1];
        SealType s3 = sealSlots[2];

        // 三同印判定
        if (s1 == s2 && s2 == s3)
        {
            switch (s1)
            {
                case SealType.Dragon: return SpellType.DragonDragonDragon;
                case SealType.Ox: return SpellType.OxOxOx;
                case SealType.Rabbit: return SpellType.RabbitRabbitRabbit;
            }
        }

        // 三异印判定 (任意顺序)
        if (HasAllThreeSeals(s1, s2, s3))
        {
            return SpellType.DragonOxRabbit; // 三元归一
        }

        // 双同印判定
        return DetermineDualSealSpell(s1, s2, s3);
    }

    private bool HasAllThreeSeals(SealType s1, SealType s2, SealType s3)
    {
        HashSet<SealType> seals = new HashSet<SealType> { s1, s2, s3 };
        return seals.Contains(SealType.Dragon) && 
               seals.Contains(SealType.Ox) && 
               seals.Contains(SealType.Rabbit);
    }

    private SpellType DetermineDualSealSpell(SealType s1, SealType s2, SealType s3)
    {
        int dragonCount = CountSealType(SealType.Dragon, s1, s2, s3);
        int oxCount = CountSealType(SealType.Ox, s1, s2, s3);
        int rabbitCount = CountSealType(SealType.Rabbit, s1, s2, s3);

        // 两龙一X
        if (dragonCount == 2)
        {
            if (oxCount == 1) return SpellType.DragonDragonOx;
            if (rabbitCount == 1) return SpellType.DragonDragonRabbit;
        }
        // 两牛一X
        if (oxCount == 2)
        {
            if (dragonCount == 1) return SpellType.OxOxDragon;
            if (rabbitCount == 1) return SpellType.OxOxRabbit;
        }
        // 两兔一X
        if (rabbitCount == 2)
        {
            if (dragonCount == 1) return SpellType.RabbitRabbitDragon;
            if (oxCount == 1) return SpellType.RabbitRabbitOx;
        }

        return SpellType.None;
    }

    private int CountSealType(SealType target, params SealType[] seals)
    {
        int count = 0;
        foreach (var seal in seals)
        {
            if (seal == target) count++;
        }
        return count;
    }

    /// <summary>
    /// 清空印记槽
    /// </summary>
    public void ClearSlots()
    {
        for (int i = 0; i < maxSlots; i++)
        {
            sealSlots[i] = SealType.None;
        }
        currentSlotIndex = 0;
        OnSlotsCleared?.Invoke();
        
        Debug.Log("[SealSlot] 印记槽已清空");
    }

    /// <summary>
    /// 获取当前已填充的印记数量
    /// </summary>
    public int GetFilledSlotCount()
    {
        return currentSlotIndex;
    }

    /// <summary>
    /// 获取印记对应的中文名称
    /// </summary>
    public static string GetSealChineseName(SealType sealType)
    {
        switch (sealType)
        {
            case SealType.Dragon: return "辰";
            case SealType.Ox: return "丑";
            case SealType.Rabbit: return "卯";
            default: return "";
        }
    }

    /// <summary>
    /// 获取印记对应的颜色
    /// </summary>
    public static Color GetSealColor(SealType sealType)
    {
        switch (sealType)
        {
            case SealType.Dragon: return Color.red;
            case SealType.Ox: return new Color(1f, 0.84f, 0f); // 金色
            case SealType.Rabbit: return Color.blue;
            default: return Color.gray;
        }
    }

    /// <summary>
    /// 获取指定槽位的印记
    /// </summary>
    public SealType GetSealAt(int index)
    {
        if (index >= 0 && index < maxSlots)
        {
            return sealSlots[index];
        }
        return SealType.None;
    }
}
