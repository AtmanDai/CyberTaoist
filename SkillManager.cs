using UnityEngine;

/// <summary>
/// Simplified Skill Manager - Works with new MediaPipe gesture system
/// Handles visual feedback for gesture-based spell casting
/// </summary>
public class SkillManager : MonoBehaviour
{
    public HandSignReceiver receiver;
    public Material activeMaterial;
    public Material defaultMaterial;
    private Renderer rend;
    private float resetTimer = 0f;

    [Header("Skill Feedback")]
    public float feedbackDuration = 0.3f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (defaultMaterial == null && rend != null)
        {
            defaultMaterial = rend.material;
        }

        // Subscribe to spell cast events
        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast += OnSpellCast;
        }
    }

    void OnDestroy()
    {
        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast -= OnSpellCast;
        }
    }

    void Update()
    {
        // Reset visual effect after duration
        if (resetTimer > 0)
        {
            resetTimer -= Time.deltaTime;
            if (resetTimer <= 0 && rend != null && defaultMaterial != null)
            {
                rend.material = defaultMaterial;
            }
        }
    }

    /// <summary>
    /// Visual feedback when spell is cast
    /// </summary>
    private void OnSpellCast(SpellType spellType)
    {
        if (rend == null || activeMaterial == null) return;

        // Change material temporarily for visual feedback
        rend.material = activeMaterial;
        resetTimer = feedbackDuration;

        // Log spell cast
        string spellName = SealSlotManager.GetSpellChineseName(spellType);
        Debug.Log($">>> Spell Cast: {spellName} <<<");
    }
}
