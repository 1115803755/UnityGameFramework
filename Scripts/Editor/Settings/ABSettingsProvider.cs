/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/19 9:11:45
* Note  : SettingsProvider抽象基类
***************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityGameFramework.Editor.Settings
{
    public abstract class ABSettingsProvider : SettingsProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="scopes"></param>
        /// <param name="keywords"></param>
        public ABSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="btnLabel"></param>
        /// <param name="property"></param>
        /// <param name="uiStyle"></param>
        protected void  SelectionFolderPath(string label, string btnLabel, SerializedProperty property, GUIStyle uiStyle)
        {
            EditorGUILayout.LabelField(label, uiStyle);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(property.stringValue);
            if (GUILayout.Button(btnLabel, GUILayout.Width(140f)))
            {
                string folder = Path.Combine(Application.dataPath, property.stringValue);
                if (!Directory.Exists(folder))
                {
                    folder = Application.dataPath;
                }
                string path = EditorUtility.OpenFolderPanel(btnLabel, folder, "");
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.Equals(Application.dataPath))
                        property.stringValue = "Assets/";
                    else
                        property.stringValue = "Assets" + path.Replace(Application.dataPath, "") + "/";
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);
        }
    }
}

