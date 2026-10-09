namespace BS.ResourceManagement
{
    /// <summary>
    /// 单个对象池的运行时统计快照。
    /// 用途：验证池化是否真正生效（复用命中率），以及排查泄漏（租出数居高不下）。
    /// </summary>
    public readonly struct PoolStats
    {
        /// <summary>累计 <c>Instantiate</c> 次数。稳态下这个值应该停止增长。</summary>
        public readonly int Created;

        /// <summary>累计复用命中次数。稳态下应该持续增长。</summary>
        public readonly int Reused;

        /// <summary>当前空闲（已隐藏、等待复用）的实例数。</summary>
        public readonly int Idle;

        /// <summary>当前租出（在使用中）的实例数。</summary>
        public readonly int Rented;

        /// <summary>当前实例总数 = 空闲 + 租出。</summary>
        public int Total => Idle + Rented;

        public PoolStats(int created, int reused, int idle, int rented)
        {
            Created = created;
            Reused = reused;
            Idle = idle;
            Rented = rented;
        }

        public override string ToString()
        {
            return $"创建={Created} 复用={Reused} 空闲={Idle} 租出={Rented} 总数={Total}";
        }
    }
}
