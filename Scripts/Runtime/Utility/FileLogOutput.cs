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
    public class FileLogOutput
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
        public FileLogOutput()
        {
            m_WritingLogQueue = new Queue<LogOutputData>();
            m_WaitingLogQueue = new Queue<LogOutputData>();
            m_LogLock = new object();

            string logName = DateTime.Now.ToString("yyy-MM-dd HH:mm:ss");
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
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logData"></param>
        public void Log(LogOutputData logData)
        {
            lock (m_LogLock)
            {
                m_WaitingLogQueue.Enqueue(logData);
                Monitor.Pulse(m_LogLock);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            m_IsRunning = false;
            m_LogWriter.Close();
            m_LogWriter.Dispose();
        }
    }
}