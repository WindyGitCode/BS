using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    private GameData gameData;

    public Transform SoundPanel;//音效面板
    public Transform InputPanel;//键位面板
    public Button btnSound;//音效面板按钮
    public Button btnInput;//键位面板按钮
    public Button btnExit;

    //音效面板控件
    public Toggle toggleMusic;
    public Toggle toggleSound;
    public Slider sliderMusic;
    public Slider sliderSound;

    //键位面板控件
    public Transform tip;//修改键位时提示
    public Button btnSkill;//修改技能键位的按钮
    public Text skillKey;//显示目前键位
    public Button btnReload;//修改装填弹药键位的按钮
    public Text ReloadKey;//显示目前键位
    public Button btnInteract;       // 交互
    public Text interactKey;
    public Button btnRoll;           // 翻滚
    public Text rollKey;
    public Button btnDialog;         // 对话
    public Text dialogKey;
    public Button btnPause;          // 暂停
    public Text pauseKey;
    public Button btnUseHPItem;      // 使用医疗包
    public Text useHPItemKey;
    public Button btnUseSPItem;      // 使用体力剂
    public Text useSPItemKey;

    // 内部状态
    private bool isSettingSkillKey = false;
    private bool isSettingReloadKey = false;
    private bool isSettingInteractKey = false;
    private bool isSettingRollKey = false;
    private bool isSettingDialogKey = false;
    private bool isSettingPauseKey = false;
    private bool isSettingUseHPItemKey = false;
    private bool isSettingUseSPItemKey = false;

    // ====================== 【默认键位】 ======================
    private KeyCode defaultSkillKey = KeyCode.Q;
    private KeyCode defaultReloadKey = KeyCode.R;
    private KeyCode defaultInterRactKey = KeyCode.F;
    private KeyCode defaultRollKey = KeyCode.LeftShift;
    private KeyCode defaultDialogKey = KeyCode.Space;
    private KeyCode defaultPauseKey = KeyCode.P;
    private KeyCode defaultUseHPItemKey = KeyCode.Alpha1;
    private KeyCode defaultUseSPItemKey = KeyCode.Alpha2;

    // ====================== 【当前使用的键位】 ======================
    public static KeyCode CurrentSkillKey;
    public static KeyCode CurrentReloadKey;
    public static KeyCode CurrentInteractKey;
    public static KeyCode CurrentRollKey;
    public static KeyCode CurrentDialogKey;
    public static KeyCode CurrentPauseKey;
    public static KeyCode CurrentUseHPItemKey;
    public static KeyCode CurrentUseSPItemKey;
    public override void Init()
    {
        gameData=GameDataMgr.Instance.GetGameData();
        // 加载所有保存的键位
        CurrentSkillKey = gameData.skillKey;
        CurrentReloadKey = gameData.reloadKey;
        CurrentInteractKey = gameData.interactKey;
        CurrentRollKey = gameData.rollKey;
        CurrentDialogKey = gameData.dialogKey;
        CurrentPauseKey = gameData.pauseKey;
        CurrentUseHPItemKey = gameData.useHPItemKey;
        CurrentUseSPItemKey = gameData.useSPItemKey;
        //键位按钮监听
        btnSkill.onClick.AddListener(StartSetSkillKey);
        btnReload.onClick.AddListener(StartSetReloadKey);
        btnInteract.onClick.AddListener(StartSetInteractKey);
        btnRoll.onClick.AddListener(StartSetRollKey);
        btnDialog.onClick.AddListener(StartSetDialogKey);
        btnPause.onClick.AddListener(StartSetPauseKey);
        btnUseHPItem.onClick.AddListener(StartSetUseHPItemKey);
        btnUseSPItem.onClick.AddListener(StartSetUseSPItemKey);
        //显示当前键位
        RefreshKeyText();

        btnSound.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            SoundPanel.gameObject.SetActive(true);
            InputPanel.gameObject.SetActive(false);
        });
        btnInput.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            InputPanel.gameObject.SetActive(true);
            SoundPanel.gameObject.SetActive(false);
        });
        //音频
        toggleMusic.isOn = MusicDataMgr.Instance.musicData.isMusicOn;
        toggleSound.isOn = MusicDataMgr.Instance.musicData.isSoundOn;
        sliderMusic.value = MusicDataMgr.Instance.musicData.musicVolume;
        sliderSound.value = MusicDataMgr.Instance.musicData.soundVolume;
        btnExit.onClick.AddListener(() =>
        {
            AudioMgr.Instance.PlayBtnClick();
            MusicDataMgr.Instance.SaveMusicData();
            //SaveKeySettings(); // 退出时保存键位
            UIMgr.Instance.HidePanel<SettingPanel>();
            UIMgr.Instance.ShowPanel<BeginPanel>();
        });
        toggleMusic.onValueChanged.AddListener((isOn) =>
        {
            AudioMgr.Instance.PlayBtnClick();
            //设置音乐开关
            MusicDataMgr.Instance.SetBKMusicIsOn(isOn);
            MusicDataMgr.Instance.musicData.isMusicOn = isOn;
        });
        toggleSound.onValueChanged.AddListener((isOn) =>
        {
            AudioMgr.Instance.PlayBtnClick();
            //设置音效开关
            MusicDataMgr.Instance.SetBKSoundIsOn(isOn);
            MusicDataMgr.Instance.musicData.isSoundOn = isOn;
        });
        sliderMusic.onValueChanged.AddListener((value) =>
        {
            //设置音乐音量
            MusicDataMgr.Instance.SetMusicVolume(value);
            MusicDataMgr.Instance.musicData.musicVolume = value;
        });
        sliderSound.onValueChanged.AddListener((value) =>
        {
            //设置音效音量
            MusicDataMgr.Instance.setSoundVolume(value);
            MusicDataMgr.Instance.musicData.soundVolume = value;
        });
    }
    public override void Update()
    {
        base.Update();

        // 判断是否正在设置任意按键
        bool isAnySetting = isSettingSkillKey || isSettingReloadKey || isSettingInteractKey ||
                            isSettingRollKey || isSettingDialogKey || isSettingPauseKey ||
                            isSettingUseHPItemKey || isSettingUseSPItemKey;

        if (isAnySetting && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    if (isSettingSkillKey) SetSkillKey(key);
                    else if (isSettingReloadKey) SetReloadKey(key);
                    else if (isSettingInteractKey) SetInteractKey(key);
                    else if (isSettingRollKey) SetRollKey(key);
                    else if (isSettingDialogKey) SetDialogKey(key);
                    else if (isSettingPauseKey) SetPauseKey(key);
                    else if (isSettingUseHPItemKey) SetUseHPItemKey(key);
                    else if (isSettingUseSPItemKey) SetUseSPItemKey(key);
                    break;
                }
            }
        }
    }

    #region 键位设置逻辑
    private void StartSetSkillKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingSkillKey = true; skillKey.text = ""; }
    private void StartSetReloadKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingReloadKey = true; ReloadKey.text = ""; }
    private void StartSetInteractKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingInteractKey = true; interactKey.text = ""; }
    private void StartSetRollKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingRollKey = true; rollKey.text = ""; }
    private void StartSetDialogKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingDialogKey = true; dialogKey.text = ""; }
    private void StartSetPauseKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingPauseKey = true; pauseKey.text = ""; }
    private void StartSetUseHPItemKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingUseHPItemKey = true; useHPItemKey.text = ""; }
    private void StartSetUseSPItemKey() { ClearAllFlags(); tip.gameObject.SetActive(true); isSettingUseSPItemKey = true; useSPItemKey.text = ""; }

    private void ClearAllFlags()
    {
        isSettingSkillKey = false;
        isSettingReloadKey = false;
        isSettingInteractKey = false;
        isSettingRollKey = false;
        isSettingDialogKey = false;
        isSettingPauseKey = false;
        isSettingUseHPItemKey = false;
        isSettingUseSPItemKey = false;
    }

    private void SetSkillKey(KeyCode key) { SaveKey(ref CurrentSkillKey, ref gameData.skillKey, key); }
    private void SetReloadKey(KeyCode key) { SaveKey(ref CurrentReloadKey, ref gameData.reloadKey, key); }
    private void SetInteractKey(KeyCode key) { SaveKey(ref CurrentInteractKey, ref gameData.interactKey, key); }
    private void SetRollKey(KeyCode key) { SaveKey(ref CurrentRollKey, ref gameData.rollKey, key); }
    private void SetDialogKey(KeyCode key) { SaveKey(ref CurrentDialogKey, ref gameData.dialogKey, key); }
    private void SetPauseKey(KeyCode key) { SaveKey(ref CurrentPauseKey, ref gameData.pauseKey, key); }
    private void SetUseHPItemKey(KeyCode key) { SaveKey(ref CurrentUseHPItemKey, ref gameData.useHPItemKey, key); }
    private void SetUseSPItemKey(KeyCode key) { SaveKey(ref CurrentUseSPItemKey, ref gameData.useSPItemKey, key); }

    private void SaveKey(ref KeyCode currentKey, ref KeyCode savedKey, KeyCode newKey)
    {
        currentKey = newKey;
        savedKey = newKey;
        GameDataMgr.Instance.SaveGameData();
        ClearAllFlags();
        tip.gameObject.SetActive(false);
        RefreshKeyText();
    }

    private void RefreshKeyText()
    {
        skillKey.text = CurrentSkillKey.ToString();
        ReloadKey.text = CurrentReloadKey.ToString();
        interactKey.text = CurrentInteractKey.ToString();
        rollKey.text = CurrentRollKey.ToString();
        dialogKey.text = CurrentDialogKey.ToString();
        pauseKey.text = CurrentPauseKey.ToString();
        useHPItemKey.text = CurrentUseHPItemKey.ToString();
        useSPItemKey.text = CurrentUseSPItemKey.ToString();
    }
    #endregion
}
