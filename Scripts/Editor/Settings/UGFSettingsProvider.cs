/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:43:38
* Note  : UGF配置面板
***************************************************************/
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityGameFramework.Editor.Settings
{
    public class UGFSettingsProvider : ABSettingsProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private SerializedObject _serializedObject;

        /// <summary>
        /// 
        /// </summary>
        private SerializedProperty _toolsConfigRootDir;

        /// <summary>
        /// 
        /// </summary>
        private GUIStyle _buttonStyle;

        /// <summary>
        /// 
        /// </summary>
        private GUIStyle _uiStyle;

        /// <summary>
        /// 
        /// </summary>
        public UGFSettingsProvider() : base("Project/UGFSettings", SettingsScope.Project) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchContext"></param>
        /// <param name="rootElement"></param>
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            EditorStatusWatcher.OnEditorFocused += OnEditorFocused;
            InitGUI();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitGUI()
        {
            var setting = UGFSettings.LoadOrCreate();
            _serializedObject?.Dispose();
            _serializedObject = new SerializedObject(setting);
            _toolsConfigRootDir = _serializedObject.FindProperty("toolsConfigRootDir");
            _uiStyle = _uiStyle ?? new GUIStyle() 
            {
                fontSize = 14,
                normal =
                {
                    textColor = Color.white,
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEditorFocused()
        {
            InitGUI();
            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnTitleBarGUI()
        {
            base.OnTitleBarGUI();
            var rect = GUILayoutUtility.GetLastRect();
            _buttonStyle = _buttonStyle ?? GUI.skin.GetStyle("IconButton");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="searchContext"></param>
        public override void OnGUI(string searchContext)
        {
            using (CreateSettingsWindowGUIScope())
            {
                // 解决编辑器打包时出现的 _serializedObject.targetObject 意外销毁的情况
                if (_serializedObject == null || !_serializedObject.targetObject)
                {
                    InitGUI();
                }
                _serializedObject.Update();
                EditorGUI.BeginChangeCheck();

                SelectionFolderPath("工具相关的配置文件保存根目录", "选择路径", _toolsConfigRootDir, _uiStyle);

                if (EditorGUI.EndChangeCheck())
                {
                    _serializedObject.ApplyModifiedProperties();
                    UGFSettings.Save();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private IDisposable CreateSettingsWindowGUIScope()
        {
            var unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var type = unityEditorAssembly.GetType("UnityEditor.SettingsWindow+GUIScope");
            return Activator.CreateInstance(type) as IDisposable;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnDeactivate()
        {
            base.OnDeactivate();
            EditorStatusWatcher.OnEditorFocused -= OnEditorFocused;
            UGFSettings.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        static UGFSettingsProvider provider;
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            if (UGFSettings.Instance && provider == null)
            {
                provider = new UGFSettingsProvider();
                using (var so = new SerializedObject(UGFSettings.Instance))
                {
                    provider.keywords = GetSearchKeywordsFromSerializedObject(so);
                }
            }
            return provider;
        }
    }
}