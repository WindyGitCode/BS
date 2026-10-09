using System.Collections.Generic;
using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 单个预制体对应的池。由 <see cref="PoolService"/> 持有，不对外暴露。
    ///
    /// 数据结构选择：
    /// <list type="bullet">
    /// <item><c>Stack</c> 存空闲实例 —— LIFO 让最近归还的对象优先被复用，
    ///       缓存局部性更好，也能让多余的实例自然沉底、被容量上限淘汰。</item>
    /// <item><c>HashSet</c> 存租出实例 —— 归还时 O(1) 校验「是否真属于本池、是否真的租出中」，
    ///       从而能检测重复归还与跨池归还。</item>
    /// </list>
    /// </summary>
    internal sealed class Pool
    {
        private readonly PoolService _owner;
        private readonly GameObject _prefab;
        private readonly int _maxIdleSize;
        private readonly Stack<PooledInstance> _idle;
        private readonly HashSet<PooledInstance> _rented;

        /// <summary>累计创建的实例数（Instantiate 次数）。</summary>
        public int CreatedCount { get; private set; }

        /// <summary>累计复用命中次数。</summary>
        public int ReusedCount { get; private set; }

        public int IdleCount => _idle.Count;

        public int RentedCount => _rented.Count;

        public Pool(PoolService owner, GameObject prefab, int prewarmCount, int maxIdleSize)
        {
            _owner = owner;
            _prefab = prefab;
            _maxIdleSize = Mathf.Max(1, maxIdleSize);
            _idle = new Stack<PooledInstance>(Mathf.Max(4, prewarmCount));
            _rented = new HashSet<PooledInstance>();

            for (int i = 0; i < prewarmCount; i++)
            {
                _idle.Push(CreateInstance());
            }
        }

        // ------------------------------------------------------------------ 创建

        private PooledInstance CreateInstance()
        {
            // 先挂到常驻的 [Pool] 根节点下，保证过场景时不被销毁
            GameObject go = Object.Instantiate(_prefab, _owner.IdleRoot);

            // 【刻意不改名】Instantiate 产生的名字是 "Xxx(Clone)"，这里保持不变。
            //
            // 原因：本项目现有代码存在依赖克隆名的判断，例如
            //   TowerBullet.cs:37/41/46  if (gameObject.name == "TowerBullet_1(Clone)")
            // 用来区分子弹类型。若在此处把名字改成 prefab.name（去掉 "(Clone)"），
            // 那些比较会全部失效，子弹将打不出任何伤害。
            //
            // 池化迁移期不做这种「静默改变行为」的改动。等依赖名字的逻辑被
            // 改为序列化字段/枚举之后，再启用改名以美化 Hierarchy 与 Profiler 显示。
            go.SetActive(false);
            CreatedCount++;

            var inst = new PooledInstance(go) { Owner = this };
            _owner.Register(inst);

            // 挂侦测器：捕获「被外部 Destroy」的误用
            PooledInstanceWatcher watcher = go.GetComponent<PooledInstanceWatcher>();
            if (watcher == null) watcher = go.AddComponent<PooledInstanceWatcher>();
            watcher.Bind(_owner);

            return inst;
        }

        // ------------------------------------------------------------------ 取出

        /// <param name="parent">目标父节点，可为 null（表示脱离 [Pool] 根节点）。</param>
        /// <param name="setWorldPose">是否用给定的世界位置/旋转覆盖当前变换。</param>
        public PooledInstance Rent(Transform parent, Vector3 position, Quaternion rotation, bool setWorldPose)
        {
            PooledInstance inst;
            if (_idle.Count > 0)
            {
                inst = _idle.Pop();
                ReusedCount++;
            }
            else
            {
                inst = CreateInstance();
            }

            GameObject go = inst.Go;
            Transform t = go.transform;

            // 始终重设父子关系：既挂到目标父节点，也从 [Pool] 根节点下脱离。
            // worldPositionStays = false —— 保留局部变换，避免 UI 元素被重新定位。
            t.SetParent(parent, false);

            if (setWorldPose)
            {
                t.SetPositionAndRotation(position, rotation);
            }

            // 必须先激活、再回调 OnSpawn：
            // OnSpawn 中常需要 StartCoroutine，而未激活的对象无法启动协程。
            go.SetActive(true);

            inst.IsRented = true;
            inst.RentStartUnscaledTime = Time.unscaledTime;
            _rented.Add(inst);

            inst.InvokeOnSpawn();
            return inst;
        }

        // ------------------------------------------------------------------ 归还

        /// <returns>本次归还后实例是否被保留在池中（false 表示已达容量上限被销毁）。</returns>
        public bool Return(PooledInstance inst)
        {
            // 不在租出集合中 —— 说明是重复归还，交由调用方决定是否告警
            if (!_rented.Remove(inst)) return false;

            inst.IsRented = false;

            // 此时对象仍处于激活状态，OnDespawn 可以安全地停协程 / DOKill
            inst.InvokeOnDespawn();

            GameObject go = inst.Go;
            if (go == null)
            {
                // 已被外部销毁（正常路径下侦测器会先处理，这里是兜底）
                _owner.Unregister(inst);
                return false;
            }

            // 超出保留上限：直接销毁，防止空闲实例无界增长
            if (_idle.Count >= _maxIdleSize)
            {
                // 先从索引除名，避免侦测器把它当成「误用 Destroy」而误报
                _owner.Unregister(inst);
                Object.Destroy(go);
                return false;
            }

            go.transform.SetParent(_owner.IdleRoot, false);
            go.SetActive(false);
            _idle.Push(inst);
            return true;
        }

        /// <summary>补充创建若干空闲实例。可在任意时刻调用（首次 Spawn 之前调用效果最好）。</summary>
        public void PrewarmAdditional(int count)
        {
            for (int i = 0; i < count; i++)
            {
                _idle.Push(CreateInstance());
            }
        }

        // ------------------------------------------------------------------ 批量

        /// <summary>归还全部租出实例（先快照再遍历，避免遍历中被修改）。</summary>
        public void ReturnAllRented()
        {
            if (_rented.Count == 0) return;

            var snapshot = new List<PooledInstance>(_rented);
            for (int i = 0; i < snapshot.Count; i++)
            {
                Return(snapshot[i]);
            }
        }

        /// <summary>销毁全部空闲实例。租出中的不受影响。</summary>
        public void DestroyAllIdle()
        {
            while (_idle.Count > 0)
            {
                PooledInstance inst = _idle.Pop();
                _owner.Unregister(inst);
                if (inst.Go != null) Object.Destroy(inst.Go);
            }
        }

        /// <summary>
        /// 销毁池中全部实例（含租出中的）。
        /// 【危险】租出中的实例会变成野对象 —— 调用方必须先确保没人还会用到它们。
        /// 典型用途：场景卸载。
        /// </summary>
        public void DestroyAll()
        {
            while (_idle.Count > 0)
            {
                PooledInstance inst = _idle.Pop();
                _owner.Unregister(inst);
                if (inst.Go != null) Object.Destroy(inst.Go);
            }

            foreach (PooledInstance inst in _rented)
            {
                _owner.Unregister(inst);
                if (inst.Go != null) Object.Destroy(inst.Go);
            }
            _rented.Clear();
        }

        /// <summary>
        /// 从租出集合中除名（不触发 OnDespawn、不回收）。
        /// 供 <see cref="PoolService.NotifyInstanceDestroyedExternally"/> 处理脏条目时使用。
        /// </summary>
        public void ForgetRented(PooledInstance inst)
        {
            _rented.Remove(inst);
        }

        public PoolStats GetStats()
        {
            return new PoolStats(CreatedCount, ReusedCount, _idle.Count, _rented.Count);
        }

        public string PrefabName => _prefab != null ? _prefab.name : "(已销毁的预制体)";
    }
}
