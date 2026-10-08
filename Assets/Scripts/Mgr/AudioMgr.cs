using UnityEngine;

/// <summary>
/// 全局音效管理器
/// </summary>
public class AudioMgr : MonoBehaviour
{
    public static AudioMgr Instance;

    [Header("背景音乐音源")]
    public AudioSource bgmSource;
    [Header("全局音效音源")]
    public AudioSource sfxSource;
    [Header("按钮点击音效")]
    public AudioClip btnClickClip;
    [Header("BGM")]
    public AudioClip BGMClip;

    private void Awake()
    {
        // 单例
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        //事件监听
        EventMgr.Instance.AddListener(EventConst.ButtonClicked, PlayBtnClick);
        EventMgr.Instance.AddListener(EventConst.FinishDirection, PlayBGM);
    }
    // 播放按钮音效
    public void PlayBtnClick()
    {
        PlaySFX(btnClickClip);
    }

    #region 背景音乐
    public void PlayBGM()
    {
        if (bgmSource == null) return;
        bgmSource.clip = BGMClip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource?.Stop();
    }
    #endregion

    #region 全局音效
    // 单次短音效（开枪、切枪、受伤）
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume);
    }
    #endregion
    public void OnDestroy()
    {
        // 移除事件监听
        EventMgr.Instance.RemoveListener(EventConst.ButtonClicked, PlayBtnClick);
    }
}