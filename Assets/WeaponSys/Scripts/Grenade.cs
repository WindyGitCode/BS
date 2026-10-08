using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float delay = 1f; // 1秒后爆炸
    public float explosionRadius;
    public int damage;
    public GameObject explosionEffect;
    public AudioClip BoomAudioClip;

    private Rigidbody rb;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // 投掷
    public void Throw(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
        Invoke(nameof(Explode), delay);
    }

    // 爆炸
    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;
        Debug.Log("手雷爆炸！");
        // 范围检测
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Enemy")&&col.gameObject.name == "body")
            {
                Debug.Log("炸到敌人！");
                col.GetComponentInParent<MonsterController>()?.TakeDamage(damage);
                //手雷炸死敌人，发事件
                if (damage >= col.GetComponentInParent<MonsterController>()?.currentHP)
                {
                    EventMgr.Instance.Trigger(EventConst.GrenadeKillMonster);
                }
                    
            }
        }

        // 特效
        if (explosionEffect != null)
        {
            GameObject eff = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            // 自动销毁（2秒后删除）
            Destroy(eff, 2f);
        }
        //音效
        if (BoomAudioClip != null)
        {
            // 创建临时音效物体播放
            GameObject soundObj = new GameObject("GrenadeSound");
            soundObj.transform.position = transform.position;
            AudioSource audioSource = soundObj.AddComponent<AudioSource>();
            audioSource.PlayOneShot(BoomAudioClip);
            Destroy(soundObj, 2f);
        }
        Destroy(gameObject);
    }
}