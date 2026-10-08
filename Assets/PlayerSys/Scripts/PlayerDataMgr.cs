using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家数据管理器
/// </summary>
public class PlayerDataMgr
{
    private static PlayerDataMgr _instance = new PlayerDataMgr();
    public static PlayerDataMgr Instance => _instance;
    private Dictionary<string, PlayerConfig> _playerConfigDic = new Dictionary<string, PlayerConfig>();

    // 当前使用的配置和状态
    private PlayerConfig _currentPlayerConfig;
    public  PlayerState playerState;

    private PlayerDataMgr()
    {
        InitAllHeroData();
        playerState = JsonMgr.Instance.LoadData<PlayerState>("PlayerState");
    }

    // 初始化所有角色的配置、状态数据
    private void InitAllHeroData()
    {
        // 加载所有角色配置
        LoadHeroConfig("ToonSoldiers_engineer");
        LoadHeroConfig("ToonSoldiers_gunner");
        LoadHeroConfig("ToonSoldiers_medic");
        LoadHeroConfig("ToonSoldiers_flammer");
        LoadHeroConfig("ToonSoldiers_officer");
    }

    #region 角色配置管理
    // 加载单个角色配置
    private void LoadHeroConfig(string heroName)
    {
        PlayerConfig config = JsonMgr.Instance.LoadData<PlayerConfig>($"PlayerConfig_{heroName}");
        if (config != null)
        {
            _playerConfigDic[heroName] = config;
        }
        else
        {
            Debug.LogError($"未找到角色配置：PlayerConfig_{heroName}");
        }
    }

    // 切换到指定角色的配置(选择角色面板调用)
    public void SwitchHeroConfig(string heroName)
    {
        if (_playerConfigDic.TryGetValue(heroName, out var config))
        {
            _currentPlayerConfig = config;
        }
        else
        {
            Debug.LogError($"切换角色失败：{heroName}");
        }
    }

    // 获取当前角色配置
    public PlayerConfig GetPlayerConfig()
    {
        return _currentPlayerConfig;
    }

    //保存当前角色配置
    public void SavePlayerConfig()
    {
        if (_currentPlayerConfig != null)
        {
            string heroName = _currentPlayerConfig.heroName;
            JsonMgr.Instance.SaveData(_currentPlayerConfig, $"PlayerConfig_{heroName}");
        }
    }
    #endregion

    #region 角色状态管理
    //保存当前角色状态（外部可随时修改状态，然后保存）
    public void SavePlayerState()
    {
        JsonMgr.Instance.SaveData(playerState, "PlayerState");
    }

    #endregion
}