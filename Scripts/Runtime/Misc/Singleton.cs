/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/06/20 14:05:41
* Note  : mono单例组件
***************************************************************/
using UnityEngine;

namespace UnityGameFramework.Runtime.Misc
{
    public abstract class Singleton<T> : Singleton where T : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        private static T s_Instance;

        /// <summary>
        /// ReSharper disable once StaticMemberInGenericType
        /// </summary>
        private static readonly object s_Lock = new object();

        /// <summary>
        /// 
        /// </summary>
        [SerializeField] 
        private bool m_Persistent = true;

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public static T Instance
        {
            get
            {
                if (Application.isEditor && !Application.isPlaying)
                {
#if UNITY_EDITOR
                    var instances = FindObjectsOfType<T>();
                    var count = instances.Length;
                    if (count > 0)
                    {
                        if (count == 1)
                            return s_Instance = instances[0];
                    }

#endif
                    return null;
                }
                else
                {
                    if (Quitting)
                    {
                        Debug.LogWarning(
                            $"[{nameof(Singleton)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                        // ReSharper disable once AssignNullToNotNullAttribute
                        return null;
                    }

                    lock (s_Lock)
                    {
                        if (s_Instance != null)
                            return s_Instance;
                        var instances = FindObjectsOfType<T>();
                        var count = instances.Length;
                        if (count > 0)
                        {
                            if (count == 1)
                                return s_Instance = instances[0];
                            Debug.LogWarning(
                                $"[{nameof(Singleton)}<{typeof(T)}>] There should never be more than one {nameof(Singleton)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                            for (var i = 1; i < instances.Length; i++)
                                Destroy(instances[i]);
                            return s_Instance = instances[0];
                        }

                        Debug.Log(
                            $"[{nameof(Singleton)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                        return s_Instance = new GameObject($"({nameof(Singleton)}){typeof(T)}")
                            .AddComponent<T>();
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            if (m_Persistent)
                DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnAwake()
        {
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract class Singleton : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// 是否推出程序
        /// </summary>
        public static bool Quitting { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        private void OnApplicationQuit()
        {
            Quitting = true;
        }

        #endregion
    }
}
