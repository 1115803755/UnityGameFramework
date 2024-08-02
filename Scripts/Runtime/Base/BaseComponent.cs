//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using GameFramework.Localization;
using GameFramework.Resource;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 基础组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Base")]
    public sealed class BaseComponent : GameFrameworkComponent
    {
        private const int DefaultDpi = 96;  // default windows dpi

        private float m_GameSpeedBeforePause = 1f;

        [SerializeField]
        private bool m_EditorResourceMode = true;

        [SerializeField]
        private Language m_EditorLanguage = Language.Unspecified;

        [SerializeField]
        private string m_TextHelperTypeName = "UnityGameFramework.Runtime.DefaultTextHelper";

        [SerializeField]
        private string m_VersionHelperTypeName = "UnityGameFramework.Runtime.DefaultVersionHelper";

        [SerializeField]
        private string m_LogHelperTypeName = "UnityGameFramework.Runtime.DefaultLogHelper";

        [SerializeField]
        private bool m_LogOutputFile;

        [SerializeField]
        private LogOutputLevel m_LogOutputLevel = LogOutputLevel.MAX;

        [SerializeField]
        private string m_CompressionHelperTypeName = "UnityGameFramework.Runtime.DefaultCompressionHelper";

        [SerializeField]
        private string m_JsonHelperTypeName = "UnityGameFramework.Runtime.DefaultJsonHelper";

        [SerializeField]
        private int m_FrameRate = 30;

        [SerializeField]
        private float m_GameSpeed = 1f;

        [SerializeField]
        private bool m_RunInBackground = true;

        [SerializeField]
        private bool m_NeverSleep = true;

        /// <summary>
        /// 获取或设置是否使用编辑器资源模式（仅编辑器内有效）。
        /// </summary>
        public bool EditorResourceMode
        {
            get
            {
                return m_EditorResourceMode;
            }
            set
            {
                m_EditorResourceMode = value;
            }
        }

        /// <summary>
        /// 获取或设置编辑器语言（仅编辑器内有效）。
        /// </summary>
        public Language EditorLanguage
        {
            get
            {
                return m_EditorLanguage;
            }
            set
            {
                m_EditorLanguage = value;
            }
        }

        /// <summary>
        /// 获取或设置编辑器资源辅助器。
        /// </summary>
        public IResourceManager EditorResourceHelper
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get
            {
                return m_FrameRate;
            }
            set
            {
                Application.targetFrameRate = m_FrameRate = value;
            }
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get
            {
                return m_GameSpeed;
            }
            set
            {
                Time.timeScale = m_GameSpeed = value >= 0f ? value : 0f;
            }
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        public bool IsGamePaused
        {
            get
            {
                return m_GameSpeed <= 0f;
            }
        }

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        public bool IsNormalGameSpeed
        {
            get
            {
                return m_GameSpeed == 1f;
            }
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get
            {
                return m_RunInBackground;
            }
            set
            {
                Application.runInBackground = m_RunInBackground = value;
            }
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public bool NeverSleep
        {
            get
            {
                return m_NeverSleep;
            }
            set
            {
                m_NeverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }

        /// <summary>
        /// 主线程id
        /// </summary>
        private int m_MainThreadID;
        public int MainThreadID => m_MainThreadID;

        /// <summary>
        /// 日志写到文件工具类，仅当m_LogOutputFile为true时有效
        /// </summary>
        FileLogOutput m_FileLogOutput;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            m_MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#if UNITY_5_4_OR_NEWER
            if (m_LogOutputFile)
            {
                m_FileLogOutput = new FileLogOutput(m_MainThreadID, m_LogOutputLevel);
            }
#endif

            InitTextHelper();
            InitVersionHelper();
            InitLogHelper();
            Log.Info("Game Framework Version: {0}", GameFramework.Version.GameFrameworkVersion);
            Log.Info("Game Version: {0} ({1})", GameFramework.Version.GameVersion, GameFramework.Version.InternalGameVersion);
            Log.Info("Unity Version: {0}", Application.unityVersion);

#if UNITY_5_3_OR_NEWER || UNITY_5_3
            InitCompressionHelper();
            InitJsonHelper();

            Utility.Converter.ScreenDpi = Screen.dpi;
            if (Utility.Converter.ScreenDpi <= 0)
            {
                Utility.Converter.ScreenDpi = DefaultDpi;
            }

            m_EditorResourceMode &= Application.isEditor;
            if (m_EditorResourceMode)
            {
                Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
            }

            Application.targetFrameRate = m_FrameRate;
            Time.timeScale = m_GameSpeed;
            Application.runInBackground = m_RunInBackground;
            Screen.sleepTimeout = m_NeverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
#else
            Log.Error("Game Framework only applies with Unity 5.3 and above, but current Unity version is {0}.", Application.unityVersion);
            GameEntry.Shutdown(ShutdownType.Quit);
#endif
#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnApplicationQuit()
        {
            Log.Info("OnApplicationQuit");

            // hxd 2024/08/02 关于OnApplicationQuit与OnDestroy调用问题
            // OnApplicationQuit调用先于OnDestroy
            // Editor模式下，不管是正常关闭、还是调用UnityEditor.EditorApplication.isPlaying(= Application.Shutdown)都会正常调用OnApplicationQuit与OnDestroy，但是任务管理器杀进程OnApplicationQuit都不会调用，OnDestroy公司的会调用家里的不会调用?
            // Windows发布包，不管是正常关闭、alt + f4、任务管理器杀进程、还是调用Application.Shutdown都会正常调用OnApplicationQuit与OnDestroy
            // Android发布包在手机上，正常关闭（调用Application.Shutdown）会正常调用OnApplicationQuit与OnDestroy、但是直接杀进程都不会调用（据说ios会调用OnApplicationQuit）
            // Android发布包在模拟器上，正常关闭（调用Application.Shutdown）会正常调用OnApplicationQuit与OnDestroy、但是直接杀进程（除了第一次）都不会调用（重启模拟器第一次会调用OnApplicationQuit）

            // hxd 2024/08/01 为什么不直接放到OnDestroy中？还能避免由于restart没调用的问题？
            // 猜测可能是因为希望让程序退出时OnLowMemory尽早注销掉，少执行一些逻辑，避免因为回调一些耗时操作让程序退出更卡顿？
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif

            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
            Log.Info("OnDestroy");
            // hxd 2024/07/29 e大原来设定只在OnApplicationQuit调用，但是经过测试发现，框架提供的GameEntry.Shutdown实际上不会回调OnApplicationQuit，
            // 因为内部先销毁了BaseComponent，导致OnApplicationQuit不会被回调（经测试，只要对象隐藏或者销毁，都不会回调，不管隐藏和销毁是否在Application.Quit()之前或者之后调用），
            // 另外，GameEntry.Shutdown提供的ReStart和None操作并不会执行OnApplicationQuit，导致注册的回调没注销
            // 故这里采用双保险机制，在OnApplication和OnDestroy中均执行一次确保一定回正确注销（Restart时也能正确注销，因为场景的重新加载仅会调用OnDestroy方法）
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif

#if UNITY_5_4_OR_NEWER
            if (m_LogOutputFile)
            {
                // hxd 2024/08/01 c#打开一个文件流可以不关闭么？
                // 在C#中，通常使用FileStream类来打开文件流。文件流打开后，如果不主动关闭，它将一直保持开放状态直到进程结束。
                // 但是，在实际应用中，为了避免资源泄露和同步问题，应该始终在完成文件操作后关闭文件流。
                // 一般来说独占文件打开的话，如果不关闭文件流，那么其它进程就无法读取这个文件了。
                // 二在使用写入模式打开文件的时候，如果不进行close可能会有部分数据在缓存中没有真实写入文件中，这样其它程序打开文件时看到的数据就不完整了
                m_FileLogOutput?.Close();
                m_FileLogOutput = null;
            }
#endif
            GameFrameworkEntry.Shutdown();
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = m_GameSpeedBeforePause;
        }

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        /// <summary>
        /// 
        /// </summary>
        internal void Shutdown()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitTextHelper()
        {
            if (string.IsNullOrEmpty(m_TextHelperTypeName))
            {
                return;
            }

            Type textHelperType = Utility.Assembly.GetType(m_TextHelperTypeName);
            if (textHelperType == null)
            {
                Log.Error("Can not find text helper type '{0}'.", m_TextHelperTypeName);
                return;
            }

            Utility.Text.ITextHelper textHelper = (Utility.Text.ITextHelper)Activator.CreateInstance(textHelperType);
            if (textHelper == null)
            {
                Log.Error("Can not create text helper instance '{0}'.", m_TextHelperTypeName);
                return;
            }

            Utility.Text.SetTextHelper(textHelper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="GameFrameworkException"></exception>
        private void InitVersionHelper()
        {
            if (string.IsNullOrEmpty(m_VersionHelperTypeName))
            {
                return;
            }

            Type versionHelperType = Utility.Assembly.GetType(m_VersionHelperTypeName);
            if (versionHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find version helper type '{0}'.", m_VersionHelperTypeName));
            }

            GameFramework.Version.IVersionHelper versionHelper = (GameFramework.Version.IVersionHelper)Activator.CreateInstance(versionHelperType);
            if (versionHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create version helper instance '{0}'.", m_VersionHelperTypeName));
            }

            GameFramework.Version.SetVersionHelper(versionHelper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="GameFrameworkException"></exception>
        private void InitLogHelper()
        {
            if (string.IsNullOrEmpty(m_LogHelperTypeName))
            {
                return;
            }

            Type logHelperType = Utility.Assembly.GetType(m_LogHelperTypeName);
            if (logHelperType == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find log helper type '{0}'.", m_LogHelperTypeName));
            }

            GameFrameworkLog.ILogHelper logHelper = (GameFrameworkLog.ILogHelper)Activator.CreateInstance(logHelperType);
            if (logHelper == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not create log helper instance '{0}'.", m_LogHelperTypeName));
            }

            GameFrameworkLog.SetLogHelper(logHelper);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitCompressionHelper()
        {
            if (string.IsNullOrEmpty(m_CompressionHelperTypeName))
            {
                return;
            }

            Type compressionHelperType = Utility.Assembly.GetType(m_CompressionHelperTypeName);
            if (compressionHelperType == null)
            {
                Log.Error("Can not find compression helper type '{0}'.", m_CompressionHelperTypeName);
                return;
            }

            Utility.Compression.ICompressionHelper compressionHelper = (Utility.Compression.ICompressionHelper)Activator.CreateInstance(compressionHelperType);
            if (compressionHelper == null)
            {
                Log.Error("Can not create compression helper instance '{0}'.", m_CompressionHelperTypeName);
                return;
            }

            Utility.Compression.SetCompressionHelper(compressionHelper);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitJsonHelper()
        {
            if (string.IsNullOrEmpty(m_JsonHelperTypeName))
            {
                return;
            }

            Type jsonHelperType = Utility.Assembly.GetType(m_JsonHelperTypeName);
            if (jsonHelperType == null)
            {
                Log.Error("Can not find JSON helper type '{0}'.", m_JsonHelperTypeName);
                return;
            }

            Utility.Json.IJsonHelper jsonHelper = (Utility.Json.IJsonHelper)Activator.CreateInstance(jsonHelperType);
            if (jsonHelper == null)
            {
                Log.Error("Can not create JSON helper instance '{0}'.", m_JsonHelperTypeName);
                return;
            }

            Utility.Json.SetJsonHelper(jsonHelper);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnLowMemory()
        {
            Log.Info("Low memory reported...");

            ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
            if (objectPoolComponent != null)
            {
                objectPoolComponent.ReleaseAllUnused();
            }

            ResourceComponent resourceCompoent = GameEntry.GetComponent<ResourceComponent>();
            if (resourceCompoent != null)
            {
                resourceCompoent.ForceUnloadUnusedAssets(true);
            }
        }
    }
}