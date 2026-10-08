using UnityEngine;

public class TowerBullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    private float speed;
    private float destroyTimer;
    Vector3 dir;

    public void SetData(Transform _target, int _damage, float _speed)
    {
        target = _target;
        damage = _damage;
        speed = _speed;
        destroyTimer = 2f;

        dir = (target.position - transform.position).normalized;
        dir.y = 0;
    }

    private void Update()
    {
        destroyTimer -= Time.deltaTime;
        if (destroyTimer <= 0)
        {
            Destroy(gameObject);
            return;
        }
        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            if (gameObject.name == "TowerBullet_1(Clone)")
            {
                col.GetComponentInParent<MonsterController>()?.TakeDamage(damage);
            }
            else if (gameObject.name == "TowerBullet_2(Clone)")
            {
                col.GetComponentInParent<MonsterController>()?.TakeDamage(damage);
                col.GetComponentInParent<MonsterController>()?.loseSpeed();
            }
            else if (gameObject.name == "TowerBullet_3(Clone)")
            {
                ExplodeAreaDamage();
            }
            else
            {
                Debug.LogError("TowerBullet OnTriggerEnter: 未知子弹类型");
            }
        }
        else
        {
            return;
        }
        Destroy(gameObject);
    }

    public GameObject explosionEffect;   //爆炸特效
    public float explosionRadius = 2.5f;  // 爆炸半径
    public bool isExplosiveBullet = false; // 3号子弹勾上
    void ExplodeAreaDamage()
    {
        // 1. 播放爆炸特效
        if (explosionEffect != null)
        {
            GameObject eff = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(eff, 2f); // 2秒后销毁特效
        }

        // 2. 范围检测所有敌人
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var col in hitEnemies)
        {
            if (col.CompareTag("Enemy"))
            {
                MonsterController monster = col.GetComponentInParent<MonsterController>();
                if (monster != null)
                {
                    monster.TakeDamage(damage); // 范围内所有敌人都受伤
                }
            }
        }
    }

    // 爆炸范围辅助线（调试用）
    private void OnDrawGizmosSelected()
    {
        if (isExplosiveBullet)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}