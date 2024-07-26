/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:43:38
* Note  : UGF配置面板
***************************************************************/
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityGameFramework.Editor.Settings
{
    public class UGFSettingsProvider : ABSettingsProvider
    {
        /// <summary>
        /// 
        /// </summary>
        private SerializedObject m_SerializedObject;

        /// <summary>
        /// 
        /// </summary>
        private SerializedProperty m_EditorConfigRootDir;

        /// <summary>
        /// 
        /// </summary>
        private GUIStyle m_ButtonStyle;

        /// <summary>
        /// 
        /// </summary>
        private GUIStyle m_UIStyle;

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
            UGFSettingsEditorStatusWatcher.OnEditorFocused += OnEditorFocused;
            InitGUI();
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitGUI()
        {
            var setting = UGFSettings.LoadOrCreate();
            m_SerializedObject?.Dispose();
            m_SerializedObject = new SerializedObject(setting);
            m_EditorConfigRootDir = m_SerializedObject.FindProperty(nameof(setting.EditorConfigRootDir));
            m_UIStyle = m_UIStyle ?? new GUIStyle() 
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
            m_ButtonStyle = m_ButtonStyle ?? GUI.skin.GetStyle("IconButton");

            #region  绘制官方网站跳转按钮
            var w = rect.x + rect.width;
            rect.x = w - 57;
            rect.y += 6;
            rect.width = rect.height = 18;
            var content = EditorGUIUtility.IconContent("_Help");
            content.tooltip = "点击访问 UGF 官方文档";
            if (GUI.Button(rect, content, m_ButtonStyle))
            {
                Application.OpenURL("https://gameframework.cn/document/");
            }
            #endregion
            #region 绘制 Preset
            rect.x += 19;
            content = EditorGUIUtility.IconContent("Preset.Context");
            content.tooltip = "点击存储或加载 Preset .";
            if (GUI.Button(rect, content, m_ButtonStyle))
            {
                var target = UGFSettings.Instance;
                var receiver = ScriptableObject.CreateInstance<SettingsPresetReceiver>();
                receiver.Init(target, this);
                PresetSelector.ShowSelector(target, null, true, receiver);
            }
            #endregion
            #region 绘制 Reset
            rect.x += 19;
            content = EditorGUIUtility.IconContent(
#if UNITY_2021_3_OR_NEWER
                "pane options"
#else
                "_Popup"
#endif
                );
            content.tooltip = "Reset";
            if (GUI.Button(rect, content, m_ButtonStyle))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Reset"), false, () =>
                {
                    Undo.RecordObject(UGFSettings.Instance, "Capture Value for Reset");
                    var dv = ScriptableObject.CreateInstance<UGFSettings>();
                    var json = EditorJsonUtility.ToJson(dv);
                    UnityEngine.Object.DestroyImmediate(dv);
                    EditorJsonUtility.FromJsonOverwrite(json, UGFSettings.Instance);
                    UGFSettings.Save();
                });
                menu.ShowAsContext();
            }
            #endregion
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
                if (m_SerializedObject == null || !m_SerializedObject.targetObject)
                {
                    InitGUI();
                }
                m_SerializedObject.Update();
                EditorGUI.BeginChangeCheck();

                SelectionFolderPath("编辑器相关配置文件保存根目录", "选择路径", 
                    "BuildSettings.xml\nResourceEditor.xml\nResourceCollection.xml\nResourceBuilder.xml", m_EditorConfigRootDir, m_UIStyle);

                EditorGUILayout.Space(10);

                if (EditorGUI.EndChangeCheck())
                {
                    m_SerializedObject.ApplyModifiedProperties();
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
            UGFSettingsEditorStatusWatcher.OnEditorFocused -= OnEditorFocused;
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