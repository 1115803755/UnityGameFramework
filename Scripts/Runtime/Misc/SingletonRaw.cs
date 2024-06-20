/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/06/20 14:08:55
* Note  : 
***************************************************************/
using System;

namespace UnityGameFramework.Runtime.Misc
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonRaw<T>
    {
        /// <summary>
        /// 
        /// </summary>
        private static T _instance;

        /// <summary>
        /// 
        /// </summary>
        private static readonly object _lock = new object();

        /// <summary>
        /// 
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = (T)Activator.CreateInstance(typeof(T));
                        }
                    }
                }

                return _instance;
            }
        }
    }
}
