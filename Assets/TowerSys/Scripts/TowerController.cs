using BS.ResourceManagement;
using System.Collections.Generic;
using UnityEngine;

public class TowerController : MonoBehaviour
{
    [Header("基础配置")]
    public Transform firePoint;
    public string bulletPath_1 = "TowerBullet_1";
    public string bulletPath_2 = "TowerBullet_2";
    public string bulletPath_3 = "TowerBullet_3";

    private TowerData data;
    private Transform targetEnemy;
    private float attackTimer;
    private int towerLevel;
    public AudioClip fireSFX;

    //缓存子弹
    GameObject bullet_1;
    GameObject bullet_2;
    GameObject bullet_3;
    /// <summary>
    /// 根据等级初始化炮台属性
    /// </summary>
    public void InitByLevel(int level,string towerName)
    {
        towerLevel = level;
        data = TowerDataMgr.GetTowerDataByLevel(level,towerName);
        attackTimer = data.attackInterval;

        bullet_1=Resources.Load<GameObject>(bulletPath_1);
        bullet_2 = Resources.Load<GameObject>(bulletPath_2);
        bullet_3 = Resources.Load<GameObject>(bulletPath_3);
    }

    private void Start()
    {
        if (data == null)
        {
            InitByLevel(1, "1");
            Debug.LogError("TowerController Start: 未设置数据");
        }
    }

    private void Update()
    {
        FindNearestEnemy();
        RotateToTarget();
        AttackTimer();
    }

    void FindNearestEnemy()
    {
        targetEnemy = null;
        float minDistance = data.attackRange;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist <= data.attackRange && dist < minDistance)
            {
                minDistance = dist;
                targetEnemy = enemy.transform;
            }
        }
    }

    void RotateToTarget()
    {
        if (targetEnemy == null) return;

        Vector3 dir = targetEnemy.position - transform.position;
        dir.y = 0;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, 8f * Time.deltaTime);
    }

    void AttackTimer()
    {
        if (targetEnemy == null) return;
        if (attackTimer < data.attackInterval)
        {
            attackTimer += Time.deltaTime;
        }
        else
        {
            Fire();
            attackTimer = 0;
        }
    }

    void Fire()
    {
        if (targetEnemy == null || firePoint == null) return;
        GameObject bulletPrefab=null;
        switch (data.towerName)
        {
            case "1":
                bulletPrefab = bullet_1;
                if (bulletPrefab == null)
                {
                    Debug.LogError("未找到子弹预制体 Resources/TowerBullet_1");
                    return;
                }
                break;
            case "2":
                bulletPrefab = bullet_2;
                if (bulletPrefab == null)
                {
                    Debug.LogError("未找到子弹预制体 Resources/TowerBullet_2");
                    return;
                }
                break;
            case "3":
                bulletPrefab = bullet_3;
                if (bulletPrefab == null)
                {
                    Debug.LogError("未找到子弹预制体 Resources/TowerBullet_3");
                    return;
                }
                break;
            default:
                Debug.LogError("未知的塔类型: " + data.towerName);
                return;
        }
        if(bulletPrefab != null)
        {
            if (fireSFX != null)
                AudioMgr.Instance.PlaySFX(fireSFX);
            GameObject bullet = PoolService.Instance.Spawn(bulletPrefab, firePoint.position, firePoint.rotation);
            TowerBullet b = bullet.GetComponent<TowerBullet>();
            b.SetData(targetEnemy, data.damage, data.bulletSpeed);
        }
        else
        {
            Debug.LogError("Fire: 子弹预制体未设置");
        }
    }

    private void OnDrawGizmos()
    {
        float range = data != null ? data.attackRange : 5f;
        Gizmos.color = new Color(1, 0.2f, 0.2f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }
}