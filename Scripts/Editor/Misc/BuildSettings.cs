//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace UnityGameFramework.Editor
{
    /// <summary>
    /// 构建配置相关的实用函数。
    /// </summary>
    internal static class BuildSettings
    {
        /// <summary>
        /// 
        /// </summary>
        internal static BuildSettingsController s_Controller = null;

        /// <summary>
        /// 将构建场景设置为默认。
        /// </summary>
        [MenuItem("Game Framework/Scenes in Build Settings/Default Scenes", false, 20)]
        public static void DefaultScenes()
        {
            s_Controller = s_Controller ?? new BuildSettingsController();
            if(s_Controller.Load())
            {
                HashSet<string> sceneNames = new HashSet<string>();
                foreach (string sceneName in s_Controller.defaultScenes)
                {
                    sceneNames.Add(sceneName);
                }

                List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
                foreach (string sceneName in sceneNames)
                {
                    scenes.Add(new EditorBuildSettingsScene(sceneName, true));
                }

                EditorBuildSettings.scenes = scenes.ToArray();

                Debug.Log("Set scenes of build settings to default scenes.");
            }
            else
            {
                Debug.Log("load BuildSettings.xml failed");
            }
        }

        /// <summary>
        /// 将构建场景设置为所有。
        /// </summary>
        [MenuItem("Game Framework/Scenes in Build Settings/All Scenes", false, 21)]
        public static void AllScenes()
        {
            s_Controller = s_Controller ?? new BuildSettingsController();
            if (s_Controller.Load())
            {
                HashSet<string> sceneNames = new HashSet<string>();
                foreach (string sceneName in s_Controller.defaultScenes)
                {
                    sceneNames.Add(sceneName);
                }

                string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", s_Controller.searchScenePaths.ToArray());
                foreach (string sceneGuid in sceneGuids)
                {
                    string sceneName = AssetDatabase.GUIDToAssetPath(sceneGuid);
                    sceneNames.Add(sceneName);
                }

                List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
                foreach (string sceneName in sceneNames)
                {
                    scenes.Add(new EditorBuildSettingsScene(sceneName, true));
                }

                EditorBuildSettings.scenes = scenes.ToArray();

                Debug.Log("Set scenes of build settings to all scenes.");
            }
            else
            {
                Debug.Log("load BuildSettings.xml failed");
            }
        }
    }

    /// <summary>
    /// 构建配置面板
    /// </summary>
    internal sealed class BuilderSetting : EditorWindow
    {
        [MenuItem("Game Framework/Scenes in Build Settings/Open Editor", false, 41)]
        private static void Open()
        {
            BuilderSetting window = GetWindow<BuilderSetting>("BuilderSetting Editor", true);
            window.minSize = new Vector2(1400f, 600f);
        }

        /// <summary>
        /// 
        /// </summary>
        BuildSettingsController m_Controller = null;

        /// <summary>
        /// 
        /// </summary>
        ReorderableList m_defaultScenesList;

        /// <summary>
        /// 
        /// </summary>
        ReorderableList m_SearchScenePathsList;

        /// <summary>
        /// 
        /// </summary>
        OpenPanelOption m_openPanelOption;

        /// <summary>
        /// 选择面板类型
        /// </summary>
        enum emOpenPanelType
        {
            None,
            DefaultScene,
            SearchScenePath,
        }

        /// <summary>
        /// 选择面板操作
        /// </summary>
        struct OpenPanelOption
        {
            public static readonly OpenPanelOption s_Defalut = new OpenPanelOption { index = -1, type = emOpenPanelType.None };
            public int index;
            public emOpenPanelType type;
            public bool Valid()
            {
                return type != emOpenPanelType.None && index >= 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            m_Controller = new BuildSettingsController();
            if(m_Controller.Load())
            {
                Debug.Log("Load configuration success.");
            }
            else
            {
                Debug.LogWarning("Load configuration failure.");
            }

            m_defaultScenesList = new ReorderableList(m_Controller.defaultScenes, typeof(string));
            m_defaultScenesList.drawHeaderCallback = DrawDefaultScenesHeader;
            m_defaultScenesList.drawElementCallback = DrawDefaultScenesElement;
            m_SearchScenePathsList = new ReorderableList(m_Controller.searchScenePaths, typeof(string));
            m_SearchScenePathsList.drawHeaderCallback = DrawSearchScenePathsHeader;
            m_SearchScenePathsList.drawElementCallback = DrawSearchScenePathsElement;

            m_openPanelOption = OpenPanelOption.s_Defalut;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            if(null != m_Controller)
            {
                m_defaultScenesList.DoLayoutList();
                m_SearchScenePathsList.DoLayoutList();
                ProcessOpenPanelOption();
            }
            if(GUILayout.Button("Save Configuration"))
            {
                SaveConfiguration();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SaveConfiguration()
        {
            if (m_Controller.Save())
            {
                Debug.Log("Save configuration success.");
            }
            else
            {
                Debug.LogWarning("Save configuration failure.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        void DrawDefaultScenesHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Default Scenes");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="selected"></param>
        /// <param name="focused"></param>
        void DrawDefaultScenesElement(Rect rect, int index, bool selected, bool focused)
        {
            if(!string.IsNullOrEmpty(m_Controller.defaultScenes[index]))
            {
                EditorGUI.LabelField(rect, m_Controller.defaultScenes[index]);
            }
            else
            {
                if (GUI.Button(rect, new GUIContent("please select a scene")))
                {
                    m_openPanelOption.index = index;
                    m_openPanelOption.type = emOpenPanelType.DefaultScene;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        void DrawSearchScenePathsHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Search Scene Paths");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="selected"></param>
        /// <param name="focused"></param>
        void DrawSearchScenePathsElement(Rect rect, int index, bool selected, bool focused)
        {
            if (!string.IsNullOrEmpty(m_Controller.searchScenePaths[index]))
            {
                EditorGUI.LabelField(rect, m_Controller.searchScenePaths[index]);
            }
            else
            {
                if (GUI.Button(rect, new GUIContent("please select a search scene folder")))
                {
                    m_openPanelOption.index = index;
                    m_openPanelOption.type = emOpenPanelType.SearchScenePath;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ProcessOpenPanelOption()
        {
            if (m_openPanelOption.Valid())
            {
                switch (m_openPanelOption.type)
                {
                    case emOpenPanelType.DefaultScene:
                        {
                            string path = EditorUtility.OpenFilePanel("select a scene", Application.dataPath, "unity");
                            if (!string.IsNullOrEmpty(path))
                            {

                                if (m_openPanelOption.index < m_Controller.defaultScenes.Count)
                                {
                                    m_Controller.defaultScenes[m_openPanelOption.index] = path.Substring(path.IndexOf("Asset"));
                                }
                            }
                        }
                        break;
                    case emOpenPanelType.SearchScenePath:
                        {
                            string path = EditorUtility.OpenFolderPanel("select a folder", Application.dataPath, Application.dataPath);
                            if (!string.IsNullOrEmpty(path))
                            {

                                if (m_openPanelOption.index < m_Controller.searchScenePaths.Count)
                                {
                                    m_Controller.searchScenePaths[m_openPanelOption.index] = path.Substring(path.IndexOf("Asset"));
                                }
                            }
                        }
                        break;
                }
                m_openPanelOption = OpenPanelOption.s_Defalut;
            }
        }
    }
}
