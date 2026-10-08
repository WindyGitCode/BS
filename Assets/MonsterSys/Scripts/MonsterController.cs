using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour
{
    public MonsterConfig monsterData;// 怪物数据（通过编辑器赋值，包含ID，出生时会根据ID从数据管理器获取完整数据）
    
    public bool isDead;
    public bool isAttacking;
    public bool isDamaged;
    public bool isWalking;

    private int isFinishAnim;
    public Animator animator;
    private NavMeshAgent navAgent;//导航代理
    public int currentHP;// 当前血量
    private Transform targetTower; // 主塔目标
    private Transform targetPlayer; // 玩家目标
    private float attackTimer; // 攻击间隔计时器
    private AudioClip deadAudio;//死亡音效

    void Start()
    {
        monsterData = MonsterDataMgr.Instance.GetMonsterDataByID(1);
        deadAudio = Resources.Load<AudioClip>("Audio/MonsterDead");
        if (deadAudio == null)
        {
            Debug.Log("未加载到deadAudio");
        }
        InitMonster();
    }

    void Update()
    {
        if (targetPlayer == null)
            targetPlayer = PlayerController.Instance.transform;
        //生存模式中，将玩家视为主塔
        if (targetTower == null)
        {
            if (ChoosePaternPanel.GameMode == E_LevelType.Survival)
            {
                targetTower = PlayerController.Instance.transform;
            }
            else
            {
                targetTower = MainTower.Instance.transform;
            }  
        }
        if (isDead) return;
        attackTimer += Time.deltaTime;
        FindTarget();
        UpdateAnimation();
    }
    // 初始化敌人
    void InitMonster()
    {
        // 获取动画组件
        animator = GetComponentInChildren<Animator>();
        // 获取寻路组件
        navAgent = GetComponent<NavMeshAgent>();
        currentHP = monsterData.maxHP;
        navAgent.speed = monsterData.moveSpeed;
        isDead = false;
        isAttacking = false;
        isDamaged = false;
        isWalking = false;
        if(navAgent == null)
        {
            Debug.LogError("敌人缺少NavMeshAgent组件！");
            return;
        }
        else
        {
            //Debug.Log("敌人NavMeshAgent组件加载成功");
        }
    }

    #region 寻找目标（优先玩家，其次主塔）
    void FindTarget()
    {
        if (ChoosePaternPanel.GameMode==E_LevelType.Survival)//生存模式逻辑
        {
            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= monsterData.atkRange)
            {
                AttackPlayer();
                //Debug.Log("敌人正在攻击玩家");
            }
            else if (targetTower != null && Vector3.Distance(transform.position, targetTower.position) <= monsterData.atkRange)
            {
                AttackTower();
                //Debug.Log("敌人正在攻击主塔");
            }
            // 都不在攻击范围 → 移动
            else
            {
                //Debug.Log("敌人正在移动");
                MoveToTower();
            }
        }
        else//其他模式逻辑
        {
            // 优先攻击玩家
            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.position) <= monsterData.atkRange)
            {
                AttackPlayer();
                //Debug.Log("敌人正在攻击玩家");
            }
            // 其次攻击塔
            else if (targetTower != null && Vector3.Distance(transform.position, targetTower.position) <= monsterData.atkRange + 2)
            {
                AttackTower();
                //Debug.Log("敌人正在攻击主塔");
            }
            // 都不在攻击范围 → 移动
            else
            {
                //Debug.Log("敌人正在移动");
                MoveToTower();
            }
        }
        
    }
    #endregion

    // 移动到主塔（出生后调用）
    public void MoveToTower()
    {
        isAttacking = false;
        isWalking = true;
        if (isFinishAnim == 1)
        {
            if (targetTower == null || isDead || isAttacking) return;
            navAgent.isStopped = false;
            navAgent.SetDestination(targetTower.position);
            //Debug.Log("敌人正在向主塔移动");
        }
    }

    // 攻击玩家
    void AttackPlayer()
    {
        if (attackTimer < monsterData.atkCD) return;
        isAttacking = true;
        navAgent.isStopped = true;
        attackTimer = 0;
        // 转向目标
        LookAtTarget(targetPlayer);
        // 玩家受伤逻辑
        float distance = Vector3.Distance(transform.position, targetPlayer.position);
        if (distance <= monsterData.atkRange)
        {
            Debug.Log("怪物攻击命中玩家！伤害：" + monsterData.atk);
            // 玩家受伤事件，把伤害值传过去
            EventMgr.Instance.Trigger<MonsterConfig>(EventConst.PlayerTakeDamage, monsterData);
        }
    }

    // 攻击主塔
    void AttackTower()
    {
        if (attackTimer < 1.0f) return;
        isWalking = false;
        isAttacking = true;
        navAgent.isStopped = true;
        attackTimer = 0;
        LookAtTarget(targetTower);
        // 主塔受伤逻辑
        MainTower.Instance.TakeDamage(monsterData.atk);
    }

    // 转向目标
    void LookAtTarget(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0;
        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, monsterData.rotateSpeed * Time.deltaTime);
    }

    // 更新动画
    void UpdateAnimation()
    {
        animator.SetBool("walk", isWalking);
        animator.SetBool("attack", isAttacking);
        animator.SetBool("dead", isDead);
        animator.SetBool("damage", isDamaged);
    }

    // 受伤函数（你的武器攻击会调用这个）
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP = Mathf.Max(0, currentHP - damage);
        //Debug.Log("敌人受伤，剩余血量：" + currentHP);
        if (currentHP <= 0)
        {
            Die();
        }
        if (damage > 90)
        {
            EventMgr.Instance.Trigger(EventConst.HeadShotKill);
        }
    }

    // 死亡
    void Die()
    {
        isDead = true;
        navAgent.isStopped = true;
        animator.SetBool("dead", true);
        //音效
        AudioMgr.Instance.PlaySFX(deadAudio);
        // 死亡后2秒销毁敌人
        Destroy(gameObject, 2f);
        EventMgr.Instance.Trigger(EventConst.MonsterKilled);
        //生存模式击杀奖励
        if (ChoosePaternPanel.GameMode == E_LevelType.Survival)
        {
            GameDataMgr.Instance.AddMoney(10);
            EventMgr.Instance.Trigger(EventConst.GameDataChange);
        }
    }
    public void loseSpeed()
    {
          navAgent.speed = monsterData.moveSpeed * 0.5f;
    }
    public void FinishSpawnAnim()
    {
        isFinishAnim = 1;
    }
}