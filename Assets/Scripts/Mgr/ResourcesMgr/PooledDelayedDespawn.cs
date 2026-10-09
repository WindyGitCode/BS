using System.Collections;
using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 允许外部在任意时刻启动一个「延迟归还」倒计时，到期后对象自动回池。
    /// 挂在预制体上即可。
    ///
    /// 【用途】替代 <c>Destroy(gameObject, 2f)</c> 这类延迟销毁写法，例如
    /// <c>MonsterController.Die()</c> 里「死亡动画播完 2 秒后销毁」。
    ///
    /// 【相比 Destroy(obj, delay) 的优势】
    /// <list type="bullet">
    /// <item>可取消：对象若被提前回收，残留的倒计时会被 <see cref="OnSpawn"/>/<see cref="OnDespawn"/> 清掉，
    ///       不会像 <c>Destroy(obj, delay)</c> 那样在对象已被复用时突然把它销毁。</item>
    /// <item>可重置：重复调用 <see cref="DespawnAfter"/> 会重新计时，而不是叠加多个销毁请求。</item>
    /// </list>
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class PooledDelayedDespawn : MonoBehaviour, IPoolable
    {
        private Coroutine _routine;

        /// <summary>复用时先清掉上一次残留的倒计时，避免旧计时器把新生命周期的对象回收掉。</summary>
        public void OnSpawn()
        {
            Cancel();
        }

        public void OnDespawn()
        {
            Cancel();
        }

        /// <summary>
        /// 启动倒计时，到期后自动归还池。重复调用会重置计时（不会叠加）。
        /// </summary>
        /// <param name="delay">延迟秒数。</param>
        /// <param name="useUnscaledTime">是否使用未缩放时间（游戏暂停时仍然到期）。</param>
        public void DespawnAfter(float delay, bool useUnscaledTime = false)
        {
            Cancel();

            if (delay <= 0f)
            {
                PoolService.Instance.Despawn(gameObject);
                return;
            }

            _routine = StartCoroutine(CoDespawn(delay, useUnscaledTime));
        }

        /// <summary>取消尚未到期的归还请求（例如对象被提前回收或复活）。</summary>
        public void CancelDespawn()
        {
            Cancel();
        }

        private void Cancel()
        {
            if (_routine == null) return;
            StopCoroutine(_routine);
            _routine = null;
        }

        private IEnumerator CoDespawn(float delay, bool useUnscaledTime)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(delay);
            else yield return new WaitForSeconds(delay);

            _routine = null;
            PoolService.Instance.Despawn(gameObject);
        }
    }
}
