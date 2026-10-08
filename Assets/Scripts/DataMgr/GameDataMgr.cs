using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public static class InputConst
{
    // 移动
    public const string MoveHorizontal = "MoveHorizontal";
    public const string MoveVertical = "MoveVertical";

    // 视角
    public const string LookX = "LookX";
    public const string LookY = "LookY";

    // 动作
    public const string Fire = "Fire";
    public const string Reload = "Reload";
    public const string Jump = "Jump";
    public const string WeaponSwap = "WeaponSwap";
}
public class GameDataMgr
{
    private static GameDataMgr _instance;
    public static GameDataMgr Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameDataMgr();
            _instance.Init();
            return _instance;
        }
    }

    private GameData _gameData; // 游戏数据实体

    private GameDataMgr()
    {
        
    }
    public void Init()
    {
        if (_gameData == null)
        {
            _gameData = JsonMgr.Instance.LoadData<GameData>("GameData");
        }
    }

    // 获取数据
    public GameData GetGameData()
    {
        return _gameData;
    }

    // 保存数据
    public void SaveGameData()
    {
        JsonMgr.Instance.SaveData(_gameData, "gameData");
    }

    // ==================== 金币系统 ====================

   // 直接设置金币数量
    public void SetMoney(int value)
    {
        _gameData.gold = Mathf.Max(0, value); // 不能为负
        SaveGameData();
        EventMgr.Instance.Trigger(EventConst.GameDataChange); // 触发金币更新事件
    }

    // 增加金币
    public void AddMoney(int value)
    {
        if (value < 0) return;
        _gameData.gold += value;
        SaveGameData();
        EventMgr.Instance.Trigger(EventConst.GameDataChange); // 触发金币更新事件
    }

    // 减少金币，返回是否成功
    public bool SpendMoney(int value)
    {
        if (value < 0) return false;
        if (_gameData.gold < value) return false;

        _gameData.gold -= value;
        SaveGameData();
        EventMgr.Instance.Trigger(EventConst.GameDataChange); // 触发金币更新事件
        return true;
    }

    //加手雷
    public void AddGrenade(int value)
    {
        if (value < 0) return;
        _gameData.Grenade += value;
        SaveGameData();
        EventMgr.Instance.Trigger(EventConst.GameDataChange);
    }
    //加步枪子弹
    public void AddMainGunBullet(int value)
    {
        if (value < 0) return;
        _gameData.MainGunAmmo += value;
        SaveGameData();
        EventMgr.Instance.Trigger(EventConst.GameDataChange);
    }
    public void SetAmmoNum(int MainGunAmmo,int HandGunAmmo,int HeavyGunAmmo,int ShotGunAmmo,int SniperAmmo,int Grenade)
    { 
        _gameData.MainGunAmmo = MainGunAmmo;
        _gameData.HandGunAmmo = HandGunAmmo;
        _gameData.HeavyGunAmmo = HeavyGunAmmo;
        _gameData.ShotGunAmmo = ShotGunAmmo;
        _gameData.SniperAmmo = SniperAmmo;
        _gameData.Grenade = Grenade;
        SaveGameData();
    }
    public void SetItemNum(int gold, int HP, int SP)
    {
        _gameData.gold = gold;
        _gameData.MedKit = HP;
        _gameData.StaminaPotion = SP;
        SaveGameData();
    }
    public void SetFixTowerNum(int value)
    {
        _gameData.TowerRepairKit = value;
        SaveGameData();
    }
}
