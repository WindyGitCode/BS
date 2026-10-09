using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 对象池服务接口。
    ///
    /// 【为什么要依赖接口而不是具体类】
    /// 业务代码只应依赖本接口，这样后续把池的实现替换掉（例如改为按场景分池、
    /// 或接入 Addressables 的实例池）时，所有调用点都不需要改动。
    ///
    /// 【使用约定】
    /// <list type="bullet">
    /// <item>传入的 <c>prefab</c> 必须是<b>预制体资源本身</b>，不能是场景里已存在的实例。</item>
    /// <item>通过 <see cref="Spawn(GameObject)"/> 得到的对象，回收时<b>必须</b>调用 <see cref="Despawn"/>，
    ///       严禁直接 <c>Destroy</c>（池会侦测到并告警，且该槽位会被除名）。</item>
    /// <item>同一个 <c>prefab</c> 永远共用一个池，与调用方无关。</item>
    /// </list>
    /// </summary>
    public interface IPoolService
    {
        // ---------------------------------------------------------------- 取出

        /// <summary>取出实例，<b>不改动</b>其位置与旋转，也不改变父子关系（UI 列表项常用）。</summary>
        GameObject Spawn(GameObject prefab);

        /// <summary>取出实例并设置世界位置/旋转，根节点下（子弹、特效、怪物常用）。</summary>
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation);

        /// <summary>取出实例、挂到 <paramref name="parent"/> 下并设置世界位置/旋转。</summary>
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent);

        /// <summary>取出实例并挂到 <paramref name="parent"/> 下，<b>保留</b>其局部变换（UI 列表项常用）。</summary>
        GameObject Spawn(GameObject prefab, Transform parent);

        /// <summary>取出实例并直接返回指定组件，省掉调用侧的 GetComponent。</summary>
        T Spawn<T>(GameObject prefab) where T : Component;

        /// <summary>取出实例、设置世界位置/旋转，并直接返回指定组件。</summary>
        T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component;

        /// <summary>取出实例、挂到父节点并设置世界位置/旋转，直接返回指定组件。</summary>
        T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component;

        // ---------------------------------------------------------------- 归还

        /// <summary>
        /// 归还实例。会依次触发其 <see cref="IPoolable.OnDespawn"/>、隐藏并放回池中。
        /// 重复归还会被检测并告警（不会重复入池）。
        /// </summary>
        void Despawn(GameObject instance);

        /// <summary>预创建若干个实例放入池中，避免首次使用时才 Instantiate（应放在加载遮罩期间调用）。</summary>
        void Prewarm(GameObject prefab, int count);

        // ---------------------------------------------------------------- 批量与清理

        /// <summary>强制归还指定预制体当前所有租出实例（关卡结束时兜底用）。</summary>
        void DespawnAllRented(GameObject prefab);

        /// <summary>销毁指定预制体的全部空闲实例，租出中的不受影响。</summary>
        void ClearPool(GameObject prefab);

        /// <summary>
        /// 清空全部池。场景切换 / 关卡卸载时调用。
        /// </summary>
        /// <param name="destroyRented">
        /// 是否连同租出中的实例一起销毁。场景卸载时通常传 <c>true</c>；
        /// 否则租出中的实例会变成「野对象」——已不属于任何池，<see cref="Despawn"/> 会失败。
        /// </param>
        void ClearAll(bool destroyRented = false);

        // ---------------------------------------------------------------- 观测

        /// <summary>获取指定预制体的池统计。若该预制体尚未建池，返回全 0。</summary>
        PoolStats GetStats(GameObject prefab);

        /// <summary>把所有池的统计打到 Console，用于验证优化效果与排查泄漏。</summary>
        void LogStats();
    }
}
