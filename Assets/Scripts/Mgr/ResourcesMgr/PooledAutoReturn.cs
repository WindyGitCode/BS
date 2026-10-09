using System.Collections;
using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 让对象在取出后经过固定时长自动归还池。挂在预制体上即可。
    ///
    /// 【为什么不用全局扫描】
    /// 旧的 <c>EffectPool</c> 维护一个 <c>Dictionary&lt;GameObject, float&gt;</c>，
    /// 每 <c>checkInterval</c> 秒把所有租出对象全量遍历一次。本实现改为：
    /// 每个实例在存活期间只跑一个倒计时协程 —— 开销与「当前租出数量」完全无关，
    /// 且不需要服务层持有 Update。
    ///
    /// 这是特效类对象的默认回收方式；需要「由外部决定何时开始倒计时」的场景
    /// （例如怪物死亡后再延迟 2 秒回收）请改用 <see cref="PooledDelayedDespawn"/>。
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class PooledAutoReturn : MonoBehaviour, IPoolable
    {
        [Tooltip("从取出到自动归还的秒数")]
        [SerializeField] private float lifeTime = 1f;

        [Tooltip("勾选后使用未缩放时间：游戏暂停(Time.timeScale=0)时仍会到期归还")]
        [SerializeField] private bool useUnscaledTime = false;

        private Coroutine _routine;
        private object _wait;                 // WaitForSeconds 或 WaitForSecondsRealtime
        private float _cachedLifeTime;
        private bool _cachedUnscaledTime;
        private bool _waitBuilt;

        /// <summary>运行时修改存活时长（下一次 <see cref="OnSpawn"/> 生效）。</summary>
        public void SetLifeTime(float seconds)
        {
            lifeTime = Mathf.Max(0.01f, seconds);
            _waitBuilt = false;
        }

        public void OnSpawn()
        {
            Cancel();
            EnsureWait();
            _routine = StartCoroutine(CoAutoReturn());
        }

        public void OnDespawn()
        {
            Cancel();
        }

        private void Cancel()
        {
            if (_routine == null) return;
            StopCoroutine(_routine);
            _routine = null;
        }

        /// <summary>
        /// 构建并缓存 yield 指令，避免每次取出都分配一个新的 WaitForSeconds。
        /// 仅在参数变化时重建。
        /// </summary>
        private void EnsureWait()
        {
            float t = Mathf.Max(0.01f, lifeTime);
            if (_waitBuilt && _cachedLifeTime == t && _cachedUnscaledTime == useUnscaledTime) return;

            _cachedLifeTime = t;
            _cachedUnscaledTime = useUnscaledTime;
            _wait = useUnscaledTime ? (object)new WaitForSecondsRealtime(t) : new WaitForSeconds(t);
            _waitBuilt = true;
        }

        private IEnumerator CoAutoReturn()
        {
            yield return _wait;

            // 先清空句柄：本协程即将结束，无需再被 StopCoroutine
            _routine = null;

            PoolService.Instance.Despawn(gameObject);
        }
    }
}
