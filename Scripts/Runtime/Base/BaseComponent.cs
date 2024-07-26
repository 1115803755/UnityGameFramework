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
        private int m_mainThreadID;
        public int MainThreadID => m_mainThreadID;

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

            m_mainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

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

#if UNITY_5_4_OR_NEWER
            if(m_LogOutputFile)
            {
                Application.logMessageReceived += LogCallback;
                Application.logMessageReceivedThreaded += LogMultiThreadCallback;
                m_FileLogOutput = new FileLogOutput();
            }
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
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif

#if UNITY_5_4_OR_NEWER
            if (m_LogOutputFile)
            {
                m_FileLogOutput.Close();
                Application.logMessageReceived -= LogCallback;
                Application.logMessageReceivedThreaded -= LogMultiThreadCallback;
            }
#endif
            StopAllCoroutines();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDestroy()
        {
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

        #region LogCallBack 

        // hxd 2024/07/26 参考QFramework的设计
        /// <summary>
        /// 日志调用回调，主线程和其他线程都会回调这个函数，在其中根据配置输出日志
        /// （但是在真机上如果发生 Error 或者 Exception 时,收不到堆栈信息，此时配合LogMultiThreadCallback实现）
        /// </summary>
        /// <param name="log">日志</param>
        /// <param name="track">堆栈追踪</param>
        /// <param name="type">日志类型</param>
        private void LogCallback(string log, string track, LogType type)
        {
            if (m_mainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                OutputLog(log, track, type);
            }
        }

        /// <summary>
        /// 日志回调（需要在处理 Log 信息的时候要保证线程安全）
        /// </summary>
        /// <param name="log"></param>
        /// <param name="track"></param>
        /// <param name="type"></param>
        private void LogMultiThreadCallback(string log, string track, LogType type)
        {
            if (m_mainThreadID != System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                OutputLog(log, track, type);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="log"></param>
        /// <param name="track"></param>
        /// <param name="type"></param>
        private void OutputLog(string log, string track, LogType type)
        {
            if (FileLogOutput.s_LogTypeLevelDict[type] >= m_LogOutputLevel)
            {
                m_FileLogOutput.Log(new FileLogOutput.LogOutputData { 
                    Log = log,
                    Track = track,
                    LogType = type,
                });
            }
        }

        #endregion
    }
}