/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/08/06 11:04:42
* Note  : 
***************************************************************/
using GameFramework;
using System.Threading.Tasks;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// Await包装类
    /// </summary>
    public class AwaitDataWrap<T> : IReference
    {
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// TaskCompletionSource
        /// </summary>
        public TaskCompletionSource<T> Source { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static AwaitDataWrap<T> Create(object userData, TaskCompletionSource<T> source)
        {
            AwaitDataWrap<T> awaitDataWrap = ReferencePool.Acquire<AwaitDataWrap<T>>();
            awaitDataWrap.UserData = userData;
            awaitDataWrap.Source = source;
            return awaitDataWrap;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            UserData = null;
            Source = null;
        }
    }
}
