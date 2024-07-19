/***************************************************************
* Author: HuangXiaoDong
* Data  : 2024/07/18 19:35:22
* Note  : 框架配置菜单
***************************************************************/
using UnityEditor;

namespace UnityGameFramework.Editor.Settings
{
    public static class MenuProvider
    {
        [MenuItem("Game Framework/Settings...", priority = 61)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/UGFSettings");
    }
}

