using System;
using UnityEngine;

namespace BS.ResourceManagement
{
    /// <summary>
    /// 池中一个实例的内部包装。
    ///
    /// 存在的意义：
    /// 1. 缓存该实例上的全部 <see cref="IPoolable"/>，避免每次 Spawn 都调用 GetComponents（会分配数组）。
    /// 2. 记录租借状态与时间，用于「重复归还」与「泄漏」检测。
    /// </summary>
    internal sealed class PooledInstance
    {
        /// <summary>该实例所属的池。</summary>
        public readonly GameObject Go;

        /// <summary>实例上的全部可池化组件。仅构造时取一次。</summary>
        public readonly IPoolable[] Poolables;

        /// <summary>所属池。由 <see cref="Pool.CreateInstance"/> 赋值。</summary>
        public Pool Owner;

        /// <summary>是否处于「已租出」状态。</summary>
        public bool IsRented;

        /// <summary>本次租出的起始时刻（未缩放时间），仅用于泄漏诊断。</summary>
        public float RentStartUnscaledTime;

        public PooledInstance(GameObject go)
        {
            Go = go;
            Poolables = go.GetComponents<IPoolable>();
        }

        /// <summary>
        /// 逐个调用 <see cref="IPoolable.OnSpawn"/>。
        /// 单个组件抛异常不会中断其余组件，但会完整打印堆栈——便于定位问题组件。
        /// </summary>
        public void InvokeOnSpawn()
        {
            for (int i = 0; i < Poolables.Length; i++)
            {
                try
                {
                    Poolables[i].OnSpawn();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, Go);
                }
            }
        }

        /// <summary>
        /// 逐个调用 <see cref="IPoolable.OnDespawn"/>。异常处理策略同 <see cref="InvokeOnSpawn"/>。
        /// </summary>
        public void InvokeOnDespawn()
        {
            for (int i = 0; i < Poolables.Length; i++)
            {
                try
                {
                    Poolables[i].OnDespawn();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, Go);
                }
            }
        }
    }
}
