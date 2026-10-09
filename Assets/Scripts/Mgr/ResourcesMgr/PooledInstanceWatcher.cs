using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 【内部使用，请勿手动添加】侦测池化对象被外部 <c>Destroy</c> 的情况。
    ///
    /// 由 <see cref="Pool"/> 在创建实例时自动挂载。作用是解决一个静默泄漏：
    /// 若调用方对池化对象调用了 <c>Destroy(gameObject)</c>（而不是归还），
    /// 池的租出集合与索引表里会永久残留一条指向已销毁对象的脏条目 ——
    /// 这既会让统计数字失真，也会让对象无法被复用。
    ///
    /// 本组件在 <c>OnDestroy</c> 时通知服务清理，并打印一条告警，
    /// 让「误用 Destroy」这类问题在开发期立刻暴露，而不是变成线上难以定位的泄漏。
    ///
    /// 注意：池自身销毁实例前会先从索引表中除名，因此正常回收不会触发告警。
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public sealed class PooledInstanceWatcher : MonoBehaviour
    {
        private PoolService _service;

        internal void Bind(PoolService service)
        {
            _service = service;
        }

        private void OnDestroy()
        {
            // 服务可能在编辑器退出 / 域重载时先一步被回收
            if (_service == null) return;
            _service.NotifyInstanceDestroyedExternally(gameObject);
        }
    }
}
