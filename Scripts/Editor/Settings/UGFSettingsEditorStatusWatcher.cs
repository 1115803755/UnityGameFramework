/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:25:45
* Note  : 编辑器状态监听
***************************************************************/
using System;
using UnityEditor;
using UnityEditorInternal;

namespace UnityGameFramework.Editor.Settings
{
    /// <summary>
    /// 监听编辑器状态，当编辑器重新 focus 时，重新加载实例，避免某些情景下 svn 、git 等外部修改了数据却无法同步的异常。
    /// </summary>
    [InitializeOnLoad]
    public static class UGFSettingsEditorStatusWatcher
    {
        /// <summary>
        /// 
        /// </summary>
        public static Action OnEditorFocused;

        /// <summary>
        /// 
        /// </summary>
        static bool isFocused;

        /// <summary>
        /// 
        /// </summary>
        static UGFSettingsEditorStatusWatcher() => EditorApplication.update += Update;

        /// <summary>
        /// 
        /// </summary>
        static void Update()
        {
            if (isFocused != InternalEditorUtility.isApplicationActive)
            {
                isFocused = InternalEditorUtility.isApplicationActive;
                if (isFocused)
                {
                    UGFSettings.LoadOrCreate();
                    OnEditorFocused?.Invoke();
                }
            }
        }
    }
}
