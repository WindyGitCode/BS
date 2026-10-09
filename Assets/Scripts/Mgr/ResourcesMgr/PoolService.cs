using System.Collections.Generic;
using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 对象池服务。
    ///
    /// 【与旧 EffectPool 的关键差异】
    /// <list type="bullet">
    /// <item><b>池键</b>：按「预制体资源引用」分池，而不是按 <c>prefab.name</c>。
    ///       旧实现用名字做键，同名不同资源会串池（把 A 的实例还给 B）。</item>
    /// <item><b>自动回收</b>：没有全局 Update 扫描。用 <see cref="PooledAutoReturn"/> 让每个实例
    ///       自己跑一个倒计时协程，开销与租出数量无关；旧实现每秒全量扫描一次租出字典。</item>
    /// <item><b>复用初始化</b>：通过 <see cref="IPoolable"/> 契约强制重置状态，
    ///       避免「Start() 在复用时不执行」导致的状态残留。</item>
    /// <item><b>可观测</b>：提供 <see cref="PoolStats"/>，能直接看出复用是否真的生效。</item>
    /// </list>
    ///
    /// 【常驻性】池根节点 [Pool] 带 DontDestroyOnLoad，因此池中对象会跨场景存活。
    /// 切场景/关卡结束时请调用 <see cref="ClearAll"/> 或 <see cref="ClearPool"/> 主动释放。
    /// </summary>
    public sealed class PoolService : IPoolService
    {
        private const int DefaultMaxIdleSize = 64;

        private static PoolService _instance;

        private static PoolService Concrete => _instance ??= new PoolService();

        /// <summary>
        /// 全局访问点。
        ///
        /// 【设计说明】这是过渡期的服务定位器，与本项目现有的
        /// <c>UIMgr.Instance</c> / <c>GameDataMgr.Instance</c> 风格保持一致。
        /// 后续引入 GameBootstrap 之后，应改为由启动器注入 <see cref="IPoolService"/>，
        /// 届时所有调用点无需改动（因为它们依赖的是接口）。
        /// </summary>
        public static IPoolService Instance => Concrete;

        /// <summary>
        /// 调整默认参数，必须在首次 <c>Spawn</c> 之前调用。
        /// </summary>
        /// <param name="prewarmCount">每个新建池的默认预热数量。</param>
        /// <param name="maxIdleSize">每个池保留空闲实例的上限，超出部分归还时直接销毁。</param>
        public static void Configure(int prewarmCount = 0, int maxIdleSize = DefaultMaxIdleSize)
        {
            Concrete.ConfigureDefaults(prewarmCount, maxIdleSize);
        }

        private readonly Dictionary<GameObject, Pool> _pools = new Dictionary<GameObject, Pool>();

        /// <summary>实例 → 包装对象的反查表，使 Despawn 为 O(1)，无需给实例挂组件。</summary>
        private readonly Dictionary<GameObject, PooledInstance> _index = new Dictionary<GameObject, PooledInstance>();

        private Transform _root;
        private int _defaultPrewarmCount;
        private int _defaultMaxIdleSize = DefaultMaxIdleSize;

        private PoolService() { }

        // ------------------------------------------------------------------ 内部协作

        /// <summary>空闲实例的挂载点。常驻，跨场景不销毁。</summary>
        internal Transform IdleRoot
        {
            get
            {
                if (_root == null)
                {
                    var rootGo = new GameObject("[Pool]");
                    Object.DontDestroyOnLoad(rootGo);
                    _root = rootGo.transform;
                }
                return _root;
            }
        }

        internal void Register(PooledInstance inst)
        {
            _index[inst.Go] = inst;
        }

        internal void Unregister(PooledInstance inst)
        {
            if (inst == null) return;
            // 注意：这里的 GameObject 可能已被 Destroy。
            // 字典按引用比较，不访问 Unity 属性，因此移除是安全的。
            _index.Remove(inst.Go);
        }

        /// <summary>
        /// 由 <see cref="PooledInstanceWatcher"/> 在实例被销毁时回调。
        /// 若索引表中仍有该实例，说明是被「外部 Destroy」而非正常归还，需要除名并告警。
        /// </summary>
        internal void NotifyInstanceDestroyedExternally(GameObject instance)
        {
            if (!_index.TryGetValue(instance, out PooledInstance inst)) return; // 池主动销毁的正常路径

            bool wasRented = inst.IsRented;
            inst.IsRented = false;
            _index.Remove(instance);
            inst.Owner.ForgetRented(inst);

            if (wasRented && inst.Owner != null)
            {
                Debug.LogWarning(
                    $"[PoolService] 池化对象 '{inst.Owner.PrefabName}' 在归还前被外部 Destroy。" +
                    "该槽位已从池中除名（无法再被复用）。请改用 PoolService.Instance.Despawn(...) 归还。");
            }
        }

        private void ConfigureDefaults(int prewarmCount, int maxIdleSize)
        {
            _defaultPrewarmCount = Mathf.Max(0, prewarmCount);
            _defaultMaxIdleSize = Mathf.Max(1, maxIdleSize);
        }

        private Pool GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out Pool pool)) return pool;

            pool = new Pool(this, prefab, _defaultPrewarmCount, _defaultMaxIdleSize);
            _pools.Add(prefab, pool);
            return pool;
        }

        private PooledInstance RentInternal(GameObject prefab, Transform parent,
                                           Vector3 position, Quaternion rotation, bool setWorldPose)
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolService] Spawn 失败：prefab 为 null。");
                return null;
            }

            // 若传入的是场景中的实例而非预制体资源，克隆出来的会是「当前状态」的副本，
            // 往往不是预期行为 —— 给出提示但不阻断。
            if (prefab.scene.IsValid())
            {
                Debug.LogWarning(
                    $"[PoolService] 传入的 '{prefab.name}' 是场景中的实例，不是预制体资源。" +
                    "池会克隆它当前的状态；通常应传入 prefab 资源本身。");
            }

            return GetOrCreatePool(prefab).Rent(parent, position, rotation, setWorldPose);
        }

        // ------------------------------------------------------------------ 取出

        public GameObject Spawn(GameObject prefab)
        {
            return RentInternal(prefab, null, Vector3.zero, Quaternion.identity, false)?.Go;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return RentInternal(prefab, null, position, rotation, true)?.Go;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            return RentInternal(prefab, parent, position, rotation, true)?.Go;
        }

        public GameObject Spawn(GameObject prefab, Transform parent)
        {
            return RentInternal(prefab, parent, Vector3.zero, Quaternion.identity, false)?.Go;
        }

        public T Spawn<T>(GameObject prefab) where T : Component
        {
            GameObject go = RentInternal(prefab, null, Vector3.zero, Quaternion.identity, false)?.Go;
            return go == null ? null : go.GetComponent<T>();
        }

        public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            GameObject go = RentInternal(prefab, null, position, rotation, true)?.Go;
            return go == null ? null : go.GetComponent<T>();
        }

        public T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            GameObject go = RentInternal(prefab, parent, position, rotation, true)?.Go;
            return go == null ? null : go.GetComponent<T>();
        }

        // ------------------------------------------------------------------ 归还

        public void Despawn(GameObject instance)
        {
            // Unity 的 == 重载：已销毁的对象在此处即被拦下。
            // 此时不能访问 instance.name（会抛 MissingReferenceException）。
            if (instance == null) return;

            if (!_index.TryGetValue(instance, out PooledInstance inst))
            {
                Debug.LogWarning(
                    $"[PoolService] Despawn 失败：'{instance.name}' 不属于任何池。" +
                    "该对象不是通过 PoolService.Spawn 创建的，或已经被归还过。");
                return;
            }

            if (!inst.IsRented)
            {
                Debug.LogWarning($"[PoolService] Despawn 失败：'{instance.name}' 已经归还过了（重复归还）。");
                return;
            }

            inst.Owner.Return(inst);
        }

        public void Prewarm(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolService] Prewarm 失败：prefab 为 null。");
                return;
            }
            if (count <= 0) return;

            GetOrCreatePool(prefab).PrewarmAdditional(count);
        }

        // ------------------------------------------------------------------ 批量与清理

        public void DespawnAllRented(GameObject prefab)
        {
            if (prefab == null) return;
            if (_pools.TryGetValue(prefab, out Pool pool)) pool.ReturnAllRented();
        }

        public void ClearPool(GameObject prefab)
        {
            if (prefab == null) return;
            if (_pools.TryGetValue(prefab, out Pool pool)) pool.DestroyAllIdle();
        }

        public void ClearAll(bool destroyRented = false)
        {
            foreach (KeyValuePair<GameObject, Pool> pair in _pools)
            {
                if (destroyRented) pair.Value.DestroyAll();
                else pair.Value.DestroyAllIdle();
            }

            // 只有连租出中的一起销毁时，池本身才可以丢弃。
            // 否则必须保留，因为那些实例还要归还到各自的池里。
            if (destroyRented) _pools.Clear();
        }

        // ------------------------------------------------------------------ 观测

        public PoolStats GetStats(GameObject prefab)
        {
            if (prefab != null && _pools.TryGetValue(prefab, out Pool pool)) return pool.GetStats();
            return default;
        }

        public void LogStats()
        {
            if (_pools.Count == 0)
            {
                Debug.Log("[PoolService] 当前没有任何池。");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.Append("[PoolService] 池统计（池数量=").Append(_pools.Count)
              .Append("，跟踪实例=").Append(_index.Count).AppendLine("）");

            foreach (KeyValuePair<GameObject, Pool> pair in _pools)
            {
                sb.Append("  ").Append(pair.Value.PrefabName).Append("  →  ")
                  .AppendLine(pair.Value.GetStats().ToString());
            }

            Debug.Log(sb.ToString());
        }
    }
}
