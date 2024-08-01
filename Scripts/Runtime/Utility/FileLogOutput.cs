/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/26 16:29:53
* Note  : 日志输出到文件，参考QFramework的实现
***************************************************************/
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using System;

namespace UnityGameFramework.Runtime
{
    public class FileLogOutput : IDisposable
    {
        /// <summary>
        /// 日志输出数据
        /// </summary>
        public struct LogOutputData
        {
            /// <summary>
            /// 日志信息
            /// </summary>
            public string Log { get; set; }

            /// <summary>
            /// 日志堆栈
            /// </summary>
            public string Track { get; set; }

            /// <summary>
            /// 日志输出等级
            /// </summary>
            public LogType LogType { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<LogType, LogOutputLevel> s_LogTypeLevelDict = new Dictionary<LogType, LogOutputLevel>
        {
            { LogType.Log, LogOutputLevel.LOG },
            { LogType.Warning, LogOutputLevel.WARNING },
            { LogType.Assert, LogOutputLevel.ASSERT },
            { LogType.Error, LogOutputLevel.ERROR },
            { LogType.Exception, LogOutputLevel.ERROR },
        };

        /// <summary>
        /// 
        /// </summary>
        static readonly string s_DevicePersistentPath = Application.persistentDataPath;

        /// <summary>
        /// 
        /// </summary>
        static readonly string s_LogPath = "Log";

        /// <summary>
        /// 
        /// </summary>
        private LogOutputLevel m_LogOutputLevel;

        /// <summary>
        /// 
        /// </summary>
        private Queue<LogOutputData> m_WritingLogQueue = null;

        /// <summary>
        /// 
        /// </summary>
        private Queue<LogOutputData> m_WaitingLogQueue = null;

        /// <summary>
        /// 
        /// </summary>
        private object m_LogLock = null;

        /// <summary>
        /// 
        /// </summary>
        private bool m_IsRunning = false;

        /// <summary>
        /// 
        /// </summary>
        private StreamWriter m_LogWriter = null;

        /// <summary>
        /// 
        /// </summary>
        private int m_MainThreadID = 0;

        /// <summary>
        /// 
        /// </summary>
        public FileLogOutput(int mainThreadID, LogOutputLevel logOutputLevel)
        {
            m_MainThreadID = mainThreadID;
            m_LogOutputLevel = logOutputLevel;
            Application.logMessageReceived += LogCallback;
            Application.logMessageReceivedThreaded += LogMultiThreadCallback;
            m_WritingLogQueue = new Queue<LogOutputData>();
            m_WaitingLogQueue = new Queue<LogOutputData>();
            m_LogLock = new object();

            string logName = DateTime.Now.ToString("yyy-MM-dd-HH-mm-ss");
            string logPath = string.Format("{0}/{1}/{2}.txt", s_DevicePersistentPath, s_LogPath, logName);
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            string logDir = Path.GetDirectoryName(logPath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            m_LogWriter = new StreamWriter(logPath);
            m_LogWriter.AutoFlush = true;
            m_IsRunning = true;

            // hxd 2024/07/26 一旦一个线程运行完毕之后，主程序关闭会自动关闭，所以没必要持有线程引用
            Thread fileLogThread = new Thread(new ThreadStart(WriteLog));
            fileLogThread.Start();
        }

        /// <summary>
        /// 
        /// </summary>
        void WriteLog()
        {
            while (m_IsRunning)
            {
                if (m_WritingLogQueue.Count == 0)
                {
                    lock (m_LogLock)
                    {
                        while (m_WaitingLogQueue.Count == 0)
                            Monitor.Wait(m_LogLock);
                        Queue<LogOutputData> tmpQueue = m_WritingLogQueue;
                        m_WritingLogQueue = m_WaitingLogQueue;
                        m_WaitingLogQueue = tmpQueue;
                    }
                }
                else
                {
                    while (m_WritingLogQueue.Count > 0)
                    {
                        LogOutputData log = m_WritingLogQueue.Dequeue();
                        try
                        {
                            // hxd 2024/07/26 仅错误和异常输出堆栈
                            if (log.LogType == LogType.Error || log.LogType == LogType.Exception)
                            {
                                m_LogWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                                m_LogWriter.WriteLine(System.DateTime.Now.ToString() + "\t" + log.Log + "\n");
                                m_LogWriter.WriteLine(log.Track);
                                m_LogWriter.WriteLine("---------------------------------------------------------------------------------------------------------------------");
                            }
                            else
                            {
                                m_LogWriter.WriteLine(System.DateTime.Now.ToString() + "\t" + log.Log);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            // hxd 2024/07/29 “Can not write to a closed TextWriter.”可能写的过程中被关闭了文件流，
                            m_WritingLogQueue.Clear();
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logData"></param>
        public void Log(LogOutputData logData)
        {
            if(m_IsRunning)
            {
                lock (m_LogLock)
                {
                    m_WaitingLogQueue.Enqueue(logData);
                    Monitor.Pulse(m_LogLock);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            m_IsRunning = false;
            m_LogWriter.Close();
            Application.logMessageReceived -= LogCallback;
            Application.logMessageReceivedThreaded -= LogMultiThreadCallback;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            Dispose();
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
            if (m_MainThreadID == Thread.CurrentThread.ManagedThreadId)
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
            if (m_MainThreadID != Thread.CurrentThread.ManagedThreadId)
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
            if (s_LogTypeLevelDict[type] >= m_LogOutputLevel)
            {
                Log(new LogOutputData
                {
                    Log = log,
                    Track = track,
                    LogType = type,
                });
            }
        }

        #endregion
    }
}