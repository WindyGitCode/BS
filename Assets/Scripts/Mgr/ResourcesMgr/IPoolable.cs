namespace BS.ResourceManagement
{
    /// <summary>
    /// 可池化对象的生命周期契约。
    ///
    /// 【接入要求 —— 这是池化最容易踩的坑】
    /// 实现本接口的类型，必须把「每次复用都要重做的初始化」放进 <see cref="OnSpawn"/>，
    /// 而不是放在 Unity 的 <c>Start()</c> 里。
    /// 原因：对象从池中取出复用时，<c>Awake()</c> 和 <c>Start()</c> 都不会再执行，
    /// 只有 <see cref="OnSpawn"/> 会被调用。若继续依赖 <c>Start()</c>，
    /// 复用出来的对象会带着上一次残留的状态（血量、isDead 标志、导航停止状态等）。
    ///
    /// 【职责分工建议】
    /// <list type="bullet">
    /// <item><c>Awake()</c>   —— 只做一次的事：缓存组件引用、创建 WaitForSeconds 等。</item>
    /// <item><see cref="OnSpawn"/>   —— 每次复用都要做：重置状态/血量/动画/导航/计时器。</item>
    /// <item><see cref="OnDespawn"/> —— 归还前清理：停协程、DOTween 的 DOKill、停音效。</item>
    /// </list>
    ///
    /// 实现类可以同时是 <c>Interactable</c> 这类既有基类的子类，本接口不占用继承位。
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 对象从池中取出、被激活之后立即调用。
        /// 此处必须完成全部运行时状态的初始化，使其等同于「刚被创建出来的对象」。
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// 对象归还池之前调用，此时对象仍处于激活状态，
        /// 因此可以安全地在此处 <c>StopAllCoroutines()</c> / <c>DOTween.Kill()</c>。
        /// </summary>
        void OnDespawn();
    }
}
