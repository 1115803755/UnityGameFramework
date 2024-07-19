/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:20:35
* Note  : Scriptable单例组件
***************************************************************/
using System;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityGameFramework.Editor.Settings
{
    public class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        private static T s_Instance;
        public static T Instance
        {
            get
            {
                if (!s_Instance)
                {
                    LoadOrCreate();
                }
                return s_Instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static T LoadOrCreate()
        {
            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                var arr = InternalEditorUtility.LoadSerializedFileAndForget(filePath);
                s_Instance = arr.Length > 0 ? arr[0] as T : s_Instance ?? CreateInstance<T>();
            }
            else
            {
                Debug.LogError($"save location of {nameof(ScriptableSingleton<T>)} is invalid");
            }
            return s_Instance;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saveAsText"></param>
        public static void Save(bool saveAsText = true)
        {
            if (!s_Instance)
            {
                Debug.LogError("Cannot save ScriptableSingleton: no instance!");
                return;
            }

            string filePath = GetFilePath();
            if (!string.IsNullOrEmpty(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                UnityEngine.Object[] obj = new T[1] { s_Instance };
                InternalEditorUtility.SaveToSerializedFileAndForget(obj, filePath, saveAsText);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static string GetFilePath()
        {
            return typeof(T).GetCustomAttributes(inherit: true)
                .Where(v => v is FilePathAttribute)
                .Cast<FilePathAttribute>()
                .FirstOrDefault()
                ?.filepath;
        }
    }

    /// <summary>
    /// 单例资源存放路径特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class FilePathAttribute : Attribute
    {
        internal string filepath;
        /// <summary>
        /// 单例存放路径
        /// </summary>
        /// <param name="path">相对 Project 路径</param>
        public FilePathAttribute(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid relative path (it is empty)");
            }
            if (path[0] == '/')
            {
                path = path.Substring(1);
            }
            filepath = path;
        }
    }
}
