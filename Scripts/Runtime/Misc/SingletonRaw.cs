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
        private static T s_Instance;

        /// <summary>
        /// 
        /// </summary>
        private static readonly object s_Lock = new object();

        /// <summary>
        /// 
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    lock (s_Lock)
                    {
                        if (s_Instance == null)
                        {
                            s_Instance = (T)Activator.CreateInstance(typeof(T));
                        }
                    }
                }

                return s_Instance;
            }
        }
    }
}
