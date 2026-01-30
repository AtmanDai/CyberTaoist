using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Gesture IDs from MediaPipe detection (matching mediapipe_demo.py)
/// </summary>
public static class GestureID
{
    public const int None = 0;
    public const int Rock = 1;       // Defense
    public const int ThumbsUp = 2;   // Fireballs
    public const int Fist = 3;       // Normal attack
}

/// <summary>
/// Simplified spell types - three gestures with immediate release
/// </summary>
public enum SpellType
{
    None,
    Rock,      // Defense - protective barrier
    ThumbsUp,  // Fireballs - ranged attack
    Fist       // Normal attack - melee attack
}

/// <summary>
/// Legacy seal types (kept for backward compatibility)
/// </summary>
public enum SealType
{
    None = 0,
    Dragon = 1,
    Ox = 2,
    Rabbit = 3
}

/// <summary>
/// Simplified Gesture Manager - replaces the old SealSlotManager
/// No seal slots, just direct gesture to spell mapping with immediate release
/// </summary>
public class SealSlotManager : MonoBehaviour
{
    public static SealSlotManager Instance { get; private set; }

    // Events
    public event Action<SpellType> OnSpellDetermined;

    [Header("Gesture Mapping")]
    public int rockGestureID = GestureID.Rock;
    public int thumbsUpGestureID = GestureID.ThumbsUp;
    public int fistGestureID = GestureID.Fist;

    // Legacy compatibility
    [Header("Legacy Settings (Deprecated)")]
    public int maxSlots = 3;
    public int dragonSignID = 5;
    public int oxSignID = 2;
    public int rabbitSignID = 4;
    public int endSealSignID = 13;

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

    /// <summary>
    /// Process gesture from MediaPipe and trigger spell immediately
    /// </summary>
    public bool TryAddSealBySignID(int signID)
    {
        SpellType spell = ConvertGestureToSpell(signID);
        
        if (spell != SpellType.None)
        {
            // Trigger spell immediately
            OnSpellDetermined?.Invoke(spell);
            Debug.Log($"[SealSlotManager] Gesture detected: {spell}");
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Convert MediaPipe gesture ID to spell type
    /// </summary>
    private SpellType ConvertGestureToSpell(int gestureID)
    {
        if (gestureID == rockGestureID) return SpellType.Rock;
        if (gestureID == thumbsUpGestureID) return SpellType.ThumbsUp;
        if (gestureID == fistGestureID) return SpellType.Fist;
        return SpellType.None;
    }

    /// <summary>
    /// Get spell name in Chinese
    /// </summary>
    public static string GetSpellChineseName(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Rock: return "岩守 (Rock Defense)";
            case SpellType.ThumbsUp: return "焰弹 (Fireball)";
            case SpellType.Fist: return "拳击 (Power Strike)";
            default: return "";
        }
    }

    /// <summary>
    /// Get spell color
    /// </summary>
    public static Color GetSpellColor(SpellType spellType)
    {
        switch (spellType)
        {
            case SpellType.Rock: return new Color(1f, 0.84f, 0f); // Gold
            case SpellType.ThumbsUp: return Color.red;
            case SpellType.Fist: return Color.white;
            default: return Color.gray;
        }
    }

    // Legacy methods for backward compatibility (deprecated)
    [System.Obsolete("TriggerSpellRelease is deprecated. Spells are now cast immediately via TryAddSealBySignID.")]
    public void TriggerSpellRelease() 
    { 
        Debug.LogWarning("[SealSlotManager] TriggerSpellRelease is deprecated. Use TryAddSealBySignID for immediate spell casting.");
    }
    
    [System.Obsolete("ClearSlots is deprecated. Seal slots are no longer used.")]
    public void ClearSlots() 
    { 
        Debug.LogWarning("[SealSlotManager] ClearSlots is deprecated. Seal slots are no longer used.");
    }
    
    [System.Obsolete("GetFilledSlotCount is deprecated. Seal slots are no longer used.")]
    public int GetFilledSlotCount() { return 0; }
    
    [System.Obsolete("GetSealAt is deprecated. Seal slots are no longer used.")]
    public SealType GetSealAt(int index) { return SealType.None; }
    
    [System.Obsolete("GetSealChineseName is deprecated. Use GetSpellChineseName instead.")]
    public static string GetSealChineseName(SealType sealType) { return ""; }
    
    [System.Obsolete("GetSealColor is deprecated. Use GetSpellColor instead.")]
    public static Color GetSealColor(SealType sealType) { return Color.gray; }
}
