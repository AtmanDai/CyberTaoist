using UnityEngine;

/// <summary>
/// 音频管理器 - 管理所有游戏音效
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;           // 音效播放器
    public AudioSource bgmSource;           // 背景音乐播放器

    [Header("Sound Effects")]
    public AudioClip sealSuccessClip;       // 结印成功音效
    public AudioClip spellCastClip;         // 术式释放音效
    public AudioClip enemyHitClip;          // 敌人受击音效
    public AudioClip playerHitClip;         // 玩家受伤音效
    public AudioClip attackClip;            // 攻击音效
    public AudioClip domainEnterClip;       // 领域展开音效
    public AudioClip domainExitClip;        // 领域关闭音效
    public AudioClip victoryClip;           // 胜利音效
    public AudioClip defeatClip;            // 失败音效
    public AudioClip bossRoarClip;          // Boss咆哮音效

    [Header("Background Music")]
    public AudioClip normalBGM;             // 普通战斗BGM
    public AudioClip bossBGM;               // Boss战BGM

    [Header("Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    [Header("Domain Slow Effect")]
    public float domainPitchMultiplier = 0.5f;  // 领域展开时音高降低

    private float originalPitch = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 创建AudioSource组件
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }
    }

    void Start()
    {
        // 订阅事件
        SubscribeToEvents();

        // 开始播放背景音乐
        PlayBGM(normalBGM);
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
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

        if (SealSlotManager.Instance != null)
        {
            SealSlotManager.Instance.OnSealAdded += (index, seal) => PlaySFX(sealSuccessClip);
        }

        if (SpellSystem.Instance != null)
        {
            SpellSystem.Instance.OnSpellCast += (spell) => PlaySFX(spellCastClip);
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
    }

    /// <summary>
    /// 游戏状态变化回调
    /// </summary>
    private void OnGameStateChanged(GameState previousState, GameState newState)
    {
        switch (newState)
        {
            case GameState.DomainExpansion:
                PlaySFX(domainEnterClip);
                SetBGMPitch(domainPitchMultiplier);
                break;
            case GameState.Normal:
                if (previousState == GameState.DomainExpansion)
                {
                    PlaySFX(domainExitClip);
                }
                SetBGMPitch(1f);
                break;
        }
    }

    #region SFX Methods

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// 播放音效（指定音量）
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, volume * sfxVolume);
    }

    /// <summary>
    /// 播放结印成功音效
    /// </summary>
    public void PlaySealSuccess()
    {
        PlaySFX(sealSuccessClip);
    }

    /// <summary>
    /// 播放术式释放音效
    /// </summary>
    public void PlaySpellCast()
    {
        PlaySFX(spellCastClip);
    }

    /// <summary>
    /// 播放敌人受击音效
    /// </summary>
    public void PlayEnemyHit()
    {
        PlaySFX(enemyHitClip);
    }

    /// <summary>
    /// 播放玩家受伤音效
    /// </summary>
    public void PlayPlayerHit()
    {
        PlaySFX(playerHitClip);
    }

    /// <summary>
    /// 播放攻击音效
    /// </summary>
    public void PlayAttack()
    {
        PlaySFX(attackClip);
    }

    /// <summary>
    /// 播放胜利音效
    /// </summary>
    public void PlayVictory()
    {
        PlaySFX(victoryClip);
        StopBGM();
    }

    /// <summary>
    /// 播放失败音效
    /// </summary>
    public void PlayDefeat()
    {
        PlaySFX(defeatClip);
        StopBGM();
    }

    /// <summary>
    /// 播放Boss咆哮
    /// </summary>
    public void PlayBossRoar()
    {
        PlaySFX(bossRoarClip);
    }

    #endregion

    #region BGM Methods

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        
        if (bgmSource.clip != clip)
        {
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Pause();
        }
    }

    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.UnPause();
        }
    }

    /// <summary>
    /// 设置BGM音高（用于慢动作效果）
    /// </summary>
    public void SetBGMPitch(float pitch)
    {
        if (bgmSource != null)
        {
            bgmSource.pitch = pitch;
        }
    }

    /// <summary>
    /// 切换到Boss战BGM
    /// </summary>
    public void PlayBossBGM()
    {
        PlayBGM(bossBGM);
    }

    /// <summary>
    /// 切换到普通BGM
    /// </summary>
    public void PlayNormalBGM()
    {
        PlayBGM(normalBGM);
    }

    #endregion

    #region Volume Control

    /// <summary>
    /// 设置音效音量
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 设置背景音乐音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// 静音所有音频
    /// </summary>
    public void MuteAll(bool mute)
    {
        if (sfxSource != null) sfxSource.mute = mute;
        if (bgmSource != null) bgmSource.mute = mute;
    }

    #endregion
}
